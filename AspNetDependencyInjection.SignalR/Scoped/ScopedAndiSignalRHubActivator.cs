using System;

using Microsoft.AspNet.SignalR.Hubs;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>Factory for <see cref="IHub"/> objects. Consumed by SignalR's <see cref="DefaultHubManager"/>. This implementation uses the current request or operation scope as controlled by <see cref="ScopedAndiSignalRHubDispatcher"/>.</summary>
	public class ScopedAndiSignalRHubActivator : IHubActivator
	{
		private readonly ScopedAndiSignalRDependencyResolver dr;

		/// <summary>Constructor.</summary>
		/// <param name="dr">Required. Cannot be null.</param>
		public ScopedAndiSignalRHubActivator( ScopedAndiSignalRDependencyResolver dr )
		{
			this.dr = dr ?? throw new ArgumentNullException( nameof( dr ) );
		}

		/// <summary>Creates a new instance of the <see cref="IHub"/> object specified by <paramref name="descriptor"/>. The current request or operation scope is used. If no scope is currently available then an <see cref="InvalidOperationException"/> exception is thrown.</summary>
		public IHub Create( HubDescriptor descriptor )
		{
			// SignalR's IHubActivator always creates the requested object, even if it is not registered.
			// By default, SignalR (using its `DefaultHubActivator`) does require each Hub to be registered, but registering each Hub type is not required when using a DI engine that supports creating objects from non-registered types with constructor parameters.

			// This line below fails because MEDI will *not* create a non-registered type, even if the type has a constructor where all parameters are registered.
			//	return (IHub)scope.ServiceProvider.GetRequiredService( serviceType: descriptor.HubType );

			// ...so do this instead:

			Object instantiated = this.dr.CreateInstanceUsingScope( descriptor.HubType );
			return (IHub)instantiated;
		}
	}
}
