using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Compilation;
using System.Web.UI.WebControls;

namespace AspNetDependencyInjection.WebForms
{
	/// <summary>
	/// A drop-in replacement for <see cref="ObjectDataSource" /> controls that enables dependency injection
	/// instantiation for data sources.
	///
	/// To use it, register the control namespace by adding the following element to your web.config file:
	///
	/// <c>
	/// &lt;configuration&gt;
	///   &lt;system.web&gt;
	///     &lt;pages&gt;
	///       &lt;controls&gt;
	///         ...
	///         &lt;add tagPrefix="di" namespace="AspNetDependencyInjection.WebForms" assembly="AspNetDependencyInjection" /&gt;
	///         ...
	///       &lt;/controls&gt;
	///     &lt;/pages&gt;
	///   &lt;/system.web&gt;
	/// &lt;/configuration&gt;
	/// </c>
	///
	/// Then replace the &lt;asp:ObjectDataSource&gt; tags in your code with &lt;di:DiObjectDataSource&gt; tags.
	/// </summary>
	/// <remarks>
	/// Please make sure your <see cref="ObjectDataSource" /> methods are not marked <see langword="static" />.
	/// </remarks>
	public class DiObjectDataSource : ObjectDataSource
	{
		/// <summary>
		/// Background: <see cref="ObjectDataSource" /> controls invoke a `Select` method defined
		/// in the `Page` object in order to retrieve the data to bind to other controls. This select method is
		/// usually a <see langword="static" /> method on the containing page, which prevents using DI,
		/// but can also be an instance method, in which case it instantiates a new `Page` object and calls the method
		/// on the newly created instance.
		/// That's the scenario when DI should be possible.
		///
		/// Unfortunately it seems that Microsoft never updated the <see cref="ObjectDataSource" /> control
		/// to use <see cref="HttpRuntime.WebObjectActivator" />,
		/// so it's not able to instantiate DI-enabled `Page` controls.
		/// However, it is possible to override its <see cref="ObjectDataSource.ObjectCreating" />
		/// event to make it work the way we want.
		/// </summary>
		public DiObjectDataSource()
		{
			// We need to use `BuildManager.GetType` instead of `Type.GetType` because this method is actually
			// able to look for the type in all of the application assemblies - otherwise we'd be restricted
			// to the current assembly types.

			this.ObjectCreating += ( sender, args ) =>
				args.ObjectInstance = HttpRuntime.WebObjectActivator.GetService( BuildManager.GetType(
					( (ObjectDataSourceView)sender ).TypeName, throwOnError: false, ignoreCase: true ) );
		}
	}
}
