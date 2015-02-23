using System;

using Microsoft.Practices.Unity;

using SampleWebApplication;

namespace Foo.SampleWebApplication
{
	public partial class SampleExternalControl : System.Web.UI.UserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			serviceOne.Text = Service == null ? "null" : "not null";
		}

		[Dependency]
		public Service1 Service { get; set; }
	}
}