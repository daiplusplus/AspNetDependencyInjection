using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

using Microsoft.Extensions.DependencyInjection;

using Unity.WebForms.Configuration;
using Unity.WebForms.Internal;
using Unity.WebForms.Services;

namespace Unity.WebForms
{
	/// <summary>HttpModule that establishes the <see cref="IServiceScope"/> for each <see cref="HttpApplication"/> and <see cref="HttpContext"/>. All <see cref="HttpApplication"/> instances share the same container as  </summary>
	public sealed class UnityHttpModule : IHttpModule
	{
		private readonly IServiceProvider rootServiceProvider;

		/// <summary>Constructs a new instance of <see cref="UnityHttpModule"/>. This constructor is invoked by the ASP.NET runtime which uses the <see cref="HttpRuntime.WebObjectActivator"/> to provide the constructor parameters.</summary>
		public UnityHttpModule( IServiceProviderAccessor rootServiceProviderAccessor )
		{
			if( rootServiceProviderAccessor == null ) throw new ArgumentNullException(nameof(rootServiceProviderAccessor));

			this.rootServiceProvider = rootServiceProviderAccessor.RootServiceProvider ?? throw new ArgumentException( message: "The " + nameof(rootServiceProviderAccessor.RootServiceProvider) + " property returned null.", paramName: nameof(rootServiceProviderAccessor) );
		}

		/// <summary>Initializes an <see cref="IHttpModule"/> for a new <see cref="HttpApplication"/> instance. This method is invoked for each new <see cref="HttpApplication"/> instance created - ASP.NET will create multiple <see cref="HttpApplication"/> instances in the same AppDomain.</summary>
		/// <param name="httpApplication">An <see cref="HttpApplication"/> to associate with the root <see cref="IServiceProvider"/> which is used to create an <see cref="IServiceScope"/> for each request.</param>
		public void Init( HttpApplication httpApplication )
		{
			// Important notes:
			//	* ASP.NET creates and manages a pool of HttpApplication instances:
			//		* It creates multiple "special" instances which are used for the `Application_Start`, `Application_End`, and `Session_End` events. These are given a dummy HttpContext object.
			//		* It creates multiple "normal" instances which are used in normal HTTP requests. These are given real HttpContext objects.
			//			* It creates and manages enough "normal" instances so that each request-thread-pool thread can have its own exclusive instance (so no need to `lock` inside a non-static method in a HttpApplication subclass)

			// https://github.com/Microsoft/referencesource/blob/master/System.Web/HttpApplicationFactory.cs
			// https://stackoverflow.com/questions/1140915/httpmodule-init-method-is-called-several-times-why
			// https://lowleveldesign.org/2011/07/20/global-asax-in-asp-net/ (this article implies it *instantiates* a new HttpApplication instance for each request, but it actually only creates enough so each request thread has its own instance)

			// These event hookups have to be done on every HttpApplication instance - otherwise the event-handlers will never be invoked.

			httpApplication.BeginRequest += this.OnContextBeginRequest;
			httpApplication.EndRequest   += this.OnContextEndRequest;

			// TODO: Is it possible to detect if a HttpApplication instance is "special" or not? Does it matter?
			// Are special instances created before or after PreStart and PostStart WebActivatorEx events?
			
			httpApplication.SetApplicationServiceProvider( this.rootServiceProvider );
		}

		/// <summary>This method does nothing.</summary>
		void IHttpModule.Dispose()
		{
		}

		#region Life-cycle event handlers

		private void OnContextBeginRequest( Object sender, EventArgs e )
		{
			HttpApplication httpApplication = (HttpApplication)sender;

			IServiceProvider applicationServiceProvider = httpApplication.GetApplicationServiceProvider();
			
			IServiceScope requestServiceScope = applicationServiceProvider.CreateScope();

			// Register the current HttpContextBase inside the `helper` instance so it can be used by the DefaultHttpContextAccessor instance.
			{
				DefaultHttpContextAccessorHelper helper = requestServiceScope.ServiceProvider.GetService<DefaultHttpContextAccessorHelper>();
				if( helper != null )
				{
					helper.HttpContext = new HttpContextWrapper( httpApplication.Context );
				}
			}

			httpApplication.Context.SetRequestServiceScope( requestServiceScope );
		}

		/// <summary>Ensures that the child container gets disposed of properly at the end of each request cycle.</summary>
		private void OnContextEndRequest( Object sender, EventArgs e )
		{
			// I found there are times when this `OnContextEndRequest` would be called but `OnContextBeginRequest` was not called.
			// This happened when ApplicationInsights' package installed its <httpModules> in <system.web> instead of <system.webServer> while `<system.webServer><validation validateIntegratedModeConfiguration="true" />`.

			HttpApplication httpApplication = (HttpApplication)sender;

			if( httpApplication.Context.TryGetRequestServiceScope( out IServiceScope requestServiceScope ) )
			{
				requestServiceScope.Dispose();
			}
		}

		#endregion
	}
}
