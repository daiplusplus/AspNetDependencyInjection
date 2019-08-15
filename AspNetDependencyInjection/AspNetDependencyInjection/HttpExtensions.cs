using System;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>Collection of extension methods for the <see cref="HttpApplicationState" /> class.</summary>
	public static class HttpContextExtensions
	{
		/// <summary>Key used for locating the <see cref="IServiceProvider"/> in the <see cref="HttpApplication"/>'s <see cref="HttpApplicationState"/> collection.</summary>
		private const String ApplicationServiceProviderKey = "EntLibContainer";

		/// <summary>Key used for locating the child <see cref="IServiceScope"/> used for resolution during the current HTTP request.</summary>
		private const String RequestServiceProviderKey = "EntLibChildContainer";

		/// <summary>Object used for locking to prevent threading issues. (TODO: Is this really necessary?)</summary>
		private static readonly Object _thisLock = new Object();

		/// <summary>Gets the <see cref="IServiceProvider"/> instance out of application state. Returns <c>false</c> if the <see cref="HttpApplication"/>'s <see cref="HttpApplicationState"/> service-provider has not yet been set.</summary>
		/// <param name="httpApplication">The Global.asax instance.</param>
		/// <param name="applicationServiceProvider"></param>
		public static Boolean TryGetApplicationServiceProvider( this HttpApplication httpApplication, out IServiceProvider applicationServiceProvider )
		{
			applicationServiceProvider = httpApplication.Application[ ApplicationServiceProviderKey ] as IServiceProvider;
			return applicationServiceProvider != null;
		}

		/// <summary>Gets the <see cref="IServiceProvider"/> instance out of application state. Throws <see cref="InvalidOperationException"/> if the <see cref="HttpApplication"/>'s <see cref="HttpApplicationState"/> service-provider has not yet been set.</summary>
		/// <param name="httpApplication">The Global.asax instance.</param>
		/// <returns>The <see cref="IServiceProvider"/> instance.</returns>
		public static IServiceProvider GetApplicationServiceProvider( this HttpApplication httpApplication )
		{
			IServiceProvider serviceProvider = httpApplication.Application[ ApplicationServiceProviderKey ] as IServiceProvider;
			if( serviceProvider is null ) throw new InvalidOperationException( "The root " + nameof(IServiceProvider) + " has not been set for this " + nameof(HttpApplication) + "." );
			return serviceProvider;
		}

		/// <summary>Stores a <see cref="IServiceScope"/> instance into application state. This method does not normally need to called from web-application code but is exposed if you wish to override the container for a particular <see cref="HttpApplication"/> instance.</summary>
		/// <param name="httpApplication">The Global.asax instance.</param>
		/// <param name="newApplicationServiceProvider">The <see cref="IServiceProvider"/> to store inside <paramref name="httpApplication"/>'s <see cref="HttpApplicationState"/>.</param>
		public static void SetApplicationServiceProvider( this HttpApplication httpApplication, IServiceProvider newApplicationServiceProvider )
		{
			httpApplication.Application.Lock();

			try
			{
				httpApplication.Application[ApplicationServiceProviderKey] = newApplicationServiceProvider;
			}
			finally
			{
				httpApplication.Application.UnLock();
			}
		}

		/// <summary>Attempts to get the per-request <see cref="IServiceScope"/> from the provided <see cref="HttpContext"/>. Returns <c>false</c> if the child container does not exist (and in which case the <paramref name="serviceScope"/> parameter value is undefined). Returns <c>true</c> if it was found (and will be returned via <paramref name="serviceScope"/>).</summary>
		public static Boolean TryGetRequestServiceScope( this HttpContext context, out IServiceScope serviceScope )
		{
			serviceScope = context.Items[ RequestServiceProviderKey ] as IServiceScope;
			return serviceScope != null;
		}

		/// <summary>Attempts to get the per-request child <see cref="IServiceScope"/> from the provided <see cref="HttpContextBase"/>. Returns <c>false</c> if the child container does not exist (ignore the <paramref name="serviceScope"/> parameter value). Returns <c>true</c> if it was found (and will be returned via <paramref name="serviceScope"/>).</summary>
		public static Boolean TryGetRequestServiceScope( this HttpContextBase context, out IServiceScope serviceScope )
		{
			serviceScope = context.Items[ RequestServiceProviderKey ] as IServiceScope;
			return serviceScope != null;
		}

		/// <summary>Gets the child container instance out of request state. Throws <see cref="InvalidOperationException"/> if the root container has not yet been set.</summary>
		/// <param name="context">The current request context.</param>
		/// <returns>The request's service scope.</returns>
		public static IServiceScope GetRequestServiceScope( this HttpContext context )
		{
			IServiceScope serviceScope = context.Items[ RequestServiceProviderKey ] as IServiceScope;
			if( serviceScope == null ) throw new InvalidOperationException( "The request service scope has not been set for this " + nameof(HttpContext) + " object." );
			return serviceScope;
		}

		/// <summary>Gets the child container instance out of request state. Throws <see cref="InvalidOperationException"/> if the root container has not yet been set.</summary>
		/// <param name="context">The current request context.</param>
		/// <returns>The request's service scope.</returns>
		public static IServiceScope GetRequestServiceScope( this HttpContextBase context )
		{
			IServiceScope childContainer = context.Items[ RequestServiceProviderKey ] as IServiceScope;
			if( childContainer == null ) throw new InvalidOperationException( "The request service scope has not been set for this " + nameof(HttpContextBase) + " object." );
			return childContainer;
		}

		/// <summary>Stores the provided <paramref name="serviceScope"/> into request state (<see cref="HttpContext.Items"/>). This method does not normally need to called from web-application code but is exposed if you wish to override the container for a particular request.</summary>
		/// <param name="context">The request context.</param>
		/// <param name="serviceScope">The request's service-scope.</param>
		public static void SetRequestServiceScope( this HttpContext context, IServiceScope serviceScope )
		{
			lock( _thisLock )
			{
				context.Items[RequestServiceProviderKey] = serviceScope;
			}
		}

		/// <summary>Stores the provided <paramref name="serviceScope"/> into request state (<see cref="HttpContextBase.Items"/>). This method does not normally need to called from web-application code but is exposed if you wish to override the container for a particular request.</summary>
		/// <param name="context">The request context.</param>
		/// <param name="serviceScope">The request's service-scope.</param>
		public static void SetRequestServiceScope( this HttpContextBase context, IServiceScope serviceScope )
		{
			lock( _thisLock )
			{
				context.Items[RequestServiceProviderKey] = serviceScope;
			}
		}
	}
}
