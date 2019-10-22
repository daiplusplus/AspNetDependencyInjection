using System;
using Microsoft.AspNet.SignalR.Hubs;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>Factory for <see cref="IHub"/> objects. Consumed by SignalR's <see cref="DefaultHubManager"/>. This implementation does not use any scopes for limiting the lifetime of services.</summary>
	public class UnscopedAndiSignalRHubActivator : IHubActivator
	{
		private readonly UnscopedAndiSignalRDependencyResolver dr;

		/// <summary>Constructor.</summary>
		/// <param name="dr">Required. Cannot be null.</param>
		public UnscopedAndiSignalRHubActivator( UnscopedAndiSignalRDependencyResolver dr )
		{
			this.dr = dr ?? throw new ArgumentNullException( nameof( dr ) );
		}

		/// <summary>Creates a new instance of the <see cref="IHub"/> object specified by <paramref name="descriptor"/>.</summary>
		public IHub Create( HubDescriptor descriptor )
		{
			Object instantiated = this.dr.ObjectFactoryCache.GetRequiredService( this.dr.RootServiceProvider, descriptor.HubType );
			return (IHub)instantiated;
		}
	}
}
