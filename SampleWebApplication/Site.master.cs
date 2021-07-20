using System;
using System.Web.UI;

namespace SampleWebApplication
{
	/// <summary>Sample master Sample page demonstrating constructor service injection into MasterPages.</summary>
	public partial class SiteMaster : MasterPage
	{
		public SiteMaster( Service1 service1, Service2 service2 )
		{
			this.InjectedService1 = service1 ?? throw new ArgumentNullException(nameof(service1));
			this.InjectedService2 = service2 ?? throw new ArgumentNullException(nameof(service2));
		}

		/// <summary>Gets the <see cref="Service1" /> dependency (injected).</summary>
		public Service1 InjectedService1 { get; }

		/// <summary>Gets the <see cref="Service2" /> dependency (injected).</summary>
		public Service2 InjectedService2 { get; }
	}
}
