using System;
using System.Web.UI;

using Unity;

namespace SampleWebApplication
{
	/// <summary>
	///		Sample page demonstrating injection at the page level.
	/// </summary>
	public partial class _Default : Page
	{
		protected override void OnInit( EventArgs e )
		{
			if ( !IsPostBack )
			{
				AddControls();
			}

			base.OnInit( e );
		}

		protected void Page_Load( object sender, EventArgs e )
		{
		}

		#region Helpers

		/// <summary>
		///		Dynamically adds a new User Control to the page, resolving the
		///		dependencies manually.
		/// </summary>
		private void AddControls()
		{
			InjectedControl newControl = LoadControl("InjectedControl.ascx") as InjectedControl;
			Container.BuildUp( newControl );
			DynamicInjectedControl.Controls.Add( newControl );
		}

		#endregion

		#region Dependencies

		/// <summary>Gets/sets the <see cref="Service1" /> dependency (injected).</summary>
		[Dependency]
		public Service1 InjectedService1 { get; set; }

		/// <summary>Gets/sets the <see cref="Service2" /> dependency (injected).</summary>
		[Dependency]
		public Service2 InjectedService2 { get; set; }

		/// <summary>Gets/sets the <see cref="IUnityContainer"/> instance for dynamic resolution.</summary>
		[Dependency]
		public IUnityContainer Container { get; set; }

		#endregion
	}
}
