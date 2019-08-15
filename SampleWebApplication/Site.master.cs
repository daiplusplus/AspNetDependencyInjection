using System;
using System.Web.UI;

namespace SampleWebApplication
{
	/// <summary>
	///		Sample master Sample page demonstrating injection at the page level.
	/// </summary>
	public partial class SiteMaster : MasterPage
	{
		public SiteMaster( Service1 service1, Service2 service2 )
		{
			this.InjectedService1 = service1 ?? throw new ArgumentNullException(nameof(service1));
			this.InjectedService2 = service2 ?? throw new ArgumentNullException(nameof(service2));
		}

		protected void Page_Load(object sender, EventArgs e)
		{
		}

		#region Dependencies

		/// <summary>Gets/sets the <see cref="Service1" /> dependency (injected).</summary>
		public Service1 InjectedService1 { get; set; }

		/// <summary>Gets/sets the <see cref="Service2" /> dependency (injected).</summary>
		public Service2 InjectedService2 { get; set; }

		#endregion
	}
}
