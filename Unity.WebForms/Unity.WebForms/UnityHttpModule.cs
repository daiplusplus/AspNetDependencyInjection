using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;

using Unity.WebForms.Configuration;

namespace Unity.WebForms
{
	/// <summary>HttpModule that establishes <see cref="IUnityContainer"/> container instances for each <see cref="HttpApplication"/> and <see cref="HttpContext"/>. All <see cref="HttpApplication"/> instances share the same container as  </summary>
	public sealed class UnityHttpModule : IHttpModule
	{
		/// <summary>Indicates whether the configuration data has been loaded already or not.</summary>
		private static Boolean _configurationLoaded = false;

		/// <summary>Backing field for the list of prefixes to ignore.</summary>
		private static IReadOnlyList<NamespacePrefix> _ignoreNamespacePrefixes;

		#region Implementation of IHttpModule

		/// <summary>Initializes a module and prepares it to handle requests.</summary>
		/// <param name="httpApplication">An <see cref="T:System.Web.HttpApplication"/> to associated with the root container.</param>
		public void Init( HttpApplication httpApplication )
		{
			// Note that IHttpModule.Init can be called multiple times as the ASP.NET runtime will pool HttpApplication instances in the same process:
			// https://stackoverflow.com/questions/1140915/httpmodule-init-method-is-called-several-times-why

			// Lazily optional configuration:
			if( !_configurationLoaded )
			{
				Object configurationSectionObj = ConfigurationManager.GetSection( UnityWebFormsConfiguration.SectionPath );
				if( configurationSectionObj is UnityWebFormsConfiguration configuration )
				{
					_ignoreNamespacePrefixes = configuration.Prefixes
						.OfType<NamespaceConfigurationElement>()
						.Select( el => new NamespacePrefix( el.Prefix ) )
						.ToList();
				}
				else
				{
					_ignoreNamespacePrefixes = Array.Empty<NamespacePrefix>();
				}

				_configurationLoaded = true;
			}

			// These event hookups have to be done on every HttpApplication instance, even if the RootContainer isn't set yet - otherwise the event-handlers will never be invoked.

			httpApplication.BeginRequest             += this.OnContextBeginRequest;
			httpApplication.PreRequestHandlerExecute += this.OnContextPreRequestHandlerExecute;
			httpApplication.EndRequest               += this.OnContextEndRequest;

			if( StaticWebFormsUnityContainerOwner.RootContainer == null )
			{
				// TODO: Is it possible to detect if a HttpApplication instance is 'special' or not?
				// Because if this is not a 'special' HttpApplication then this method should throw an exception complaining that `StaticWebFormsUnityContainerOwner.RootContainer == null`.
				//return;
			}
			else
			{
				httpApplication.SetApplicationContainer( StaticWebFormsUnityContainerOwner.RootContainer );
			}
		}

		/// <summary>This method does nothing.</summary>
		void IHttpModule.Dispose()
		{
		}

		#endregion

		#region Life-cycle event handlers

		/// <summary>Initializes a new child container at the beginning of each request.</summary>
		private void OnContextBeginRequest( Object sender, EventArgs e )
		{
			HttpApplication httpApplication = (HttpApplication)sender;

			IUnityContainer applicationContainer = httpApplication.GetApplicationContainer();
			
			IUnityContainer childContainer = applicationContainer.CreateChildContainer();

			// Unity's `IsResolved` method is slow - so we register a dummy implementation by default:
			// https://stackoverflow.com/questions/878994/is-there-tryresolve-in-unity

			IChildContainerConfiguration childConfiguration = applicationContainer.Resolve<IChildContainerConfiguration>();

			// Register one-off, special services:
			{
				HttpContextBase httpContextBase = new HttpContextWrapper( httpApplication.Context );

				// Registering `IHttpContextAccessor` only in the per-request container so services that are not registered per-request (e.g. globally singleton) cannot use this service.
				// Also this needs to be done here in order to get a non-ThreadLocalStorage reference to HttpContext.
				DefaultHttpContextAccessor httpContextAccessor = new DefaultHttpContextAccessor( httpContextBase );
				childContainer.RegisterInstance<IHttpContextAccessor>( httpContextAccessor );

				childConfiguration.ConfigureRequestContainer( httpContextBase, childContainer );
			}

			httpApplication.Context.SetChildContainer( childContainer );
		}

		/// <summary>Registers the injection event to fire when the page has been initialized.</summary>
		private void OnContextPreRequestHandlerExecute( Object sender, EventArgs e )
		{
			HttpApplication httpApplication = (HttpApplication)sender;

			IHttpHandler handler = httpApplication.Context.Handler;

			/* No hander means static content; so no need for a container */
			if( handler == null )
			{
				return;
			}
			
			IUnityContainer childContainer = httpApplication.Context.GetChildContainer();
			childContainer.BuildUp( t: handler.GetType(), existing: handler );

			// User controls are ready to be built up after the page initialization in complete
			if( handler is Page page )
			{
				page.InitComplete += ( Object icSender, EventArgs icEventArgs ) => this.OnPageInitComplete( icSender, icEventArgs, httpApplication.Context );
			}
		}

		/// <summary>Build-up each control in the page's control tree.</summary>
		private void OnPageInitComplete( Object sender, EventArgs e, HttpContext httpContext )
		{
			if( _ignoreNamespacePrefixes == null ) throw new InvalidOperationException( "This " + nameof(UnityHttpModule) + " instance has not been initialized." );

			Page page = (Page)sender;

			IUnityContainer childContainer = httpContext.GetChildContainer();

			foreach( Control c in GetControlTree( page ) )
			{
				String typeFullName     = c.GetType().FullName           ?? String.Empty;
				String baseTypeFullName = c.GetType().BaseType?.FullName ?? String.Empty;

				// filter on namespace prefixes to avoid attempts to build up controls needlessly
				Boolean controlIsMatchedByAPrefix = _ignoreNamespacePrefixes.Any( p => p.Matches( typeFullName ) || p.Matches( baseTypeFullName ) );
				if( !controlIsMatchedByAPrefix )
				{
					childContainer.BuildUp( c.GetType(), c );
				}
			}
		}

		/// <summary>Ensures that the child container gets disposed of properly at the end of each request cycle.</summary>
		private void OnContextEndRequest( Object sender, EventArgs e )
		{
			// I found there are times when this `OnContextEndRequest` would be called but `OnContextBeginRequest` was not called.
			// This happened when ApplicationInsights' package installed its <httpModules> in <system.web> instead of <system.webServer> while `<system.webServer><validation validateIntegratedModeConfiguration="true" />`.

			HttpApplication httpApplication = (HttpApplication)sender;

			if( httpApplication.Context.TryGetChildContainer( out IUnityContainer childContainer ) )
			{
				childContainer.Dispose();
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
