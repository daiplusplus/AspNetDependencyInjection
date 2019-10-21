using System;
using Microsoft.AspNet.SignalR.Hubs;

namespace AspNetDependencyInjection.Internal
{
	public class UnscopedDependencyInjectionSignalRHubActivator : IHubActivator
	{
		private readonly ApplicationDependencyInjection di;
		private readonly IServiceProvider               rootServiceProvider;

		public UnscopedDependencyInjectionSignalRHubActivator( ApplicationDependencyInjection di, IServiceProvider rootServiceProvider )
		{
			this.di                  = di                  ?? throw new ArgumentNullException( nameof( di ) );
			this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException( nameof( rootServiceProvider ) );
		}

		public IHub Create( HubDescriptor descriptor )
		{
			Object instantiated = this.di.ObjectFactoryCache.GetRequiredService( this.rootServiceProvider, descriptor.HubType );
			return (IHub)instantiated;
		}
	}
}
