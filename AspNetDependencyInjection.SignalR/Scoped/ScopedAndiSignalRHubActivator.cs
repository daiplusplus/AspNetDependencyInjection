using System;

using Microsoft.AspNet.SignalR.Hubs;

namespace AspNetDependencyInjection.Internal
{
	public class ScopedAndiSignalRHubActivator : IHubActivator
	{
		private readonly ScopedAndiSignalRDependencyResolver dr;

		public ScopedAndiSignalRHubActivator( ScopedAndiSignalRDependencyResolver dr )
		{
			this.dr = dr ?? throw new ArgumentNullException( nameof( dr ) );
		}

		public IHub Create( HubDescriptor descriptor )
		{
			// SignalR's IHubActivator always creates the requested object, even if it is not registered.
			// By default, SignalR (using its `DefaultHubActivator`) does require each Hub to be registered, but registering each Hub type is not required when using a DI engine that supports creating objects from non-registered types with constructor parameters.

			// This line below fails because MEDI will *not* create a non-registered type, even if the type has a constructor where all parameters are registered.
			//	return (IHub)scope.ServiceProvider.GetRequiredService( serviceType: descriptor.HubType );

			// ...so do this instead:

			Object instantiated = this.dr.CreateInstanceUsingScope( descriptor.HubType, fallbackToDefaultDependencyResolver: false );
			return (IHub)instantiated;
		}
	}
}
