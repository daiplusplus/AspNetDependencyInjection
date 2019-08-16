using System;
using System.Threading;
using System.Web;
using System.Web.Hosting;

using Microsoft.Extensions.DependencyInjection;

using AspNetDependencyInjection.Internal;

namespace AspNetDependencyInjection
{
	/// <summary>Controls the lifespan of the configured <see cref="IServiceCollection"/>. This class implements <see cref="IRegisteredObject"/> to ensure the root <see cref="IServiceProvider"/> is disposed when the <see cref="HostingEnvironment"/> shuts down. Only 1 instance of this class can exist at a time in a single AppDomain.</summary>
	public class ApplicationDependencyInjection : IDisposable, IRegisteredObject
	{
		private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim( initialCount: 1, maxCount: 1 );

		private readonly IServiceCollection services;
		private readonly ServiceProvider    rootServiceProvider;

		/// <summary>Exposes <see cref="ImmutableApplicationDependencyInjectionConfiguration"/>.</summary>
		protected internal ImmutableApplicationDependencyInjectionConfiguration Configuration      { get; }

		/// <summary>Exposes <see cref="DependencyInjectionWebObjectActivator"/>.</summary>
		protected internal DependencyInjectionWebObjectActivator                WebObjectActivator { get; }

		/// <summary>Indicates if this <see cref="ApplicationDependencyInjection"/> instance has already been disposed.</summary>
		protected internal Boolean                                              IsDisposed { get; private set; }

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

			// Register necessary internal services:

			services.TryAddDefaultAspNetExclusions();

			return new ApplicationDependencyInjection( configuration, services );
		}

		/// <summary>Constructor for subclasses. When the constructor returns all static/global state will have been set (e.g. <see cref="HttpRuntime"/>, <see cref="HostingEnvironment"/> and dynamic modules added).</summary>
		protected ApplicationDependencyInjection( ApplicationDependencyInjectionConfiguration configuration, IServiceCollection services )
		{
			// Validate:

			if( !_semaphore.Wait( millisecondsTimeout: 0 ) )
			{
				throw new InvalidOperationException( "Another " + nameof(ApplicationDependencyInjection) + " has already been created in this AppDomain without being disposed first (or the previous dispose attempt failed)." );
			}

			{
				// `HttpRuntime.WebObjectActivator == null` by default. I see no point to caching-and-restoring any existing WebObjectActivator given AspNetDependencyInjection is supposed to be *exclusive* and own an entire application's DI, so require that no other existing WebObjectActivator be set.
				IServiceProvider existingWoa = HttpRuntime.WebObjectActivator;
				if( existingWoa != null )
				{
					throw new InvalidOperationException( "Another " + nameof(HttpRuntime) + "." + nameof(HttpRuntime.WebObjectActivator) + " has been set. Its type is " + existingWoa.GetType().FullName + "." );
				}
			}

			this.Configuration = configuration.ToImmutable();

			services.AddSingleton<IServiceProviderAccessor>( sp => new AspNetDependencyInjection.Services.DefaultServiceProviderAccessor( this.Configuration, sp ) );

			// Initialize fields:

			this.services            = services ?? throw new ArgumentNullException(nameof(services));
			this.rootServiceProvider = services.BuildServiceProvider( validateScopes: true );
			this.WebObjectActivator  = new DependencyInjectionWebObjectActivator( this.Configuration, this.rootServiceProvider, fallbackService: this.rootServiceProvider.GetRequiredService<IDependencyInjectionFallbackService>() );

			// And register:

			HttpRuntime.WebObjectActivator = this.WebObjectActivator;
			HostingEnvironment.RegisterObject( this );
			global::Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule( typeof( HttpContextScopeHttpModule ) ); // TODO: Can we un-register the module? Would we ever want to?
		}

		/// <summary>Call this method from a <see cref="WebActivatorEx.PostApplicationStartMethodAttribute"/> or other method (after your original <see cref="WebActivatorEx.PreApplicationStartMethodAttribute"/>-marked) to register additional services or reconfigure existing services if you need to perform additional service registration after your Global.asax has initialized.</summary>
		public void Reconfigure( Action<IServiceCollection> reconfigureServices )
		{
			throw new NotImplementedException();
		}

		/// <summary>Calls <see cref="Dispose()"/>. This method is called by <see cref="HostingEnvironment"/>.</summary>
		/// <param name="immediate">This parameter is unused.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
		void IRegisteredObject.Stop(Boolean immediate)
		{
			this.Dispose();
		}

		/// <summary>Consuming applications should not need to call this method directly as it is called by <see cref="HostingEnvironment"/>. This method calls <see cref="HostingEnvironment.UnregisterObject(IRegisteredObject)"/>, un-sets the <see cref="DependencyInjectionWebObjectActivator"/> from <see cref="HttpRuntime.WebObjectActivator"/> and restores its original value, and calls the <see cref="IDisposable.Dispose"/> method of the root <see cref="ServiceProvider"/>.</summary>
		public void Dispose()
		{
			this.Dispose( disposing: true );
			GC.SuppressFinalize( this );
		}

		/// <summary>See <see cref="IDisposable.Dispose"/>.</summary>
		/// <param name="disposing">When <c>true</c>, the <see cref="Dispose()"/> method was called. When <c>false</c> the finalizer was invoked.</param>
		protected virtual void Dispose( Boolean disposing )
		{
			if( this.IsDisposed ) return;

			if( disposing )
			{
				HostingEnvironment.UnregisterObject(this);

				if( HttpRuntime.WebObjectActivator == this.WebObjectActivator )
				{
					HttpRuntime.WebObjectActivator = null;
				}

				this.rootServiceProvider.Dispose();

				_semaphore.Release();
			}

			this.IsDisposed = true;
		}
	}
}
