using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>Implements SignalR's <see cref="IDependencyResolver"/> by using <see cref="DependencyInjectionWebObjectActivator"/>></summary>
	public sealed class ScopedDependencyInjectionSignalRDependencyResolver : DefaultDependencyResolver, IDependencyResolver, IDependencyInjectionClient
	{
		private readonly ApplicationDependencyInjection di;
		private readonly IServiceProvider               rootServiceProvider;

		// uhhhh - scopes in SignalR are complicated. See here: https://github.com/simpleinjector/SimpleInjector/issues/232
		
//		private readonly DependencyInjectionSignalRHubActivator  hubActivator;
//		private readonly DependencyInjectionSignalRHubDispatcher hubDispatcher;

		internal ScopedDependencyInjectionSignalRDependencyResolver( ApplicationDependencyInjection di, IServiceProvider rootServiceProvider )
			: base()
		{
			this.di                  = di                  ?? throw new ArgumentNullException( nameof(di) );
			this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException( nameof(rootServiceProvider) );

//			this.hubActivator  = new DependencyInjectionSignalRHubActivator( di, rootServiceProvider );
//			this.hubDispatcher = new DependencyInjectionSignalRHubDispatcher( di, rootServiceProvider );
			
//			this.Register( typeof(IHubActivator), () => this.hubActivator );

//			this.originalResolver = GlobalHost.DependencyResolver; // don't use the static getter because it has a Lazy<T> initializer.
//			GlobalHost.DependencyResolver = this;
		}

		public IServiceProvider                        RootServiceProvider => this.rootServiceProvider; // HACK
//		public DependencyInjectionSignalRHubActivator  HubActivator        => this.hubActivator;
//		public DependencyInjectionSignalRHubDispatcher HubDispatcher       => this.hubDispatcher;

//		public DependencyInjectionSignalRHubDispatcher CreateHubDispatcher( HubConfiguration hubConfig )
//		{
//			return new DependencyInjectionSignalRHubDispatcher( this.di, this.rootServiceProvider, hubConfig );
//		}

		public override Object GetService( Type serviceType )
		{
			if( serviceType == null ) throw new ArgumentNullException(nameof(serviceType));

			System.Diagnostics.Debug.Write( "GetService( " + serviceType.FullName );

			//

			IServiceScope scope = DependencyInjectionSignalRHubDispatcher.GetScope();
			Object fromScope = scope.ServiceProvider.GetService( serviceType );
			if( fromScope != null )
			{
				System.Diagnostics.Debug.WriteLine( ", from scope) )" );
				return fromScope;
			}

			Object fromDI = this.rootServiceProvider.GetService( serviceType );
			if( fromDI != null )
			{
				System.Diagnostics.Debug.WriteLine( ", from rootServiceProvider)" );
				return fromDI;
			}

			Object fromDefault = base.GetService( serviceType );
			if( fromDefault != null )
			{
				System.Diagnostics.Debug.WriteLine( ", from DefaultDependencyResolver)" );
				return fromDefault;
			}

			return null;
		}

		/// <summary>Tries to resolve <see cref="IEnumerable{T}"/> where <c>T</c> is <paramref name="serviceType"/>.</summary>
		public override IEnumerable<Object> GetServices( Type serviceType )
		{
			if( serviceType == null ) throw new ArgumentNullException(nameof(serviceType));

			System.Diagnostics.Debug.Write( "GetServices( " + serviceType.FullName + " )" );

			//

			Type ienumerableOf = serviceType.ToIEnumerableOf();

			IEnumerable<Object> fromDI = (IEnumerable<Object>)this.GetService( ienumerableOf );

			IEnumerable<Object> fromDefault = base.GetServices( serviceType );
			
			if( fromDI == null )
			{
				return fromDefault;
			}
			else // `fromDI != null`
			{
				if( fromDefault == null )
				{
					return fromDI;
				}
				else
				{
					return fromDI.Concat( fromDefault );
				}
			}
		}
	}
}
