using System;
using System.Collections;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>Collection of extension methods for the <see cref="HttpApplicationState" /> class.</summary>
	public static class HttpContextExtensions
	{
		private const String _HttpApplication_RootServiceProvider = nameof(_HttpApplication_RootServiceProvider); // Root IServiceProvider in HttpApplication.
		private const String _HttpApplication_ServiceScope        = nameof(_HttpApplication_ServiceScope); // IServiceScope in HttpApplication (when )
		private const String _HttpContext_ServiceScope            = nameof(_HttpContext_ServiceScope); // IServiceScope in HttpContext (when `UseRequestScopes == true`)

		#region HttpApplication - RootServiceProvider

		/// <summary>Gets the <see cref="IServiceProvider"/> instance out of application state. Returns <c>false</c> if the <see cref="HttpApplication"/>'s <see cref="HttpApplicationState"/> service-provider has not yet been set.</summary>
		/// <param name="httpApplication">The Global.asax instance.</param>
		/// <param name="rootServiceProvider"></param>
		public static Boolean TryGetRootServiceProvider( this HttpApplication httpApplication, out IServiceProvider rootServiceProvider )
		{
			if( httpApplication == null ) throw new ArgumentNullException(nameof(httpApplication));

			//

			rootServiceProvider = httpApplication.Application[ _HttpApplication_RootServiceProvider ] as IServiceProvider;
			return rootServiceProvider != null;
		}

		/// <summary>Gets the <see cref="IServiceProvider"/> instance out of application state. Throws <see cref="InvalidOperationException"/> if the <see cref="HttpApplication"/>'s <see cref="HttpApplicationState"/> service-provider has not yet been set.</summary>
		/// <param name="httpApplication">The Global.asax instance.</param>
		/// <returns>The <see cref="IServiceProvider"/> instance.</returns>
		public static IServiceProvider GetRootServiceProvider( this HttpApplication httpApplication )
		{
			if( httpApplication == null ) throw new ArgumentNullException(nameof(httpApplication));

			//

			IServiceProvider serviceProvider = httpApplication.Application[ _HttpApplication_RootServiceProvider ] as IServiceProvider;
			if( serviceProvider is null ) throw new InvalidOperationException( "The root " + nameof(IServiceProvider) + " has not been set for this " + nameof(HttpApplication) + "." );
			return serviceProvider;
		}

		/// <summary>Stores a <see cref="IServiceScope"/> instance into application state. This method does not normally need to called from web-application code but is exposed if you wish to override the <see cref="IServiceProvider"/> (but not <see cref="IServiceScope"/>) for a particular <see cref="HttpApplication"/> instance.</summary>
		/// <param name="httpApplication">The Global.asax instance.</param>
		/// <param name="rootServiceProvider">The <see cref="IServiceProvider"/> to store inside <paramref name="httpApplication"/>'s <see cref="HttpApplicationState"/>.</param>
		public static void SetRootServiceProvider( this HttpApplication httpApplication, IServiceProvider rootServiceProvider )
		{
			if( httpApplication == null ) throw new ArgumentNullException(nameof(httpApplication));

			//

			httpApplication.Application.Lock();

			try
			{
				httpApplication.Application[_HttpApplication_RootServiceProvider] = rootServiceProvider;
			}
			finally
			{
				httpApplication.Application.UnLock();
			}
		}

		#endregion

		#region HttpApplication - ServiceScope

		// HttpApplication does not directly expose any public (or `protected`) instance state.
		// * We cannot use `HttpApplicationState` with a fixed key because it's shared by all instances of `HttpApplication`.
		// * We cannot use `HttpApplicationState` with a variable key because `HttpApplication` does not expose any kind of instance-identifier (and don't think about using GetHashCode() as the basis for a per-instance key).
		// * We cannot use `HttpContext` because that's only for a single request/response lifecycle.
		// * We do seem to be able to abuse HttpApplication's only real expando mutable state field: a `Hashtable` used to store Handler Factories - provided we use a key value that will never be used.
		// * Or we can require consumers to implement `IScopedHttpApplication` and store the IServiceScope themselves.

		/// <summary>Attempts to get the per-HttpApplication <see cref="IServiceScope"/>. Returns <c>false</c> if the child <see cref="IServiceScope"/> does not exist (and in which case the <paramref name="serviceScope"/> parameter value is undefined). Returns <c>true</c> if it was found (and will be returned via <paramref name="serviceScope"/>).</summary>
		public static Boolean TryGetHttpApplicationServiceScope( this HttpApplication httpApplication, out IServiceScope serviceScope )
		{
			if( httpApplication == null ) throw new ArgumentNullException(nameof(httpApplication));

			//

			if( httpApplication is IScopedHttpApplication scopedHttpApplication )
			{
				serviceScope = scopedHttpApplication.HttpApplicationServiceScope;
				
				//if( serviceScope == null ) throw new InvalidOperationException( "The " + nameof(scopedHttpApplication.HttpApplicationServiceScope) + " property returned null" );
			}
			else
			{
				Hashtable handlerFactories = httpApplication.GetHandlerFactories();
				serviceScope = handlerFactories[ _HttpApplication_ServiceScope ] as IServiceScope;
			}

			return serviceScope != null;
		}

		/// <summary>Gets the per-HttpApplication <see cref="IServiceScope"/> instance out of <paramref name="httpApplication"/>. Throws <see cref="InvalidOperationException"/> if the <see cref="IServiceScope"/> is unavailable.</summary>
		/// <param name="httpApplication">The current HttpApplication instance.</param>
		/// <returns>The HttpApplication's service scope.</returns>
		public static IServiceScope GetHttpApplicationServiceScope( this HttpApplication httpApplication )
		{
			if( httpApplication == null ) throw new ArgumentNullException(nameof(httpApplication));

			//

			if( httpApplication is IScopedHttpApplication scopedHttpApplication )
			{
				IServiceScope serviceScope = scopedHttpApplication.HttpApplicationServiceScope;
				if( serviceScope == null ) throw new InvalidOperationException( "The " + nameof(scopedHttpApplication.HttpApplicationServiceScope) + " property returned null" );
				return serviceScope;
			}
			else
			{
				Hashtable handlerFactories = httpApplication.GetHandlerFactories();
				Object serviceScopeObj = handlerFactories[ _HttpApplication_ServiceScope ];
				if( serviceScopeObj == null ) throw new InvalidOperationException( "The IServiceScope has not been set in this HttpApplication instance." );

				return (IServiceScope)serviceScopeObj;
			}
		}

		/// <summary>Stores the provided <paramref name="serviceScope"/> into <paramref name="httpApplication"/>. This method does not normally need to called from web-application code but is exposed if you wish to override the <see cref="IServiceScope"/> (not <see cref="IServiceProvider"/>) for a particular HttpApplication.</summary>
		/// <param name="httpApplication">The HttpApplication instance.</param>
		/// <param name="serviceScope">The HttpApplication's service-scope.</param>
		public static void SetHttpApplicationServiceScope( this HttpApplication httpApplication, IServiceScope serviceScope )
		{
			if( httpApplication == null ) throw new ArgumentNullException(nameof(httpApplication));
			if( serviceScope == null ) throw new ArgumentNullException(nameof(serviceScope));

			//

			if( httpApplication is IScopedHttpApplication scopedHttpApplication )
			{
				scopedHttpApplication.HttpApplicationServiceScope = serviceScope;
			}
		}

		#endregion

		#region HttpContext - ServiceScope

		/// <summary>Attempts to get the per-request <see cref="IServiceScope"/> from the provided <see cref="HttpContext"/>. Returns <c>false</c> if the child <see cref="IServiceScope"/> does not exist (and in which case the <paramref name="serviceScope"/> parameter value is undefined). Returns <c>true</c> if it was found (and will be returned via <paramref name="serviceScope"/>).</summary>
		public static Boolean TryGetRequestServiceScope( this HttpContext context, out IServiceScope serviceScope )
		{
			serviceScope = context.Items[ _HttpContext_ServiceScope ] as IServiceScope;
			return serviceScope != null;
		}

		/// <summary>Gets the per-request <see cref="IServiceScope"/> instance out of request state. Throws <see cref="InvalidOperationException"/> if the per-requesst <see cref="IServiceScope"/> has not yet been set.</summary>
		/// <param name="context">The current request context.</param>
		/// <returns>The request's service scope.</returns>
		public static IServiceScope GetRequestServiceScope( this HttpContext context )
		{
			IServiceScope serviceScope = context.Items[ _HttpContext_ServiceScope ] as IServiceScope;
			if( serviceScope == null ) throw new InvalidOperationException( "The request service scope has not been set for this " + nameof(HttpContext) + " object." );
			return serviceScope;
		}

		/// <summary>Stores the provided <paramref name="serviceScope"/> into request state (<see cref="HttpContext.Items"/>). This method does not normally need to called from web-application code but is exposed if you wish to override the per-request <see cref="IServiceScope"/> for a particular request.</summary>
		/// <param name="context">The request context.</param>
		/// <param name="serviceScope">The request's service-scope.</param>
		public static void SetRequestServiceScope( this HttpContext context, IServiceScope serviceScope )
		{
			context.Items[_HttpContext_ServiceScope] = serviceScope;
		}

		#endregion

		#region HttpContextBase - ServiceScope

		/// <summary>Attempts to get the per-request <see cref="IServiceScope"/> from the provided <see cref="HttpContextBase"/>. Returns <c>false</c> if the child <see cref="IServiceScope"/> does not exist (and in which case the <paramref name="serviceScope"/> parameter value is undefined). Returns <c>true</c> if it was found (and will be returned via <paramref name="serviceScope"/>).</summary>
		public static Boolean TryGetRequestServiceScope( this HttpContextBase context, out IServiceScope serviceScope )
		{
			serviceScope = context.Items[ _HttpContext_ServiceScope ] as IServiceScope;
			return serviceScope != null;
		}

		/// <summary>Gets the per-request <see cref="IServiceScope"/> instance out of request state. Throws <see cref="InvalidOperationException"/> if the per-requesst <see cref="IServiceScope"/> has not yet been set.</summary>
		/// <param name="context">The current request context.</param>
		/// <returns>The request's service scope.</returns>
		public static IServiceScope GetRequestServiceScope( this HttpContextBase context )
		{
			IServiceScope serviceScope = context.Items[ _HttpContext_ServiceScope ] as IServiceScope;
			if( serviceScope == null ) throw new InvalidOperationException( "The request service scope has not been set for this " + nameof(HttpContextBase) + " object." );
			return serviceScope;
		}

		/// <summary>Stores the provided <paramref name="serviceScope"/> into request state (<see cref="HttpContextBase.Items"/>). This method does not normally need to called from web-application code but is exposed if you wish to override the per-request <see cref="IServiceScope"/> for a particular request.</summary>
		/// <param name="context">The request context.</param>
		/// <param name="serviceScope">The request's service-scope.</param>
		public static void SetRequestServiceScope( this HttpContextBase context, IServiceScope serviceScope )
		{
			context.Items[_HttpContext_ServiceScope] = serviceScope;
		}

		#endregion
	}
}
