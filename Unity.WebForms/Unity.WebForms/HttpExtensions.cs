using System;
using System.Web;

namespace Unity.WebForms
{
	/// <summary>Collection of extension methods for the <see cref="HttpApplicationState" /> class.</summary>
	public static class HttpExtensions
	{
		/// <summary>Key used for locating the Unity container in the Http Application state.</summary>
		private const String GlobalContainerKey = "EntLibContainer";

		/// <summary>Key used for locating the Child Unity container used for resolution during the current request.</summary>
		private const String RequestContainerKey = "EntLibChildContainer";

		/// <summary>Object used for locking to prevent threading issues.</summary>
		private static readonly Object _thisLock = new Object();

		/// <summary>Gets the container instance out of application state, creating it if necessary.</summary>
		/// <param name="appState">The application state instance.</param>
		/// <returns>The Unity container instance.</returns>
		public static IUnityContainer GetContainer( this HttpApplicationState appState )
		{
			IUnityContainer myContainer = appState[GlobalContainerKey] as IUnityContainer;

			try
			{
				if( myContainer == null )
				{
					appState.Lock();

					myContainer = new UnityContainer();
					appState[GlobalContainerKey] = myContainer;
				}
			}
			finally
			{
				appState.UnLock();
			}

			return myContainer;
		}

		/// <summary>Stores a Unity container instance into application state.</summary>
		/// <param name="appState">The application state instance.</param>
		/// <param name="container">The Unity container instance to store.</param>
		public static void SetContainer( this HttpApplicationState appState, IUnityContainer container )
		{
			appState.Lock();

			try
			{
				appState[GlobalContainerKey] = container;
			}
			finally
			{
				appState.UnLock();
			}
		}

		/// <summary>Gets the child container instance out of request state, creating it if necessary.</summary>
		/// <param name="context">The current request context.</param>
		/// <returns>The child Unity container reference.</returns>
		public static IUnityContainer GetChildContainer( this HttpContext context )
		{
			IUnityContainer childContainer = context.Items[RequestContainerKey] as IUnityContainer;

			if( childContainer == null )
			{
				lock( _thisLock )
				{
					childContainer = GetContainer( context.Application ).CreateChildContainer();
					context.SetChildContainer( childContainer );
				}
			}

			return childContainer;
		}

		/// <summary>Stores the child Unity instance into request state.</summary>
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
