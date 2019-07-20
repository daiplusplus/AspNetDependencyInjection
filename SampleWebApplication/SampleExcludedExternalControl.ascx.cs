using System;

using SampleWebApplication;

using Unity;

namespace ExcludedNamespace.SampleWebApplication
{
	/// <summary>This control's dependencies should be excluded from DI because it is in the "Foo." namespace which is excluded as per the &lt;Unity.WebForms&gt;&lt;ignoreNamspaces&gt; configuration in web.config.</summary>
	public partial class SampleExcludedExternalControl : System.Web.UI.UserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			this.serviceOne.Text = this.Service1 == null ? "null" : "not null";
			this.serviceTwo.Text = this.Service2 == null ? "null" : "not null";
		}

		[Dependency]
		public Service1 Service1 { get; set; }

		[Dependency]
		public Service2 Service2 { get; set; }
	}
}