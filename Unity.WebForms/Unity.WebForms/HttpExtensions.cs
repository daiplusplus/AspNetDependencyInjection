using System;
using System.Web;

namespace Unity.WebForms
{
	/// <summary>Collection of extension methods for the <see cref="HttpApplicationState" /> class.</summary>
	public static class HttpExtensions
	{
		/// <summary>Key used for locating the Unity container in the Http Application state.</summary>
		private const String RootContainerKey = "EntLibContainer";

		/// <summary>Key used for locating the Child Unity container used for resolution during the current request.</summary>
		private const String RequestContainerKey = "EntLibChildContainer";

		/// <summary>Object used for locking to prevent threading issues.</summary>
		private static readonly Object _thisLock = new Object();

		/// <summary>Gets the container instance out of application state. Throws <see cref="InvalidOperationException"/> if the root container has not yet been set.</summary>
		/// <param name="appState">The application state instance.</param>
		/// <returns>The Unity container instance.</returns>
		public static IUnityContainer GetApplicationContainer( this HttpApplication httpApplication )
		{
			IUnityContainer rootContainer = httpApplication.Application[ RootContainerKey ] as IUnityContainer;
			if( rootContainer == null ) throw new InvalidOperationException( "The root container has not been set for this HttpApplicationState." );
			return rootContainer;
		}

		/// <summary>Stores a Unity container instance into application state. This method does not normally need to called from web-application code but is exposed if you wish to override the container for a particular <see cref="HttpApplication"/> instance.</summary>
		/// <param name="appState">The application state instance.</param>
		/// <param name="container">The Unity container instance to store.</param>
		public static void SetApplicationContainer( this HttpApplication httpApplication, IUnityContainer container )
		{
			httpApplication.Application.Lock();

			try
			{
				httpApplication.Application[RootContainerKey] = container;
			}
			finally
			{
				httpApplication.Application.UnLock();
			}
		}

		/// <summary>Gets the child container instance out of request state. Throws <see cref="InvalidOperationException"/> if the root container has not yet been set.</summary>
		/// <param name="context">The current request context.</param>
		/// <returns>The child Unity container reference.</returns>
		public static IUnityContainer GetChildContainer( this HttpContext context )
		{
			IUnityContainer childContainer = context.Items[ RequestContainerKey ] as IUnityContainer;
			if( childContainer == null ) throw new InvalidOperationException( "The child container has not been set for this HttpContext." );
			return childContainer;
		}

		/// <summary>Stores the child Unity instance into request state. This method does not normally need to called from web-application code but is exposed if you wish to override the container for a particular request.</summary>
		/// <param name="context">The request context.</param>
		/// <param name="container">The child container instance.</param>
		public static void SetChildContainer( this HttpContext context, IUnityContainer container )
		{
			lock( _thisLock )
			{
				context.Items[RequestContainerKey] = container;
			}
		}
	}
}
