using System;
using System.Web.UI;

namespace SampleWebApplication
{
	/// <summary>Sample User Control demonstrating constructor service injection into UserControls.</summary>
	public partial class InjectedControl : UserControl
	{
		/// <summary>NOTE: For reasons not yet fully understood, when using constructor-injected services in UserControls, the UserControl must be a public class (not abstract) and must have a public (not protected) constructor.</summary>
		public InjectedControl( Service1 service1, Service2 service2 )
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
