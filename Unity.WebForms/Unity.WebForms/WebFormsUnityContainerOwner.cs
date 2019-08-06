using System;
using System.Web;
using System.Web.Hosting;

namespace Unity.WebForms
{
	/// <summary>Controls the lifespan of the provided <see cref="IUnityContainer"/> (or creates a new <see cref="IUnityContainer"/>). This class implements <see cref="IRegisteredObject"/> to ensure the container is disposed when the <see cref="HostingEnvironment"/> shuts down.</summary>
	public sealed class WebFormsUnityContainerOwner : IDisposable, IRegisteredObject
	{
		private readonly UnityContainerServiceProvider ucsp;

		/// <summary>Constructs a new instance of <see cref="WebFormsUnityContainerOwner"/>. Registers this instance with <see cref="HostingEnvironment.RegisterObject(IRegisteredObject)"/> and sets the provided container as the <see cref="System.Web.HttpRuntime.WebObjectActivator"/> using <see cref="UnityContainerServiceProvider.SetWebObjectActivatorContainer(IUnityContainer)"/>.</summary>
		/// <param name="applicationContainer">Required. Throws <see cref="ArgumentNullException"/> if <c>null</c>.</param>
		public WebFormsUnityContainerOwner( IUnityContainer applicationContainer )
		{
			this.Container = applicationContainer ?? throw new ArgumentNullException(nameof(applicationContainer));

			// Before continuing, ensure that IChildContainerConfiguration is registered, and if not, add our own dummy implementation:
			if( !applicationContainer.IsRegistered<IChildContainerConfiguration>() )
			{
				applicationContainer.RegisterSingleton<IChildContainerConfiguration,DefaultChildContainerConfiguration>();
			}

			HostingEnvironment.RegisterObject( this );

			StaticWebFormsUnityContainerOwner.RootContainer = this.Container;
			this.ucsp = UnityContainerServiceProvider.SetWebObjectActivatorContainer(this.Container);
		}

		/// <summary>Returns the container that was used to construct this <see cref="WebFormsUnityContainerOwner"/>.</summary>
		public IUnityContainer Container { get; }

		/// <summary>Calls <see cref="Dispose"/>.</summary>
		/// <param name="immediate">This parameter is unused.</param>
		void IRegisteredObject.Stop(Boolean immediate)
		{
			this.Dispose();
		}

		/// <summary>Calls <see cref="HostingEnvironment.UnregisterObject(IRegisteredObject)"/>, removes <see cref="Container"/> as <see cref="HttpRuntime.WebObjectActivator"/> and calls the <see cref="IDisposable.Dispose"/> method of <see cref="Container"/>.</summary>
		public void Dispose()
		{
			HostingEnvironment.UnregisterObject(this);

			if( StaticWebFormsUnityContainerOwner.RootContainer == this.Container )
			{
				this.ucsp.RemoveAsWebObjectActivator();
				StaticWebFormsUnityContainerOwner.RootContainer = null;
			}

			this.Container.Dispose();
		}
	}

	internal static class StaticWebFormsUnityContainerOwner
	{
		/// <summary>The single root, application-level container that is associated with each <see cref="System.Web.HttpApplication"/> instance and <see cref="System.Web.HttpRuntime.WebObjectActivator"/>.</summary>
		public static IUnityContainer RootContainer { get; set; }
	}
}
