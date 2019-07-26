using System;
using System.Web;

namespace Unity.WebForms
{
	/// <summary>Collection of extension methods for the <see cref="HttpApplicationState" /> class.</summary>
	public static class SystemWebUnityContainerExtensions
	{
		/// <summary>Key used for locating the Unity container in the Http Application state.</summary>
		private const String RootContainerKey = "EntLibContainer";

		/// <summary>Key used for locating the Child Unity container used for resolution during the current request.</summary>
		private const String RequestContainerKey = "EntLibChildContainer";

		/// <summary>Object used for locking to prevent threading issues.</summary>
		private static readonly Object _thisLock = new Object();

		/// <summary>Gets the container instance out of application state. Throws <see cref="InvalidOperationException"/> if the root container has not yet been set.</summary>
		/// <param name="httpApplication">The Global.asax instance.</param>
		/// <returns>The Unity container instance.</returns>
		public static IUnityContainer GetApplicationContainer( this HttpApplication httpApplication )
		{
			IUnityContainer rootContainer = httpApplication.Application[ RootContainerKey ] as IUnityContainer;
			if( rootContainer == null ) throw new InvalidOperationException( "The root container has not been set for this HttpApplicationState." );
			return rootContainer;
		}

		/// <summary>Stores a Unity container instance into application state. This method does not normally need to called from web-application code but is exposed if you wish to override the container for a particular <see cref="HttpApplication"/> instance.</summary>
		/// <param name="httpApplication">The Global.asax instance.</param>
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

		/// <summary>Attempts to get the per-request child <see cref="IUnityContainer"/> from the provided <see cref="HttpContext"/>. Returns <c>false</c> if the child container does not exist (ignore the <paramref name="childContainer"/> parameter value). Returns <c>true</c> if it was found (and will be returned via <paramref name="childContainer"/>).</summary>
		public static Boolean TryGetChildContainer( this HttpContext context, out IUnityContainer childContainer )
		{
			childContainer = context.Items[ RequestContainerKey ] as IUnityContainer;
			return childContainer != null;
		}

		/// <summary>Attempts to get the per-request child <see cref="IUnityContainer"/> from the provided <see cref="HttpContext"/>. Returns <c>false</c> if the child container does not exist (ignore the <paramref name="childContainer"/> parameter value). Returns <c>true</c> if it was found (and will be returned via <paramref name="childContainer"/>).</summary>
		public static Boolean TryGetChildContainer( this HttpContextBase context, out IUnityContainer childContainer )
		{
			childContainer = context.Items[ RequestContainerKey ] as IUnityContainer;
			return childContainer != null;
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

		/// <summary>Gets the child container instance out of request state. Throws <see cref="InvalidOperationException"/> if the root container has not yet been set.</summary>
		/// <param name="context">The current request context.</param>
		/// <returns>The child Unity container reference.</returns>
		public static IUnityContainer GetChildContainer( this HttpContextBase context )
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

		/// <summary>Stores the child Unity instance into request state. This method does not normally need to called from web-application code but is exposed if you wish to override the container for a particular request.</summary>
		/// <param name="context">The request context.</param>
		/// <param name="container">The child container instance.</param>
		public static void SetChildContainer( this HttpContextBase context, IUnityContainer container )
		{
			lock( _thisLock )
			{
				context.Items[RequestContainerKey] = container;
			}
		}
	}
}
