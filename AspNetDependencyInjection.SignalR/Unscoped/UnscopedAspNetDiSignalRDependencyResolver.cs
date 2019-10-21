using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace AspNetDependencyInjection.Internal
{
	public class UnscopedAspNetDiSignalRDependencyResolver : DefaultDependencyResolver, IDependencyResolver, /*IHubActivator,*/ IDependencyInjectionClient
	{
		private readonly ApplicationDependencyInjection di;
		private readonly IServiceProvider               rootServiceProvider;

		public UnscopedAspNetDiSignalRDependencyResolver( ApplicationDependencyInjection di, IServiceProvider rootServiceProvider )
			: base()
		{
			this.di                  = di                  ?? throw new ArgumentNullException( nameof(di) );
			this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException( nameof(rootServiceProvider) );

			//

			this.HubActivator = new UnscopedDependencyInjectionSignalRHubActivator( this );

			GlobalHost.DependencyResolver = this;

			GlobalHost.DependencyResolver.Register( typeof(IHubActivator), () => this.HubActivator );
		}

		internal ObjectFactoryCache ObjectFactoryCache => this.di.ObjectFactoryCache;

		internal IServiceProvider RootServiceProvider => this.rootServiceProvider;

		public IHubActivator HubActivator { get; }

		public T GetRootRequiredService<T>()
		{
			return (T)this.ObjectFactoryCache.GetRequiredRootService( typeof(T), useOverrides: false );
		}

//		public IHub Create( HubDescriptor descriptor )
//		{
//			Object instantiated = this.ObjectFactoryCache.GetRequiredService( this.RootServiceProvider, descriptor.HubType );
//			return (IHub)instantiated;
//		}
	}
}
