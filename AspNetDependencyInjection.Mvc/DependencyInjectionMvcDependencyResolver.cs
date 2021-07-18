using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection.Internal
{
	// It isn't necessary to subclass DefaultControllerFactory or implement IControllerFactory.
	// Because DefaultControllerFactory uses the registered IDependencyResolver anyway.

	/// <summary>Implements ASP.NET MVC's <see cref="IDependencyResolver"/> by using <see cref="DependencyInjectionWebObjectActivator"/>.</summary>
	public sealed class DependencyInjectionMvcDependencyResolver : IDependencyResolver, IDependencyInjectionClient // Surprisingly, IDependencyResolver does not implement IServiceProvider, weird.
	{
		private readonly ApplicationDependencyInjection di;

		private readonly IDependencyResolver originalIdr;

		private readonly DependencyInjectionMvcControllerActivator controllerActivator;

		internal DependencyInjectionMvcDependencyResolver( ApplicationDependencyInjection di )
		{
			this.di = di ?? throw new ArgumentNullException(nameof(di));

			this.originalIdr = DependencyResolver.Current; // Will never be null.
			this.controllerActivator = new DependencyInjectionMvcControllerActivator( di );

			DependencyResolver.SetResolver(this);
		}

		/// <summary>Returns <c><see langword="null"/></c> if the requested service does not have a registered implementation.</summary>
		public Object GetService( Type serviceType )
		{
			if( serviceType is null ) throw new ArgumentNullException(nameof(serviceType));

			// Return known factories/activator services, as ASP.NET MVC can pass these the current HttpContext, which it can't do with `IDependencyResolver`:
			{
				if( serviceType == typeof(IControllerActivator) )
				{
					return this.controllerActivator;
				}
			}

			// System.Web.Mvc's default DependencyResolver just wraps Activator, with one key difference:
			// If the requested type is an interface or is abstract, then it simply returns null.
			// However, obviously we don't want to do that because *if* it does have an implementation it should be returned.
			
			// `useOverrides: false` so Activator won't be used for types under `System.Web.Mvc`.
			if( this.di.ObjectFactoryCache.TryGetService( this.GetServiceProvider, serviceType, useOverrides: false, out Object service ) )
			{
				return service;
			}
			else if( !serviceType.IsAbstract && !serviceType.IsInterface && !serviceType.IsGenericTypeDefinition )
			{
				// Otherwise, try to Activate it:
				IServiceProvider sp = this.GetServiceProvider();

				return ActivatorUtilities.CreateInstance( sp, serviceType );
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Converts <paramref name="serviceType"/> into an <see cref="IEnumerable{T}"/> and passes it into <see cref="GetService(Type)"/>. For example:<br />
		/// <c>GetServices(typeof(String))</c> is equivalent to <c>GetService(typeof(IEnumerable&lt;String&gt;))</c>
		/// </summary>
		public IEnumerable<Object> GetServices( Type serviceType )
		{
			if( serviceType is null ) throw new ArgumentNullException( nameof( serviceType ) );

			// This implementation from `Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions`:
			Type closedGenericType = typeof(IEnumerable<>).MakeGenericType( serviceType );

			// Try requesting `IEnumerable<closedGenericType>` first:
			try
			{
				Object enumerableOfServiceType = this.GetService( closedGenericType );
				if( enumerableOfServiceType is IEnumerable<Object> done )
				{
					return done;
				}
				else if( enumerableOfServiceType is IDisposable disp )
				{
					disp.Dispose();
				}
			}
			catch
			{
			}

			// Then request the specific requested type, assuming it works:
			{
				Object serviceInstance = this.GetService( serviceType );
				if( serviceInstance != null )
				{
					return new Object[] { serviceInstance };
				}
			}

			// Otherwise, return an empty array:
			// Is there any advantage to returning `Array.Empty<ServiceType>()` instead?
			return Array.Empty<Object>();
		}

		private IServiceProvider GetServiceProvider()
		{
			HttpContext httpContext = HttpContext.Current;
			return this.di.GetServiceProviderForHttpContext( httpContext );
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
