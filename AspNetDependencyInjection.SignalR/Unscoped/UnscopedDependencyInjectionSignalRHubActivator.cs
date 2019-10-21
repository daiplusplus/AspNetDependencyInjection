using System;
using Microsoft.AspNet.SignalR.Hubs;

namespace AspNetDependencyInjection.Internal
{
	public class UnscopedDependencyInjectionSignalRHubActivator : IHubActivator
	{
		private readonly UnscopedAspNetDiSignalRDependencyResolver dr;

		public UnscopedDependencyInjectionSignalRHubActivator( UnscopedAspNetDiSignalRDependencyResolver dr )
		{
			this.dr = dr ?? throw new ArgumentNullException( nameof( dr ) );
		}

		public IHub Create( HubDescriptor descriptor )
		{
			Object instantiated = this.dr.ObjectFactoryCache.GetRequiredService( this.dr.RootServiceProvider, descriptor.HubType );
			return (IHub)instantiated;

//			Object instantiated = this.di.ObjectFactoryCache.GetRequiredService( this.rootServiceProvider, descriptor.HubType );
//			return (IHub)instantiated;
		}
	}
}
