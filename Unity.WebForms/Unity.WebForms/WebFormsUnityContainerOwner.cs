using System;
using System.Web.Hosting;

namespace Unity.WebForms
{
	/// <summary>Controls the lifespan of the provided <see cref="IUnityContainer"/> (or creates a new <see cref="IUnityContainer"/>). This class implements <see cref="IRegisteredObject"/> to ensure the container is disposed when the <see cref="HostingEnvironment"/> shuts down.</summary>
	public sealed class WebFormsUnityContainerOwner : IDisposable, IRegisteredObject
	{
		private UnityContainerServiceProvider ucsp;
		private Boolean                       isDisposed;

		public WebFormsUnityContainerOwner( IUnityContainer applicationContainer )
		{
			this.Container = applicationContainer ?? throw new ArgumentNullException(nameof(applicationContainer));

			HostingEnvironment.RegisterObject( this );
		}

		public IUnityContainer Container { get; } // TODO: Add Disposed check to this property getter?

		public void Install()
		{
			if( this.isDisposed ) throw new ObjectDisposedException( objectName: nameof(WebFormsUnityContainerOwner) );

			StaticWebFormsUnityContainerOwner.RootContainer = this.Container;
			this.ucsp = UnityContainerServiceProvider.SetWebObjectActivatorContainer( this.Container );
		}

		public void Stop( Boolean immediate )
		{
			if( this.isDisposed ) throw new ObjectDisposedException( objectName: nameof(WebFormsUnityContainerOwner) );
			if( this.ucsp == null ) throw new InvalidOperationException( "This instance has not yet installed its container." );

			HostingEnvironment.UnregisterObject( this );
			this.ucsp.RemoveAsWebObjectActivator();
			StaticWebFormsUnityContainerOwner.RootContainer = null;

			this.Dispose();
		}

		public void Dispose()
		{
			this.isDisposed = true;

			this.Container.Dispose();
		}
	}

	internal static class StaticWebFormsUnityContainerOwner
	{
		/// <summary>The single root, application-level container that is associated with each <see cref="System.Web.HttpApplication"/> instance and <see cref="System.Web.HttpRuntime.WebObjectActivator"/>.</summary>
		public static IUnityContainer RootContainer { get; set; }
	}
}
