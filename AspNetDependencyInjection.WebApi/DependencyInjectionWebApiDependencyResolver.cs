using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
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
			
			GlobalConfiguration.Configuration.DependencyResolver = this;
		}

		/// <summary>When <paramref name="serviceType"/> is for a <see cref="IHttpController"/> then the type will be resolved, otherwise an exception is thrown. Otherwise this method returns <c>null</c> if the type cannot be resolved or created.</summary>
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

			try
			{
				return this.di.ObjectFactoryCache.TryGetRootService( serviceType, useOverrides: true, out Object resolved ) ? resolved : null;
			}
			catch
			{
				// ASP.Net WebApi: all other dependencies are optional
				return null;
			}
		}

		/// <summary>Calls <see cref="GetService(Type)"/> by converting <paramref name="serviceType"/> to <see cref="IEnumerable{T}"/>.</summary>
		public IEnumerable<Object> GetServices(Type serviceType)
		{
			return serviceType.ToIEnumerableOf( this.GetService );
		}

		/// <summary>NOOP.</summary>
		public void Dispose()
		{
			// NOOP
		}

		// Hmm, does Web API not have nested-scopes? Why is it only the root `IDependencyResolver` has `BeginScope()` instead of `IDependencyScope`?
		
		/// <summary>Creates and returns a new instance of <see cref="DependencyInjectionWebApiScope"/> that is an immediate child of the root <see cref="IServiceProvider"/>.</summary>
		public IDependencyScope BeginScope()
		{
			IServiceScope scope = this.rootServiceProvider.CreateScope();

			return new DependencyInjectionWebApiScope( this.di, scope );
		}
	}

	/// <summary>Represents the lifetime of services in ASP.NET Web API.</summary>
	public sealed class DependencyInjectionWebApiScope : IDependencyScope
	{
		private readonly ApplicationDependencyInjection di;
		private readonly IServiceScope scope;

		/// <summary>Constructor. All parameters are required.</summary>
		public DependencyInjectionWebApiScope( ApplicationDependencyInjection di, IServiceScope scope )
		{
			this.di    = di    ?? throw new ArgumentNullException( nameof( di ) );
			this.scope = scope ?? throw new ArgumentNullException( nameof( scope ) );
		}

		/// <summary>Disposes of the scope.</summary>
		public void Dispose()
		{
			this.scope.Dispose();
		}

		/// <summary>When <paramref name="serviceType"/> is for a <see cref="IHttpController"/> then the type will be resolved, otherwise an exception is thrown. Otherwise this method returns <c>null</c> if the type cannot be resolved or created.</summary>
		public Object GetService( Type serviceType )
		{
			if( typeof(IHttpController).IsAssignableFrom( serviceType ) )
			{
				return this.di.ObjectFactoryCache.GetRequiredService( this.GetServiceProvider, serviceType, useOverrides: true );
			}

			try
			{
				return this.di.ObjectFactoryCache.TryGetService( this.GetServiceProvider, serviceType, useOverrides: true, out Object resolved ) ? resolved : null;
			}
			catch
			{
				// ASP.Net WebApi: all other dependencies are optional
				return null;
			}
		}

		/// <summary>Calls <see cref="GetService(Type)"/> by converting <paramref name="serviceType"/> to <see cref="IEnumerable{T}"/>.</summary>
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
