using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>Implements SignalR's <see cref="IDependencyResolver"/> by using <see cref="DependencyInjectionWebObjectActivator"/>.</summary>
	[CLSCompliant(false)] // `HubDescriptor` is not CLS-compliant.
	public class UnscopedAndiSignalRDependencyResolver : DefaultDependencyResolver, IDependencyResolver, IDependencyInjectionClient
	{
		private readonly ApplicationDependencyInjection di;

		/// <summary>Constructor. Consuming applications should not create their own instances of <see cref="UnscopedAndiSignalRDependencyResolver"/>.</summary>
		public UnscopedAndiSignalRDependencyResolver( ApplicationDependencyInjection di, IServiceProvider rootServiceProvider )
			: base()
		{
			this.di                  = di                  ?? throw new ArgumentNullException( nameof(di) );
			this.RootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException( nameof(rootServiceProvider) );

			//

			this.HubActivator = new UnscopedAndiSignalRHubActivator( this );

			this.Register( typeof(IHubActivator), () => this.HubActivator );

			GlobalHost.DependencyResolver = this;
		}

		internal ObjectFactoryCache ObjectFactoryCache => this.di.ObjectFactoryCache;

		internal IServiceProvider RootServiceProvider { get; }

		/// <summary>Gets the singleton <see cref="IHubActivator"/> instance that SignalR will use to construct each <see cref="IHub"/> object instance.</summary>
		public IHubActivator HubActivator { get; }
	}
}
