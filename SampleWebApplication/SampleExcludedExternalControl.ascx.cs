using System;

using SampleWebApplication;

namespace ExcludedNamespace.SampleWebApplication
{
	/// <summary>This control's dependencies should be excluded from DI because it is in the "Foo." namespace which is excluded as per the &lt;Unity.WebForms&gt;&lt;ignoreNamspaces&gt; configuration in web.config.</summary>
	public partial class SampleExcludedExternalControl : System.Web.UI.UserControl
	{
		public SampleExcludedExternalControl( Service1 service1, Service2 service2 )
		{
			this.Service1 = service1 ?? throw new ArgumentNullException(nameof(service1));
			this.Service2 = service2 ?? throw new ArgumentNullException(nameof(service2));
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			this.serviceOne.Text = this.Service1 == null ? "null" : "not null";
			this.serviceTwo.Text = this.Service2 == null ? "null" : "not null";
		}

		public Service1 Service1 { get; set; }

		public Service2 Service2 { get; set; }
	}
}