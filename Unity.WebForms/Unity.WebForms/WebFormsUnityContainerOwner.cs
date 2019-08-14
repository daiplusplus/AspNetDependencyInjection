using System;
using System.Threading;
using System.Web;
using System.Web.Hosting;

using Microsoft.Extensions.DependencyInjection;

using Unity.WebForms.Internal;

namespace Unity.WebForms
{
	/// <summary>Controls the lifespan of the configured <see cref="IServiceCollection"/>. This class implements <see cref="IRegisteredObject"/> to ensure the container is disposed when the <see cref="HostingEnvironment"/> shuts down. Only 1 instance of this class can exist at a time in a single AppDomain.</summary>
	public class WebFormsUnityContainerOwner : IDisposable, IRegisteredObject
	{
		private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim( initialCount: 1, maxCount: 1 );

		private readonly IServiceCollection services;
		private readonly ServiceProvider    rootServiceProvider;
		private readonly IServiceProvider   previousWoa;
		private readonly MediWebObjectActivatorServiceProvider ucsp;

		/// <summary>Call this method from a <see cref="WebActivatorEx.PreApplicationStartMethodAttribute"/>-marked method to instantiate a new <see cref="WebFormsUnityContainerOwner"/> and to configure services in the root service provider.</summary>
		public static WebFormsUnityContainerOwner Configure( Action<IServiceCollection> configureServices )
		{
			if( configureServices == null ) throw new ArgumentNullException(nameof(configureServices));

			ServiceCollection services = new ServiceCollection();
			configureServices( services );

			ServiceProvider rootServiceProvider = services.BuildServiceProvider( validateScopes: true );

			return new WebFormsUnityContainerOwner( applicationServiceProvider: rootServiceProvider );
		}

		/// <summary>Not intended to be called from consuming application code unless you have prepared your own <see cref="IServiceProvider"/>. Constructs a new instance of <see cref="WebFormsUnityContainerOwner"/>. Registers this instance with <see cref="HostingEnvironment.RegisterObject(IRegisteredObject)"/> and sets the provided container as the <see cref="System.Web.HttpRuntime.WebObjectActivator"/> using <see cref="UnityContainerServiceProvider.SetWebObjectActivatorContainer(IUnityContainer)"/>.</summary>
		/// <param name="applicationServiceProvider">Required. Throws <see cref="ArgumentNullException"/> if <c>null</c>.</param>
		protected WebFormsUnityContainerOwner( IServiceProvider applicationServiceProvider )
		{
			if( !_semaphore.Wait( millisecondsTimeout: 0 ) )
			{
				throw new InvalidOperationException( "Another " + nameof(WebFormsUnityContainerOwner) + " has already been created." );
			}

			//

			services.AddSingleton<Unity.WebForms.Services.IServiceProviderAccessor>( sp => new Unity.WebForms.Services.DefaultServiceProviderAccessor( sp ) );

			this.previousWoa = HttpRuntime.WebObjectActivator;

			HttpRuntime.WebObjectActivator = this.ucsp = new MediWebObjectActivatorServiceProvider( applicationServiceProvider, this.previousWoa, onUnresolvedType: null );

			HostingEnvironment.RegisterObject( this );

			global::Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule( typeof( UnityHttpModule ) ); // TODO: Can we un-register the module?
		}

		/// <summary>Returns the <see cref="IServiceProvider"/> that was used to construct this <see cref="WebFormsUnityContainerOwner"/>.</summary>
		public IServiceProvider ApplicationServiceProvider { get; }

		/// <summary>Calls <see cref="Dispose"/>.</summary>
		/// <param name="immediate">This parameter is unused.</param>
		void IRegisteredObject.Stop(Boolean immediate)
		{
			this.Dispose();
		}

		/// <summary>Calls <see cref="HostingEnvironment.UnregisterObject(IRegisteredObject)"/>, un-sets the <see cref="MediWebObjectActivatorServiceProvider"/> from <see cref="HttpRuntime.WebObjectActivator"/> and restores its original value, and calls the <see cref="IDisposable.Dispose"/> method of the root <see cref="ServiceProvider"/>.</summary>
		public void Dispose()
		{
			HostingEnvironment.UnregisterObject(this);

			if( HttpRuntime.WebObjectActivator == this.ucsp )
			{
				HttpRuntime.WebObjectActivator = this.previousWoa;
			}

			this.rootServiceProvider.Dispose();

			_semaphore.Release();
		}
	}
}
