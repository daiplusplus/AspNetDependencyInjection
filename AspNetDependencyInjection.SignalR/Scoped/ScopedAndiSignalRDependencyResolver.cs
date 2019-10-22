using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Extensions.DependencyInjection;

using Owin;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>Implements SignalR's <see cref="IDependencyResolver"/> by using <see cref="DependencyInjectionWebObjectActivator"/> and using request and operation scoped lifetimes.</summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "This is a false-positive, see https://stackoverflow.com/questions/8925925/code-analysis-ca1063-fires-when-deriving-from-idisposable-and-providing-implemen" )]
	public sealed class ScopedAndiSignalRDependencyResolver : DefaultDependencyResolver, IDependencyResolver, IDependencyInjectionClient
	{
		private readonly ApplicationDependencyInjection di;
		private readonly IServiceProvider               rootServiceProvider;

		/// <summary>Constructor. Consuming applications should not create their own instances of <see cref="ScopedAndiSignalRDependencyResolver"/>.</summary>
		public ScopedAndiSignalRDependencyResolver( ApplicationDependencyInjection di, IServiceProvider rootServiceProvider )
			: base() // <-- this results in virtual calls from DefaultDependencyResolver's constructor to `Register` - eww.
		{
			this.di                  = di                  ?? throw new ArgumentNullException( nameof(di) );
			this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException( nameof(rootServiceProvider) );

			//

			this.HubActivator = new ScopedAndiSignalRHubActivator( this );

			// IHubActivator is singleton, so this is okay:
			this.Register( typeof(IHubActivator), () => this.HubActivator );

			// HubDispatcher subclasses are transient and are resolved using the `typeof(TConnection)` passed to `appBuilder.MapSignalR<TConnection>(...)`
			this.Register( typeof(ScopedAndiSignalRHubDispatcher), this.CreateHubDispatcher );

			// Overwrite `GlobalHost.DependencyResolver` immediately so consumers can reference it in OWinStartup:
			GlobalHost.DependencyResolver = this;
		}

		#region Startup

		private HubConfiguration hubConfiguration;

		/// <summary>Handles calling <see cref="OwinExtensions.MapSignalR{TConnection}(IAppBuilder, string, ConnectionConfiguration)"/> on behalf of the consuming application inside its OwinStartup method.</summary>
		/// <param name="appBuilder">The <see cref="IAppBuilder"/> passed into the <see cref="Microsoft.Owin.OwinStartupAttribute"/>-specified startup method.</param>
		/// <param name="path">The URL path that the SignalR Owin middleware will handle. The default value is <c>&quot;/signalr&quot;</c>.</param>
		/// <param name="hubConfiguration">Can be null.</param>
		public void ConfigureSignalR( IAppBuilder appBuilder, String path = "/signalr", HubConfiguration hubConfiguration = null )
		{
			if( appBuilder == null ) throw new ArgumentNullException(nameof(appBuilder));

			if( String.IsNullOrWhiteSpace( path ) ) throw new ArgumentNullException(nameof(path));

			if( this.hubConfiguration != null ) throw new InvalidOperationException( "SignalR has already been configured using this " + nameof(ScopedAndiSignalRDependencyResolver) + " instance." );

			if( GlobalHost.DependencyResolver != this ) throw new InvalidOperationException( "SignalR's DependencyResolver has not yet been set." );

			//

			if( hubConfiguration == null )
			{
				hubConfiguration = new HubConfiguration();
			}

			if( hubConfiguration.Resolver != null && hubConfiguration.Resolver != this )
			{
				throw new InvalidOperationException( "The provided " + nameof(HubConfiguration) + " object refers to a different " + nameof(IDependencyResolver) + " object." );
			}
			else
			{
				hubConfiguration.Resolver = this;
			}

			//

			this.hubConfiguration = hubConfiguration;

			//

			appBuilder.MapSignalR<ScopedAndiSignalRHubDispatcher>( path, this.hubConfiguration );
		}

		/// <summary>Gets the singleton <see cref="IHubActivator"/> instance that SignalR will use to construct each <see cref="IHub"/> object instance.</summary>
		public IHubActivator HubActivator { get; }

		/// <summary>Creates a new transient <see cref="HubDispatcher"/> subclass that handles request and operation scopes for object lifetime.</summary>
		public ScopedAndiSignalRHubDispatcher CreateHubDispatcher()
		{
			if( this.hubConfiguration == null ) throw new InvalidOperationException( "SignalR has not been configured using this " + nameof(ScopedAndiSignalRDependencyResolver) + " instance." );

			return new ScopedAndiSignalRHubDispatcher( this.di, this.rootServiceProvider, this.hubConfiguration );
		}

		#endregion

		/// <summary>Attempts to resolve <paramref name="serviceType"/> from the current request or operation scope (if present), otherwise the root <see cref="IServiceProvider"/> is used. If the requested type cannot be resolved then this method returns <c>null</c>. This overload will always check <see cref="DefaultDependencyResolver"/> if the scope or configured root <see cref="IServiceProvider"/> cannot resolve the type.</summary>
		public override Object GetService( Type serviceType )
		{
			if( serviceType == null ) throw new ArgumentNullException(nameof(serviceType));

			// Shortcut for performance reasons, as out HubDispatcher won't be registered with the container:
			if( serviceType == typeof(ScopedAndiSignalRHubDispatcher) )
			{
				return this.CreateHubDispatcher();
			}

			return this.GetService( serviceType, fallbackToDefaultDependencyResolver: true );
		}

		/// <summary>Attempts to resolve <paramref name="serviceType"/> from the current request or operation scope (if present), otherwise the root <see cref="IServiceProvider"/> is used. If the requested type cannot be resolved then this method returns <c>null</c>. This overload can optionally always check <see cref="DefaultDependencyResolver"/> if the scope or configured root <see cref="IServiceProvider"/> cannot resolve the type.</summary>
		public Object GetService( Type serviceType, Boolean fallbackToDefaultDependencyResolver )
		{
			if( ScopedAndiSignalRHubDispatcher.TryGetScope( out IServiceScope scope ) )
			{
				if( this.di.ObjectFactoryCache.TryGetService( scope.ServiceProvider, serviceType, out Object scopedInstance ) )
				{
					return scopedInstance;
				}
			}

			if( this.di.ObjectFactoryCache.TryGetRootService( serviceType, out Object rootInstance ) )
			{
				return rootInstance;
			}

			if( fallbackToDefaultDependencyResolver )
			{
				return base.GetService( serviceType );
			}

			return null;
		}

		// This would be named `RequireService` except `type` can refer to non-registered services that must still be instantiated (e.g. Hub class instances).

		/// <summary>This method expects to be called within a SignalR request or operation scope (otherwise it throws <see cref="InvalidOperationException"/>. It uses that <see cref="IServiceScope"/> to require an object created by <see cref="AspNetDependencyInjection.ApplicationDependencyInjection"/>'s <see cref="ApplicationDependencyInjection.ObjectFactoryCache"/> (so this method will construct objects that are not registered, in addition to registered services, just like <see cref="AspNetDependencyInjection.Internal.DependencyInjectionWebObjectActivator"/>).</summary>
		public Object CreateInstanceUsingScope( Type type )
		{
			IServiceScope scope = ScopedAndiSignalRHubDispatcher.RequireScope();

			return this.di.ObjectFactoryCache.GetRequiredService( scope.ServiceProvider, type );
		}

		/// <summary>Tries to resolve <see cref="IEnumerable{T}"/> where <c>T</c> is <paramref name="serviceType"/>.</summary>
		public override IEnumerable<Object> GetServices( Type serviceType )
		{
			if( serviceType == null ) throw new ArgumentNullException(nameof(serviceType));

			Type ienumerableOf = serviceType.ToIEnumerableOf();

			IEnumerable<Object> fromDI = (IEnumerable<Object>)this.GetService( ienumerableOf, fallbackToDefaultDependencyResolver: false );

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
