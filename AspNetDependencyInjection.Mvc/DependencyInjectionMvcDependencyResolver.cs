using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AspNetDependencyInjection.Internal
{
	// It isn't necessary to subclass DefaultControllerFactory or implement IControllerFactory.
	// Because DefaultControllerFactory uses the registered IDependencyResolver anyway.

	/// <summary>Implements ASP.NET MVC's <see cref="IDependencyResolver"/> by using <see cref="DependencyInjectionWebObjectActivator"/>></summary>
	public sealed class DependencyInjectionMvcDependencyResolver : IDependencyResolver, IDependencyInjectionClient // Surprisingly, IDependencyResolver does not implement IServiceProvider, weird.
	{
		private readonly ApplicationDependencyInjection di;

		private readonly IDependencyResolver originalIdr;

		internal DependencyInjectionMvcDependencyResolver( ApplicationDependencyInjection di )
		{
			this.di = di ?? throw new ArgumentNullException(nameof(di));

			this.originalIdr = DependencyResolver.Current; // Will never be null.
		}

		/// <summary>Returns <c>null</c> if the requested service does not have a registered implementation.</summary>
		public Object GetService(Type serviceType)
		{
			if( serviceType == null ) throw new ArgumentNullException(nameof(serviceType));

			// System.Web.Mvc's default DependencyResolver just wraps Activator, with one key difference:
			// If the requested type is an interface or is abstract, then it simply returns null.
			// However, obviously we don't want to do that because *if* it does have an implementation it should be returned.
			
			// `useOverrides: false` so Activator won't be used for types under `System.Web.Mvc`.
			if( this.di.ObjectFactoryCache.TryGetService( this.GetServiceProvider, serviceType, useOverrides: false, out Object service ) )
			{
				return service;
			}
			else
			{
				return null;
			}
		}

		/// <summary>Converts <paramref name="serviceType"/> into an <see cref="IEnumerable{T}"/> and passes it into <see cref="GetService(Type)"/>.</summary>
		public IEnumerable<Object> GetServices(Type serviceType)
		{
			// This implementation from `Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions`:
			Type closedGenericType = typeof(IEnumerable<>).MakeGenericType( serviceType );

			return (IEnumerable<Object>)this.GetService( serviceType );
		}

		private IServiceProvider GetServiceProvider()
		{
			HttpContext httpContext = HttpContext.Current;
			return this.di.GetServiceProviderForCurrentHttpContext( httpContext );
		}

		/// <summary>Resets the ASP.NET MVC <see cref="DependencyResolver.Current"/>, but only if this <see cref="DependencyInjectionMvcDependencyResolver"/> instance is still the current resolver.</summary>
		public void Dispose()
		{
			if( DependencyResolver.Current == this )
			{
				DependencyResolver.SetResolver( this.originalIdr );
			}
		}
	}
}
