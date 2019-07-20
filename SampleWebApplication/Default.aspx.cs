using System;
using System.Web.UI;

using Unity;

namespace SampleWebApplication
{
	/// <summary>
	///		Sample page demonstrating injection at the page level.
	/// </summary>
	public partial class DefaultPage : Page
	{
		private readonly IExampleRequestLifelongService exampleService;
		private readonly Service4 service4;

		public DefaultPage( IExampleRequestLifelongService exampleService, Service4 service4 )
		{
			this.exampleService = exampleService ?? throw new ArgumentNullException(nameof(exampleService));
			this.service4 = service4 ?? throw new ArgumentNullException(nameof(service4));
		}

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
			this.InjectedService3.DoSomething();
			this.service4.DoSomething();
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

		public IExampleRequestLifelongService InjectedService3 => this.exampleService;

		/// <summary>Gets/sets the <see cref="IUnityContainer"/> instance for dynamic resolution.</summary>
		[Dependency]
		public IUnityContainer Container { get; set; }

		#endregion
	}
}
