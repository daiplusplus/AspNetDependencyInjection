using System;
using System.Web;

using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>HttpModule that establishes the <see cref="IServiceScope"/> for each <see cref="HttpApplication"/> and <see cref="HttpContext"/> instance.</summary>
	public sealed class HttpContextScopeHttpModule : IHttpModule
	{
#if ISSUE9
		internal const String DEBUG_HTTPCONTEXT_BEGINREQUEST_INVOKED       = "DEBUG_" + nameof(OnContextBeginRequest) + "_INVOKED";
		internal const String DEBUG_HTTPCONTEXT_ENDREQUEST_INVOKED         = "DEBUG_" + nameof(OnContextEndRequest) + "_INVOKED";
		internal const String DEBUG_HTTPAPPLICATION_ENDAPPLICATION_INVOKED = "DEBUG_" + nameof(OnHttpApplicationDisposed) + "_INVOKED";
#endif

		private readonly ImmutableApplicationDependencyInjectionConfiguration config;
		private readonly IServiceProvider rootServiceProvider;

		/// <summary>Constructs a new instance of <see cref="HttpContextScopeHttpModule"/>. This constructor is invoked by the ASP.NET runtime which uses the <see cref="HttpRuntime.WebObjectActivator"/> to provide the constructor parameters.</summary>
		public HttpContextScopeHttpModule( IServiceProviderAccessor rootServiceProviderAccessor )
		{
			if( rootServiceProviderAccessor == null ) throw new ArgumentNullException(nameof(rootServiceProviderAccessor));

			this.config              = rootServiceProviderAccessor.Configuration       ?? throw new ArgumentException( message: "The " + nameof(rootServiceProviderAccessor.Configuration) + " property returned null.", paramName: nameof(rootServiceProviderAccessor) );
			this.rootServiceProvider = rootServiceProviderAccessor.RootServiceProvider ?? throw new ArgumentException( message: "The " + nameof(rootServiceProviderAccessor.RootServiceProvider) + " property returned null.", paramName: nameof(rootServiceProviderAccessor) );
		}

		/// <summary>Initializes an <see cref="IHttpModule"/> for a new <see cref="HttpApplication"/> instance. This method is invoked for each new <see cref="HttpApplication"/> instance created - ASP.NET will create multiple <see cref="HttpApplication"/> instances in the same AppDomain.</summary>
		/// <param name="httpApplication">An <see cref="HttpApplication"/> to associate with the root <see cref="IServiceProvider"/> which is used to create an <see cref="IServiceScope"/> for each request.</param>
#pragma warning disable CA1725 // Parameter names should match base declaration // The original parameter name of `context` is confusing.
		public void Init( HttpApplication httpApplication )
#pragma warning restore CA1725
		{
			if( httpApplication is null ) throw new ArgumentNullException( nameof( httpApplication ) );

			// Important notes:
			//	* ASP.NET creates and manages a pool of HttpApplication instances:
			//		* It creates multiple "special" instances which are used for the `Application_Start`, `Application_End`, and `Session_End` events. These are given a dummy HttpContext object.
			//		* It creates multiple "normal" instances which are used in normal HTTP requests. These are given real HttpContext objects.
			//			* It creates and manages enough "normal" instances so that each request-thread-pool thread can have its own exclusive instance (so no need to `lock` inside a non-static method in a HttpApplication subclass)

			// https://github.com/Microsoft/referencesource/blob/master/System.Web/HttpApplicationFactory.cs
			// https://stackoverflow.com/questions/1140915/httpmodule-init-method-is-called-several-times-why
			// https://lowleveldesign.org/2011/07/20/global-asax-in-asp-net/ (this article implies it *instantiates* a new HttpApplication instance for each request, but it actually only creates enough so each request thread has its own instance)

			// These event hookups have to be done on every HttpApplication instance - otherwise the event-handlers will never be invoked.

			if( this.config.UseRequestScopes )
			{
				httpApplication.BeginRequest += this.OnContextBeginRequest;
				httpApplication.EndRequest   += this.OnContextEndRequest;
			}

			httpApplication.SetRootServiceProvider( this.rootServiceProvider );

			if( this.config.UseHttpApplicationScopes )
			{
				if( httpApplication is IScopedHttpApplication scopedHttpApplication )
				{
					httpApplication.Disposed += this.OnHttpApplicationDisposed;

					IServiceScope httpApplicationScope = this.rootServiceProvider.CreateScope();

					scopedHttpApplication.HttpApplicationServiceScope = httpApplicationScope;
				}
				else
				{
					String msg = nameof(this.config.UseHttpApplicationScopes) + " is true, but " + nameof(httpApplication) + "'s type (" + httpApplication.GetType().FullName + ") does not implement " + nameof(IScopedHttpApplication) + ".";
					throw new InvalidOperationException( msg );
				}
			}
		}

		/// <summary>This method does nothing.</summary>
		void IHttpModule.Dispose()
		{
		}

		/// <summary>Sets-up per-request IServiceScope.</summary>
		private void OnContextBeginRequest( Object sender, EventArgs e )
		{
			HttpApplication httpApplication = (HttpApplication)sender;

			IServiceScope requestServiceScope;
			if( this.config.UseHttpApplicationScopes )
			{
				requestServiceScope = httpApplication.GetHttpApplicationServiceScope().ServiceProvider.CreateScope();
			}
			else
			{
				requestServiceScope = httpApplication.GetRootServiceProvider().CreateScope();
			}

			// Register the current HttpContextBase inside the `helper` instance so it can be used by the DefaultHttpContextAccessor instance.
			{
				DefaultHttpContextAccessorHelper helper = requestServiceScope.ServiceProvider.GetService<DefaultHttpContextAccessorHelper>();
				if( helper != null )
				{
					helper.HttpContext = new HttpContextWrapper( httpApplication.Context );
				}
			}

			httpApplication.Context.SetRequestServiceScope( requestServiceScope );

#if ISSUE9
			httpApplication.Context.Items[ DEBUG_HTTPCONTEXT_BEGINREQUEST_INVOKED ] = "OK";
#endif
		}

		/// <summary>Ensures that the per-request IServiceScope gets disposed of properly at the end of each request cycle.</summary>
		private void OnContextEndRequest( Object sender, EventArgs e )
		{
			// I found there are times when this `OnContextEndRequest` would be called but `OnContextBeginRequest` was not called.
			// This happened when ApplicationInsights' package installed its <httpModules> in <system.web> instead of <system.webServer> while `<system.webServer><validation validateIntegratedModeConfiguration="true" />`.

			HttpApplication httpApplication = (HttpApplication)sender;
			HttpContext     httpContext     = httpApplication.Context;

			if( httpContext.TryGetRequestServiceScope( out IServiceScope requestServiceScope ) )
			{
				requestServiceScope.Dispose();

#if ISSUE9
				httpContext.Items[ DEBUG_HTTPCONTEXT_ENDREQUEST_INVOKED ] = "OK";
#endif
			}
			else
			{
#if ISSUE9
				httpContext.Items[ DEBUG_HTTPCONTEXT_ENDREQUEST_INVOKED ] = "NotFound";
#endif
			}
		}

		/// <summary>Ensures that the per-HttpApplication IServiceScope gets disposed of properly at the end of each HttpApplication lifespan.</summary>
		private void OnHttpApplicationDisposed( Object sender, EventArgs e )
		{
			HttpApplication httpApplication = (HttpApplication)sender;

			if( httpApplication.TryGetHttpApplicationServiceScope( out IServiceScope httpApplicationServiceScope ) )
			{
				httpApplicationServiceScope.Dispose();

#if ISSUE9
				httpApplication.Application[ DEBUG_HTTPAPPLICATION_ENDAPPLICATION_INVOKED ] = "OK";
#endif
			}
			else
			{
#if ISSUE9
				httpApplication.Application[ DEBUG_HTTPAPPLICATION_ENDAPPLICATION_INVOKED ] = "NotFound";
#endif
			}
		}
	}
}
