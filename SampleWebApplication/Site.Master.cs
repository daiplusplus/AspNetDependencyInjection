using System;
using System.Web.UI;
using Microsoft.Practices.Unity;

namespace SampleWebApplication
{
	/// <summary>
	///		Sample master Sample page demonstrating injection at the page level.
	/// </summary>
	public partial class SiteMaster : MasterPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
		}

		#region Dependencies

		/// <summary>Gets/sets the <see cref="Service1" /> dependency (injected).</summary>
		[Dependency]
		public Service1 InjectedService1 { get; set; }

		/// <summary>Gets/sets the <see cref="Service2" /> dependency (injected).</summary>
		[Dependency]
		public Service2 InjectedService2 { get; set; }

		#endregion
	}
}
