using System;
using System.Web.UI;

using Unity;

namespace SampleWebApplication
{
	/// <summary>
	///		Sample User Control demonstrating injection at the control level.
	/// </summary>
	public partial class InjectedControl : UserControl
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