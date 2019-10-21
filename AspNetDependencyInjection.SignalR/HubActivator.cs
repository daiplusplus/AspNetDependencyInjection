using System;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection.Internal
{
	public class DependencyInjectionSignalRHubActivator : IHubActivator
	{
		private readonly ApplicationDependencyInjection di;
		private readonly IServiceProvider rootServiceProvider;

		public DependencyInjectionSignalRHubActivator( ApplicationDependencyInjection di, IServiceProvider rootServiceProvider )
		{
			this.di                  = di                  ?? throw new ArgumentNullException( nameof( di ) );
			this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException( nameof( rootServiceProvider ) );
		}

		public IHub Create( HubDescriptor descriptor )
		{
			IServiceScope scope = DependencyInjectionSignalRHubDispatcher.GetScope();

			// This fails because MEDI will *not* create a non-registered type, even if the type has a constructor where all parameters are registered.
			// return (IHub)scope.ServiceProvider.GetRequiredService( serviceType: descriptor.HubType );

			Object instantiated = this.di.ObjectFactoryCache.GetRequiredService( scope.ServiceProvider, descriptor.HubType );
			return (IHub)instantiated;
		}
	}
}
