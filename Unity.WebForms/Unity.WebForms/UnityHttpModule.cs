using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;

using Unity.WebForms.Configuration;
using Unity.WebForms.Internal;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Unity.WebForms
{
	/// <summary>HttpModule that establishes <see cref="IUnityContainer"/> container instances for each <see cref="HttpApplication"/> and <see cref="HttpContext"/>. All <see cref="HttpApplication"/> instances share the same container as  </summary>
	public sealed class UnityHttpModule : IHttpModule
	{
		// TODO: Consider moving this to MediWebObjectActivatorServiceProvider and filter services there?
		private static IReadOnlyList<NamespacePrefix> _ignoreNamespacePrefixes = LoadIgnoredNamespacePrefixes();

		private static IReadOnlyList<NamespacePrefix> LoadIgnoredNamespacePrefixes()
		{
			Object configurationSectionObj = ConfigurationManager.GetSection( UnityWebFormsConfiguration.SectionPath );
			if( configurationSectionObj is UnityWebFormsConfiguration configuration )
			{
				return configuration.Prefixes
					.OfType<NamespaceConfigurationElement>()
					.Select( el => new NamespacePrefix( el.Prefix ) )
					.ToList();
			}
			else
			{
				return Array.Empty<NamespacePrefix>();
			}
		}

		/// <summary>Initializes a module and prepares it to handle requests.</summary>
		/// <param name="httpApplication">An <see cref="HttpApplication"/> to associated with the root container.</param>
		public void Init( HttpApplication httpApplication )
		{
			// Note that IHttpModule.Init can be called multiple times as the ASP.NET runtime will pool HttpApplication instances in the same process:
			// https://stackoverflow.com/questions/1140915/httpmodule-init-method-is-called-several-times-why

			// These event hookups have to be done on every HttpApplication instance, even if the RootContainer isn't set yet - otherwise the event-handlers will never be invoked.

			httpApplication.BeginRequest             += this.OnContextBeginRequest;
			httpApplication.PreRequestHandlerExecute += this.OnContextPreRequestHandlerExecute;
			httpApplication.EndRequest               += this.OnContextEndRequest;

			if( StaticWebFormsUnityContainerOwner.RootServiceProvider == null )
			{
				// TODO: Is it possible to detect if a HttpApplication instance is 'special' or not?
				// Because if this is not a 'special' HttpApplication then this method should throw an exception complaining that `StaticWebFormsUnityContainerOwner.RootContainer == null`.
				//return;
			}
			else
			{
				httpApplication.SetApplicationServiceProvider( StaticWebFormsUnityContainerOwner.RootServiceProvider );
			}
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

			httpApplication.Context.SetRequestServiceScope( requestServiceScope );
		}

		private void OnContextPreRequestHandlerExecute( Object sender, EventArgs e )
		{
			HttpApplication httpApplication = (HttpApplication)sender;

			IHttpHandler handler = httpApplication.Context.Handler;

			if( handler == null )
			{
				// No hander means static content; so no need for DI
				return;
			}
			else
			{
//				IServiceProvider applicationServiceProvider = httpApplication.GetApplicationServiceProvider();

//				IServiceScope requestServiceScope = httpApplication.Context.GetRequestServiceScope();

				// User controls are ready to be built up after the page initialization in complete
				if( handler is Page page )
				{
					page.InitComplete += ( Object icSender, EventArgs icEventArgs ) => this.OnPageInitComplete( icSender, icEventArgs, httpApplication.Context );
				}
			}
		}

		/// <summary>Build-up each control in the page's control tree.</summary>
		private void OnPageInitComplete( Object sender, EventArgs e, HttpContext httpContext )
		{
			if( _ignoreNamespacePrefixes == null ) throw new InvalidOperationException( "This " + nameof(UnityHttpModule) + " instance has not been initialized." );

			Page page = (Page)sender;

			IServiceScope requestServiceScope = httpContext.GetRequestServiceScope();

			foreach( Control c in GetControlTree( page ) )
			{
				String typeFullName     = c.GetType().FullName           ?? String.Empty;
				String baseTypeFullName = c.GetType().BaseType?.FullName ?? String.Empty;

				// filter on namespace prefixes to avoid attempts to build up controls needlessly
				Boolean controlIsMatchedByAPrefix = _ignoreNamespacePrefixes.Any( p => p.Matches( typeFullName ) || p.Matches( baseTypeFullName ) );
				if( !controlIsMatchedByAPrefix )
				{
//					childContainer.BuildUp( c.GetType(), c );
				}
			}
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

		#region Helpers

		/// <summary>Traverses through the control tree to build up the dependencies.</summary>
		/// <param name="root">The root control to traverse.</param>
		/// <returns>Any child controls to be processed.</returns>
		private static IEnumerable<Control> GetControlTree( Control root )
		{
			if( root.HasControls() )
			{
				foreach( Control child in root.Controls )
				{
					yield return child;

					if( child.HasControls() )
					{
						foreach( Control c in GetControlTree( child ) )
						{
							yield return c;
						}
					}
				}
			}
		}

		#endregion
	}
}
