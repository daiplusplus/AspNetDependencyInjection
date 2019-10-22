using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace AspNetDependencyInjection.Internal
{
	public class UnscopedAndiSignalRDependencyResolver : DefaultDependencyResolver, IDependencyResolver, IDependencyInjectionClient
	{
		private readonly ApplicationDependencyInjection di;

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

		public IHubActivator HubActivator { get; }
	}
}
