using System;
using System.Threading;
using System.Web;
using System.Web.Hosting;

using Microsoft.Extensions.DependencyInjection;

using AspNetDependencyInjection.Internal;

namespace AspNetDependencyInjection
{
	/// <summary>Controls the lifespan of the configured <see cref="IServiceCollection"/>. This class implements <see cref="IRegisteredObject"/> to ensure the root <see cref="IServiceProvider"/> is disposed when the <see cref="HostingEnvironment"/> shuts down. Only 1 instance of this class can exist at a time in a single AppDomain.</summary>
	public sealed class ApplicationDependencyInjection : IDisposable, IRegisteredObject
	{
		private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim( initialCount: 1, maxCount: 1 );

		private readonly IServiceCollection                    services;
		private readonly ServiceProvider                       rootServiceProvider;
		private readonly IServiceProvider                      previousWoa;
		private readonly DependencyInjectionWebObjectActivator woa;

		internal Boolean UseRequestScopes         { get; }
		internal Boolean UseHttpApplicationScopes { get; }

		/// <summary>Call this method from a <see cref="WebActivatorEx.PreApplicationStartMethodAttribute"/>-marked method to instantiate a new <see cref="ApplicationDependencyInjection"/> and to configure services in the root service provider.</summary>
		public static ApplicationDependencyInjection Configure( Action<IServiceCollection> configureServices )
		{
			return Configure( new ApplicationDependencyInjectionConfiguration(), configureServices );
		}

		/// <summary>Call this method from a <see cref="WebActivatorEx.PreApplicationStartMethodAttribute"/>-marked method to instantiate a new <see cref="ApplicationDependencyInjection"/> and to configure services in the root service provider.</summary>
		public static ApplicationDependencyInjection Configure( ApplicationDependencyInjectionConfiguration configuration, Action<IServiceCollection> configureServices )
		{
			if( configuration     == null ) throw new ArgumentNullException(nameof(configuration));
			if( configureServices == null ) throw new ArgumentNullException(nameof(configureServices));

			ServiceCollection services = new ServiceCollection();
			configureServices( services );

			return new ApplicationDependencyInjection( configuration, services );
		}

		private ApplicationDependencyInjection( ApplicationDependencyInjectionConfiguration configuration, IServiceCollection services )
		{
			if( !_semaphore.Wait( millisecondsTimeout: 0 ) )
			{
				throw new InvalidOperationException( "Another " + nameof(ApplicationDependencyInjection) + " has already been created in this AppDomain without being disposed first (or the previous dispose attempt failed)." );
			}

			this.UseRequestScopes         = configuration.UseRequestScopes;
			this.UseHttpApplicationScopes = configuration.UseHttpApplicationScopes;

			// Register necessary internal services:

			services.TryAddDefaultAspNetExclusions();
			services.AddSingleton<IServiceProviderAccessor>( sp => new AspNetDependencyInjection.Services.DefaultServiceProviderAccessor( this, sp ) );

			// Initialize fields:

			this.services            = services ?? throw new ArgumentNullException(nameof(services));
			this.rootServiceProvider = services.BuildServiceProvider( validateScopes: true );
			this.previousWoa         = HttpRuntime.WebObjectActivator;
			this.woa                = new DependencyInjectionWebObjectActivator( this.rootServiceProvider, this.previousWoa, excluded: this.rootServiceProvider.GetRequiredService<IDependencyInjectionExclusionService>() );

			// And register:

			HttpRuntime.WebObjectActivator = this.woa;
			HostingEnvironment.RegisterObject( this );
			global::Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule( typeof( HttpContextScopeHttpModule ) ); // TODO: Can we un-register the module? Would we ever want to?
		}

		/// <summary>Call this method from a <see cref="WebActivatorEx.PostApplicationStartMethodAttribute"/> or other method (after your original <see cref="WebActivatorEx.PreApplicationStartMethodAttribute"/>-marked) to register additional services or reconfigure existing services if you need to perform additional service registration after your Global.asax has initialized.</summary>
		public void Reconfigure( Action<IServiceCollection> reconfigureServices )
		{
			throw new NotImplementedException();
		}

		/// <summary>Calls <see cref="Dispose"/>. This method is called by <see cref="HostingEnvironment"/>.</summary>
		/// <param name="immediate">This parameter is unused.</param>
		void IRegisteredObject.Stop(Boolean immediate)
		{
			this.Dispose();
		}

		/// <summary>Consuming applications should not need to call this method directly as it is called by <see cref="HostingEnvironment"/>. This method calls <see cref="HostingEnvironment.UnregisterObject(IRegisteredObject)"/>, un-sets the <see cref="DependencyInjectionWebObjectActivator"/> from <see cref="HttpRuntime.WebObjectActivator"/> and restores its original value, and calls the <see cref="IDisposable.Dispose"/> method of the root <see cref="ServiceProvider"/>.</summary>
		public void Dispose()
		{
			HostingEnvironment.UnregisterObject(this);

			if( HttpRuntime.WebObjectActivator == this.woa )
			{
				HttpRuntime.WebObjectActivator = this.previousWoa;
			}

			this.rootServiceProvider.Dispose();

			_semaphore.Release();
		}
	}
}
