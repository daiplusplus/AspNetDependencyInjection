using System;

using SampleWebApplication;

namespace IncludedNamespace.SampleWebApplication
{
	public partial class SampleIncludedExternalControl : System.Web.UI.UserControl
	{
		public SampleIncludedExternalControl( Service1 service1, Service2 service2 )
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