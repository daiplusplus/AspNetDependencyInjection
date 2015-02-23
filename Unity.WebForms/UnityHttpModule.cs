using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;

using Microsoft.Practices.Unity;

using Unity.WebForms.Configuration;

namespace Unity.WebForms
{
	/// <summary>
	///		HttpModule that maintains a Unity container per request for dependency resolution.
	/// </summary>
	public class UnityHttpModule : IHttpModule
	{
		/// <summary>Indicates whether the configuration data has been loaded already or not.</summary>
		private static bool configurationLoaded = false;

		#region Implementation of IHttpModule

		/// <summary>
		///		Initializes a module and prepares it to handle requests.
		/// </summary>
		/// <param name="context">An <see cref="T:System.Web.HttpApplication"/>
		///		that provides access to the methods, properties, and events 
		///		common to all application objects within an ASP.NET application.</param>
		public void Init( HttpApplication context )
		{
			context.BeginRequest += ContextOnBeginRequest;
			context.PreRequestHandlerExecute += OnPreRequestHandlerExecute;
			context.EndRequest += ContextOnEndRequest;

			// load optional configuration, if present
			if ( !configurationLoaded && _configuration == null )
			{
				_configuration = (UnityWebFormsConfiguration)ConfigurationManager.GetSection( UnityWebFormsConfiguration.SectionPath );
				configurationLoaded = true;
			}
		}

		/// <summary>
		///		Disposes of the resources (other than memory) used by the module
		///		that implements <see cref="T:System.Web.IHttpModule"/>.
		/// </summary>
		public void Dispose()
		{
		}

		#endregion

		#region Life-cycle event handlers

		/// <summary>
		///		Initializes a new child container at the beginning of each request.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ContextOnBeginRequest( object sender, EventArgs e )
		{
			ChildContainer = ParentContainer.CreateChildContainer();
		}

		/// <summary>
		///		Registers the injection event to fire when the page has been
		///		initialized.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPreRequestHandlerExecute( object sender, EventArgs e )
		{
			/* static content; no need for a container */
			if ( HttpContext.Current.Handler == null )
			{
				return;
			}

			var handler = HttpContext.Current.Handler;
			ChildContainer.BuildUp( handler.GetType(), handler );

			// User controls are ready to be built up after the page initialization in complete
			var page = handler as Page;
			if ( page != null )
			{
				page.InitComplete += OnPageInitComplete;
			}
		}

		/// <summary>
		///		Build-up each control in the page's control tree.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPageInitComplete( object sender, EventArgs e )
		{
			var page = (Page)sender;

			foreach ( Control c in GetControlTree( page ) )
			{
				var typeFullName = c.GetType().FullName ?? string.Empty;
				var baseTypeFullName = c.GetType().BaseType != null ? c.GetType().BaseType.FullName : string.Empty;

				// filter on namespace prefixes to avoid attempts to build up controls needlessly
				if ( Prefixes.All( p => !typeFullName.StartsWith( p ) ) && Prefixes.All( p => !baseTypeFullName.StartsWith( p ) ) )
				{
					ChildContainer.BuildUp( c.GetType(), c );
				}
			}
		}

		/// <summary>
		///		Ensures that the child container gets disposed of properly at the end
		///		of each request cycle.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ContextOnEndRequest( object sender, EventArgs e )
		{
			if ( ChildContainer != null )
			{
				ChildContainer.Dispose();
			}
		}

		#endregion

		#region Helpers

		/// <summary>
		///		Traverses through the control tree to build up the dependencies.
		/// </summary>
		/// <param name="root">The root control to traverse.</param>
		/// <returns>
		///		Any child controls to be processed.
		/// </returns>
		private static IEnumerable GetControlTree( Control root )
		{
			if ( root.HasControls() )
			{
				foreach ( Control child in root.Controls )
				{
					yield return child;

					if ( child.HasControls() )
					{
						foreach ( Control c in GetControlTree( child ) )
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
		private IUnityContainer _parentContainer;

		/// <summary>
		///		Gets the parent container out of the application state.
		/// </summary>
		private IUnityContainer ParentContainer
		{
			get { return _parentContainer ?? ( _parentContainer = HttpContext.Current.Application.GetContainer() ); }
		}

		/// <summary>Backing field for the <see cref="ChildContainer"/> property.</summary>
		private IUnityContainer _childContainer;

		/// <summary>
		///  Gets/sets the child container for the current request.
		/// </summary>
		private IUnityContainer ChildContainer
		{
			get { return _childContainer; }

			set
			{
				_childContainer = value;
				HttpContext.Current.SetChildContainer( value );
			}
		}

		/// <summary>
		///		Backing field for the <see cref="Configuration" /> property.
		/// </summary>
		private static UnityWebFormsConfiguration _configuration;

		/// <summary>
		///		Configuration settings for namespaces to ignore.
		/// </summary>
		private UnityWebFormsConfiguration Configuration
		{
			get { return _configuration; }
		}

		/// <summary>Backing field for the list of prefixes to ignore.</summary>
		private static IList<string> _prefixes;

		/// <summary>Gets the list of namespace prefixes to ignore.</summary>
		private IList<string> Prefixes
		{
			get
			{
				if ( _prefixes == null )
				{
					//_prefixes = new List<string> { "System" };
					_prefixes = new List<string>();

					if ( Configuration != null )
					{
						foreach ( NamespaceConfigurationElement item in Configuration.Prefixes )
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
