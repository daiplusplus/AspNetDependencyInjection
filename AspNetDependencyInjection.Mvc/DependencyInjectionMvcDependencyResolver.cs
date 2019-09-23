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
			if( this.di.TryGetService( this.GetServiceProvider, serviceType, useOverrides: false, out Object service ) )
			{
				return service;
			}
			else
			{
				return null;
			}
		}

		public IEnumerable<Object> GetServices(Type serviceType)
		{
			Object resolved = this.GetService( serviceType );
			if( resolved != null )
			{
				return new Object[] { resolved }; // Isn't there a value-type for single enumerables instead of allocating a new array?
			}
			else
			{
				return Enumerable.Empty<Object>();
			}
		}

		private IServiceProvider GetServiceProvider()
		{
			HttpContext httpContext = HttpContext.Current;
			return this.di.GetServiceProviderForCurrentHttpContext( httpContext );
		}

		public void Dispose()
		{
			if( DependencyResolver.Current == this )
			{
				DependencyResolver.SetResolver( this.originalIdr );
			}
		}
	}
}
