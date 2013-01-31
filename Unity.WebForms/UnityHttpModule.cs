using System;
using System.Collections;
using System.Web;
using System.Web.UI;

using Microsoft.Practices.Unity;

namespace Unity.WebForms
{
    /// <summary>
    ///     Performs dependency injection on Pages and Controls through properties.
    /// </summary>
    public class UnityHttpModule : IHttpModule
    {
        #region Implementation of IHttpModule

        /// <summary>
        ///     Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, 
        ///     properties, and events common to all application objects within an ASP.NET application </param>
        public void Init( HttpApplication context )
        {
            context.PreRequestHandlerExecute += OnPreRequestHandlerExecute;
        }

        /// <summary>
        ///     Disposes of the resources (other than memory) used by the module 
        ///     that implements <see cref="T:System.Web.IHttpModule" />.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        /// <summary>
        ///     Registers the injection event to fire when the page has been
        ///     initialized.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreRequestHandlerExecute(object sender, EventArgs e)
        {
            /* static content */
            if (HttpContext.Current.Handler == null)
            {
                return;
            }

            var handler = HttpContext.Current.Handler;
            Container.BuildUp( handler.GetType(), handler );

            var page = HttpContext.Current.Handler as Page;
            if ( page != null )
            {
                page.InitComplete += OnPageInitComplete;
            }
        }

        /// <summary>
        ///     Buildup each control in the page's control tree.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPageInitComplete( object sender, EventArgs e )
        {
            var page = (Page)sender;

            foreach (Control c in GetControlTree(page))
            {
                var typeFullName = c.GetType().FullName ?? string.Empty;
                var baseTypeFullName = c.GetType().BaseType != null ? c.GetType().BaseType.FullName : string.Empty;

                // filter on namespace prefix to avoid attempts to build up internal controls
                if (!typeFullName.StartsWith("System") || !baseTypeFullName.StartsWith("System"))
                {
                    Container.BuildUp(c.GetType(), c);
                }
            }
        }

        /// <summary>
        ///     Traverses through the control tree to build up the dependencies.
        /// </summary>
        /// <param name="root">The root control to traverse.</param>
        /// <returns>
        ///     Any child controls to be processed.
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

        #region Accessors

		/// <summary>Backing field for the <see cref="Container"/> property.</summary>
        private IUnityContainer _container;

        /// <summary>
        ///     Gets the unity container out of the Application state.
        /// </summary>
        private IUnityContainer Container
        {
            get { return _container ?? (_container = (IUnityContainer)HttpContext.Current.Application["UnityContainer"]); }
        }

        #endregion
    }
}
