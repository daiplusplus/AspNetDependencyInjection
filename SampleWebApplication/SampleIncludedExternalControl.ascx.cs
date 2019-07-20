using System;

using SampleWebApplication;

using Unity;

namespace IncludedNamespace.SampleWebApplication
{
	public partial class SampleIncludedExternalControl : System.Web.UI.UserControl
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