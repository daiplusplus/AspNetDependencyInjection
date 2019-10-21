using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection.Internal
{
	public class ScopedDependencyInjectionSignalRHubActivator : IHubActivator
	{
		private readonly ApplicationDependencyInjection di;
		private readonly IServiceProvider               rootServiceProvider;

		public ScopedDependencyInjectionSignalRHubActivator( ApplicationDependencyInjection di, IServiceProvider rootServiceProvider )
		{
			this.di                  = di                  ?? throw new ArgumentNullException( nameof( di ) );
			this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException( nameof( rootServiceProvider ) );
		}

		public IHub Create( HubDescriptor descriptor )
		{
			IServiceScope scope = DependencyInjectionSignalRHubDispatcher.GetScope();

			// This line below fails because MEDI will *not* create a non-registered type, even if the type has a constructor where all parameters are registered.
			//	return (IHub)scope.ServiceProvider.GetRequiredService( serviceType: descriptor.HubType );

			// ...so do this instead:

			Object instantiated = this.di.ObjectFactoryCache.GetRequiredService( scope.ServiceProvider, descriptor.HubType );
			return (IHub)instantiated;
		}
	}


	public class UnscopedAspNetDiSignalRDependencyResolver : DefaultDependencyResolver, IDependencyResolver, IDependencyInjectionClient
	{
		private readonly ApplicationDependencyInjection di;
		private readonly IServiceProvider               rootServiceProvider;

		public UnscopedAspNetDiSignalRDependencyResolver( ApplicationDependencyInjection di, IServiceProvider rootServiceProvider )
			: base()
		{
			this.di                  = di                  ?? throw new ArgumentNullException( nameof(di) );
			this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException( nameof(rootServiceProvider) );

			//

			this.HubActivator = new UnscopedDependencyInjectionSignalRHubActivator( this.di, this.rootServiceProvider );

			GlobalHost.DependencyResolver = this;
		}

		public UnscopedDependencyInjectionSignalRHubActivator HubActivator { get; }
	}

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
