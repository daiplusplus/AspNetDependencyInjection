using System;
using Microsoft.AspNet.SignalR.Hubs;

namespace AspNetDependencyInjection.Internal
{
	public class UnscopedAndiSignalRHubActivator : IHubActivator
	{
		private readonly UnscopedAndiSignalRDependencyResolver dr;

		public UnscopedAndiSignalRHubActivator( UnscopedAndiSignalRDependencyResolver dr )
		{
			this.dr = dr ?? throw new ArgumentNullException( nameof( dr ) );
		}

		public IHub Create( HubDescriptor descriptor )
		{
			Object instantiated = this.dr.ObjectFactoryCache.GetRequiredService( this.dr.RootServiceProvider, descriptor.HubType );
			return (IHub)instantiated;
		}
	}
}
