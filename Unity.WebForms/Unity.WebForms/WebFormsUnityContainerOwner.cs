using System;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Unity.WebForms.Internal;

namespace Unity.WebForms
{
	/// <summary>Controls the lifespan of the provided <see cref="IUnityContainer"/> (or creates a new <see cref="IUnityContainer"/>). This class implements <see cref="IRegisteredObject"/> to ensure the container is disposed when the <see cref="HostingEnvironment"/> shuts down.</summary>
	public class WebFormsUnityContainerOwner : IDisposable, IRegisteredObject
	{
		private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim( initialCount: 1, maxCount: 1 );

		private readonly IServiceProvider previousWoa;

		private readonly MediWebObjectActivatorServiceProvider ucsp;

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

			this.ApplicationServiceProvider = applicationServiceProvider ?? throw new ArgumentNullException(nameof(applicationServiceProvider));

			StaticWebFormsUnityContainerOwner.RootServiceProvider = applicationServiceProvider;

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

		/// <summary>Calls <see cref="HostingEnvironment.UnregisterObject(IRegisteredObject)"/>, un-sets the <see cref="MediWebObjectActivatorServiceProvider"/> from <see cref="HttpRuntime.WebObjectActivator"/> and restores its original value, and calls the <see cref="IDisposable.Dispose"/> method of <see cref="ApplicationServiceProvider"/>.</summary>
		public void Dispose()
		{
			HostingEnvironment.UnregisterObject(this);

			if( StaticWebFormsUnityContainerOwner.RootServiceProvider == this.ApplicationServiceProvider )
			{
				StaticWebFormsUnityContainerOwner.RootServiceProvider = null;
			}

			HttpRuntime.WebObjectActivator = this.previousWoa;

			if( this.ApplicationServiceProvider is IDisposable disposable )
			{
				disposable.Dispose();
			}

			_semaphore.Release();
		}
	}

	internal static class StaticWebFormsUnityContainerOwner
	{
		/// <summary>The single root, application-level container that is associated with each <see cref="System.Web.HttpApplication"/> instance and <see cref="System.Web.HttpRuntime.WebObjectActivator"/>.</summary>
		public static IServiceProvider RootServiceProvider { get; set; }
	}
}
