using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNet.SignalR;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>Implements SignalR's <see cref="IDependencyResolver"/> by using <see cref="DependencyInjectionWebObjectActivator"/>></summary>
	public sealed class DependencyInjectionSignalRDependencyResolver : DefaultDependencyResolver, IDependencyResolver, IDependencyInjectionClient
	{
		private readonly ApplicationDependencyInjection di;
		private readonly IServiceProvider rootServiceProvider;

		// uhhhh - scopes in SignalR are complicated. See here: https://github.com/simpleinjector/SimpleInjector/issues/232
		// CBA to do this tonight.

		internal DependencyInjectionSignalRDependencyResolver( ApplicationDependencyInjection di, IServiceProvider rootServiceProvider )
		{
			this.di = di ?? throw new ArgumentNullException(nameof(di));
			this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException( nameof( rootServiceProvider ) );

//			this.originalResolver = GlobalHost.DependencyResolver; // don't use the static getter because it has a Lazy<T> initializer.
			GlobalHost.DependencyResolver = this;
		}

		public override Object GetService( Type serviceType )
		{
			if( serviceType == null ) throw new ArgumentNullException(nameof(serviceType));

			throw new NotImplementedException();
		}

		/// <summary>Tries to resolve <see cref="IEnumerable{T}"/> where <c>T</c> is <paramref name="serviceType"/>.</summary>
		public override IEnumerable<Object> GetServices( Type serviceType )
		{
			return serviceType.ToIEnumerableOf( this.GetService );
		}
	}
}
