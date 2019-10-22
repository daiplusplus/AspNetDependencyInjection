using System;

using AspNetDependencyInjection.Internal;

namespace AspNetDependencyInjection
{
	/// <summary>Extension methods for <see cref="ApplicationDependencyInjectionBuilder"/>.</summary>
	public static class ApplicationDependencyInjectionBuilderExtensions
	{
		/// <summary>Use this method to add AspNetDependencyInjection support to SignalR. It uses a custom <see cref="Microsoft.AspNet.SignalR.Hubs.HubDispatcher"/> to manage scope lifetime of <see cref="Microsoft.AspNet.SignalR.Hubs.IHub"/> objects. You should prefer this method instead of <see cref="AddUnscopedSignalRDependencyResolver(ApplicationDependencyInjectionBuilder)"/> unless you're certain you don't want scoped lifetimes in SignalR or if you experience issues with scoped lifetimes in SignalR.</summary>
		public static ApplicationDependencyInjectionBuilder AddScopedSignalRDependencyResolver( this ApplicationDependencyInjectionBuilder builder )
		{
			if( builder == null ) throw new ArgumentNullException(nameof(builder));

			return builder
				.AddClient( ( di, rootSP ) => new ScopedAndiSignalRDependencyResolver( di, rootSP ) );
		}

		/// <summary>Use this method to add AspNetDependencyInjection support to SignalR. It only supports Singleton and Transient service lifetimes and does not support scoped lifetimes. It uses a custom <see cref="Microsoft.AspNet.SignalR.Hubs.IHubActivator"/> that uses the root <see cref="IServiceProvider"/>. This approach is much simpler than the approach used by <see cref="AddScopedSignalRDependencyResolver(ApplicationDependencyInjectionBuilder)"/> so you may choose to use this method if you experience problems with scoped lifetimes.</summary>
		public static ApplicationDependencyInjectionBuilder AddUnscopedSignalRDependencyResolver( this ApplicationDependencyInjectionBuilder builder )
		{
			if( builder == null ) throw new ArgumentNullException(nameof(builder));

			// TODO: I want to add a way to add validation so there'll be an exception if both AddScoped and AddUnscoped are used at the same time.

			return builder
				.AddClient( ( di, rootSP ) => new UnscopedAndiSignalRDependencyResolver( di, rootSP ) );
		}
	}
}
