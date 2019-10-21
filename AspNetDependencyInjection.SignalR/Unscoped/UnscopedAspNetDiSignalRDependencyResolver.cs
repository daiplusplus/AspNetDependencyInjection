using System;
using Microsoft.AspNet.SignalR;

namespace AspNetDependencyInjection.Internal
{
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
}
