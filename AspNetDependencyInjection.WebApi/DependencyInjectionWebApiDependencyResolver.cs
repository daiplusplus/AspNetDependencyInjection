using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AspNetDependencyInjection.Internal
{
	// It isn't necessary to subclass DefaultControllerFactory or implement IControllerFactory.
	// Because DefaultControllerFactory uses the registered IDependencyResolver anyway.

	/// <summary>Implements ASP.NET Web PI's's <see cref="IDependencyResolver"/>.</summary>
	public sealed class DependencyInjectionWebApiDependencyResolver : IDependencyResolver, IDependencyInjectionClient // Surprisingly, IDependencyResolver does not implement IServiceProvider, weird.
	{
		private readonly ApplicationDependencyInjection di;
		private readonly IServiceProvider rootServiceProvider;

		internal DependencyInjectionWebApiDependencyResolver( ApplicationDependencyInjection di, IServiceProvider rootServiceProvider )
		{
			this.di                  = di                  ?? throw new ArgumentNullException(nameof(di));
			this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException( nameof( rootServiceProvider ) );
		}

		public Object GetService(Type serviceType)
		{
			if( serviceType == null ) throw new ArgumentNullException(nameof(serviceType));

			// Unlike ASP.NET MVC, ASP.NET Web Api does not use HttpContext and its DI resolver is also responsible for creating child scopes.
			// `DefaultHttpControllerActivator::GetInstanceOrActivator()` --> `System.Net.Http.HttpRequestMessageExtensions::GetDependencyScope(HttpRequestMessage)` --> `IDependencyResolver.BeginScope()`
			
			// Also, if a requested type is a IHttpController, it must succeed (I think...)

			if( typeof(IHttpController).IsAssignableFrom( serviceType ) )
			{
				return this.di.ObjectFactoryCache.GetRequiredRootService( serviceType, useOverrides: true );
			}

			return this.di.ObjectFactoryCache.TryGetRootService( serviceType, useOverrides: true, out Object resolved ) ? resolved : null;
		}

		public IEnumerable<Object> GetServices(Type serviceType)
		{
			return serviceType.ToIEnumerableOf( this.GetService );
		}

		public void Dispose()
		{
			// NOOP
		}

		// Hmm, does Web API not have nested-scopes? Why is it only the root `IDependencyResolver` has `BeginScope()` instead of `IDependencyScope`?
		public IDependencyScope BeginScope()
		{
			IServiceScope scope = this.rootServiceProvider.CreateScope();

			return new DependencyInjectionWebApiScope( this.di, scope );
		}
	}

	public sealed class DependencyInjectionWebApiScope : IDependencyScope
	{
		private readonly ApplicationDependencyInjection di;
		private readonly IServiceScope scope;

		public DependencyInjectionWebApiScope( ApplicationDependencyInjection di, IServiceScope scope )
		{
			this.di    = di    ?? throw new ArgumentNullException( nameof( di ) );
			this.scope = scope ?? throw new ArgumentNullException( nameof( scope ) );
		}

		public void Dispose()
		{
			this.scope.Dispose();
		}

		public Object GetService( Type serviceType )
		{
			if( typeof(IHttpController).IsAssignableFrom( serviceType ) )
			{
				return this.di.ObjectFactoryCache.GetRequiredService( this.GetServiceProvider, serviceType, useOverrides: true );
			}

			return this.di.ObjectFactoryCache.TryGetService( this.GetServiceProvider, serviceType, useOverrides: true, out Object resolved ) ? resolved : null;
		}

		public IEnumerable<Object> GetServices( Type serviceType )
		{
			return serviceType.ToIEnumerableOf( this.GetService );
		}

		private IServiceProvider GetServiceProvider()
		{
			return this.scope.ServiceProvider;
		}
	}
}
