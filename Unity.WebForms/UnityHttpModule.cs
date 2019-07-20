using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;

using Unity.WebForms.Configuration;

namespace Unity.WebForms
{
	/// <summary>HttpModule that maintains a Unity container per request for dependency resolution.</summary>
	public sealed class UnityHttpModule : IHttpModule
	{
		/// <summary>Indicates whether the configuration data has been loaded already or not.</summary>
		private static bool configurationLoaded = false;

		#region Implementation of IHttpModule

		/// <summary>Initializes a module and prepares it to handle requests.</summary>
		/// <param name="context">An <see cref="T:System.Web.HttpApplication"/>
		///		that provides access to the methods, properties, and events 
		///		common to all application objects within an ASP.NET application.</param>
		public void Init( HttpApplication context )
		{
			context.BeginRequest             += this.ContextOnBeginRequest;
			context.PreRequestHandlerExecute += this.OnPreRequestHandlerExecute;
			context.EndRequest               += this.ContextOnEndRequest;

			// load optional configuration, if present
			if ( !configurationLoaded && _configuration == null )
			{
				_configuration = (UnityWebFormsConfiguration)ConfigurationManager.GetSection( UnityWebFormsConfiguration.SectionPath );
				configurationLoaded = true;
			}
		}

		/// <summary>Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.</summary>
		public void Dispose()
		{
		}

		#endregion

		#region Life-cycle event handlers

		/// <summary>Initializes a new child container at the beginning of each request.</summary>
		private void ContextOnBeginRequest( object sender, EventArgs e )
		{
			this.ChildContainer = this.ParentContainer.CreateChildContainer();
		}

		/// <summary>Registers the injection event to fire when the page has been initialized.</summary>
		private void OnPreRequestHandlerExecute( Object sender, EventArgs e )
		{
			/* static content; no need for a container */
			if( HttpContext.Current.Handler == null )
			{
				return;
			}

			IHttpHandler handler = HttpContext.Current.Handler;
			this.ChildContainer.BuildUp( handler.GetType(), handler );

			// User controls are ready to be built up after the page initialization in complete
			if( handler is Page page )
			{
				page.InitComplete += this.OnPageInitComplete;
			}
		}

		/// <summary>Build-up each control in the page's control tree.</summary>
		private void OnPageInitComplete( Object sender, EventArgs e )
		{
			Page page = (Page)sender;

			foreach ( Control c in GetControlTree( page ) )
			{
				String typeFullName     = c.GetType().FullName           ?? String.Empty;
				String baseTypeFullName = c.GetType().BaseType?.FullName ?? String.Empty;

				// filter on namespace prefixes to avoid attempts to build up controls needlessly
				if ( this.Prefixes.All( p => !typeFullName.StartsWith( p ) ) && this.Prefixes.All( p => !baseTypeFullName.StartsWith( p ) ) )
				{
					this.ChildContainer.BuildUp( c.GetType(), c );
				}
			}
		}

		/// <summary>Ensures that the child container gets disposed of properly at the end of each request cycle.</summary>
		private void ContextOnEndRequest( object sender, EventArgs e )
		{
			if ( this.ChildContainer != null )
			{
				this.ChildContainer.Dispose();
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

		#region Properties

		/// <summary>Backing field for the <see cref="ParentContainer"/> property.</summary>
		private IUnityContainer parentContainer;

		/// <summary>Gets the parent container out of the application state.</summary>
		private IUnityContainer ParentContainer
		{
			get { return this.parentContainer ?? ( this.parentContainer = HttpContext.Current.Application.GetContainer() ); }
		}

		/// <summary>Backing field for the <see cref="ChildContainer"/> property.</summary>
		private IUnityContainer childContainer;

		/// <summary>
		///  Gets/sets the child container for the current request.
		/// </summary>
		private IUnityContainer ChildContainer
		{
			get
			{
				return this.childContainer;
			}
			set
			{
				this.childContainer = value;
				HttpContext.Current.SetChildContainer( value );
			}
		}

		/// <summary>Backing field for the <see cref="Configuration" /> property.</summary>
		private static UnityWebFormsConfiguration _configuration;

		/// <summary>Configuration settings for namespaces to ignore.</summary>
		private UnityWebFormsConfiguration Configuration
		{
			get { return _configuration; }
		}

		/// <summary>Backing field for the list of prefixes to ignore.</summary>
		private static IList<String> _prefixes;

		/// <summary>Gets the list of namespace prefixes to ignore.</summary>
		private IList<String> Prefixes
		{
			get
			{
				if ( _prefixes == null )
				{
					//_prefixes = new List<string> { "System" };
					_prefixes = new List<string>();

					if ( this.Configuration != null )
					{
						foreach ( NamespaceConfigurationElement item in this.Configuration.Prefixes )
						{
							_prefixes.Add( item.Prefix );
						}
					}
				}

				return _prefixes;
			}
		}

		#endregion
	}
}
