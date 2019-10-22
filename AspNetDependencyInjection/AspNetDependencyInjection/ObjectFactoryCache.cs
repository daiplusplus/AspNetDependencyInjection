using System;
using System.Collections.Concurrent;

using AspNetDependencyInjection.Internal;

using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection
{
	/// <summary>Caches object factories. Necessary for some high-performance situations, such as ASP.NET WebForm's <see cref="System.Web.HttpRuntime.WebObjectActivator"/>.</summary>
	public class ObjectFactoryCache
	{
		private readonly IDependencyInjectionOverrideService serviceProviderOverrides;
		private readonly IServiceProviderAccessor serviceProviderAccessor;

		/// <summary>This dictionary contains <see cref="ObjectFactory"/> delegates that will always return a concrete implementation and never return <c>null</c>.</summary>
		private readonly ConcurrentDictionary<Type,ObjectFactory> objectFactories = new ConcurrentDictionary<Type,ObjectFactory>(); // `ObjectFactory` is a delegate, btw.

		/// <summary>Constructor.</summary>
		public ObjectFactoryCache( IDependencyInjectionOverrideService serviceProviderOverrides, IServiceProviderAccessor serviceProviderAccessor )
		{
			this.serviceProviderOverrides = serviceProviderOverrides ?? throw new ArgumentNullException(nameof(serviceProviderOverrides));
			this.serviceProviderAccessor = serviceProviderAccessor ?? throw new ArgumentNullException( nameof( serviceProviderAccessor ) );
		}

		private IServiceProvider GetRootServiceProvider()
		{
			return this.serviceProviderAccessor.RootServiceProvider;
		}

		/// <summary>Gets the service object of the specified type from <see cref="ApplicationDependencyInjection.RootServiceProvider"/>. This method never returns a <c>null</c> object reference and will throw an exception if resolution fails.</summary>
		public Object GetRequiredRootService( Type serviceType, Boolean useOverrides )
		{
			return this.GetRequiredService( this.GetRootServiceProvider, serviceType, useOverrides: useOverrides );
		}

		/// <summary>Gets the service object of the specified type from <paramref name="getServiceProvider"/>. This method never returns a <c>null</c> object reference and will throw an exception if resolution fails.</summary>
		// IMPORTANT NOTE: This method MUST return an instantiated serviceType - or throw an exception. i.e. it cannot return null - so if the root IServiceProvider returns null then fallback to (completely different serviceProviders) - otherwise throw.
		public Object GetRequiredService( Func<IServiceProvider> getServiceProvider, Type serviceType, Boolean useOverrides )
		{
			if( getServiceProvider == null ) throw new ArgumentNullException( nameof(getServiceProvider) );
			if( serviceType == null ) throw new ArgumentNullException( nameof(serviceType) );

			//

			if( useOverrides && this.serviceProviderOverrides.TryGetServiceProvider( serviceType, out IServiceProvider fallbackServiceProvider ) )
			{
				return fallbackServiceProvider.GetRequiredService( serviceType );
			}
			else
			{
				IServiceProvider serviceProvider = getServiceProvider();
				if( serviceProvider == null ) throw new InvalidOperationException( nameof(getServiceProvider) + " returned null." );

				return this.GetRequiredService( serviceProvider, serviceType );
			}
		}

		/// <summary>Gets the service object of the specified type from <paramref name="serviceProvider"/>. This method never returns a <c>null</c> object reference and will throw an exception if resolution fails.</summary>
		public Object GetRequiredService( IServiceProvider serviceProvider, Type serviceType )
		{
			if( serviceProvider == null ) throw new ArgumentNullException(nameof(serviceProvider));
			if( serviceType == null ) throw new ArgumentNullException(nameof(serviceType));
			
			//


			// Optimziation: TryGet at first to avoid having to create ObjectFactoryHelper if we really don't need it:
			if( this.objectFactories.TryGetValue( serviceType, out ObjectFactory existingObjectFactory ) )
			{
				return existingObjectFactory( serviceProvider, arguments: null );
			}
			else
			{
				ObjectFactoryHelper helper = new ObjectFactoryHelper( serviceProvider );

				ObjectFactory objectFactory = this.objectFactories.GetOrAdd( key: serviceType, valueFactory: this.RequiredObjectFactoryFactory, factoryArgument: helper );

				if( helper.Instance != null )
				{
					// A new service was requested for the first time, which necessarily meant creating it as part of the ObjectFactory-building process, so return it to avoid invoking the ObjectFactory twice:
					return helper.Instance;
				}
				else
				{
					// Otherwise, an existing ObjectFactory was returned (or a test helper instance wasn't created), so use the ObjectFactory:
					return objectFactory( serviceProvider: serviceProvider, arguments: null );
				}
			}
		}

		/// <summary>Generic version of <see cref="GetRequiredService(IServiceProvider, Type)"/>.</summary>
		public T GetRequiredService<T>( IServiceProvider serviceProvider )
		{
			return (T)this.GetRequiredService( serviceProvider, typeof(T) );
		}

		/// <summary>Gets the service object of the specified type from <see cref="ApplicationDependencyInjection.RootServiceProvider"/>. This method never returns a <c>null</c> object reference and will throw an exception if resolution fails.</summary>
		public Boolean TryGetRootService( Type serviceType, Boolean useOverrides, out Object service )
		{
			return this.TryGetService( this.GetRootServiceProvider, serviceType, useOverrides: useOverrides, service: out service );
		}

		/// <summary>Gets the service object of the specified type from <see cref="ApplicationDependencyInjection.RootServiceProvider"/>. This method never returns a <c>null</c> object reference and will throw an exception if resolution fails.</summary>
		public Boolean TryGetRootService( Type serviceType, out Object service )
		{
			return this.TryGetService( this.GetRootServiceProvider(), serviceType, service: out service );
		}

		/// <summary>Attempts to get the service object of the specified type from the current <paramref name="getServiceProvider"/> result. This method returns <c>false</c> if resolution fails (and the value of <paramref name="service"/> is undefined).</summary>
		public Boolean TryGetService( Func<IServiceProvider> getServiceProvider, Type serviceType, Boolean useOverrides, out Object service )
		{
			if( getServiceProvider == null ) throw new ArgumentNullException( nameof(getServiceProvider) );
			if( serviceType == null ) throw new ArgumentNullException( nameof(serviceType) );

			//

			if( useOverrides && this.serviceProviderOverrides.TryGetServiceProvider( serviceType, out IServiceProvider serviceProviderOverride ) )
			{
				service = serviceProviderOverride.GetService( serviceType );
				return service != null;
			}
			else
			{
				IServiceProvider serviceProvider = getServiceProvider();
				if( serviceProvider == null ) throw new InvalidOperationException( nameof(getServiceProvider) + " returned null." );

				return this.TryGetService( serviceProvider, serviceType, out service );
			}
		}

		/// <summary>Attempts to get the service object of the specified type from the current <paramref name="serviceProvider"/> result. This method returns <c>false</c> if resolution fails (and the value of <paramref name="service"/> is undefined).</summary>
		public Boolean TryGetService( IServiceProvider serviceProvider, Type serviceType, out Object service )
		{
			if( serviceProvider == null ) throw new ArgumentNullException( nameof(serviceProvider) );
			if( serviceType == null ) throw new ArgumentNullException( nameof(serviceType) );

			//

			// Optimization: Does the serviceType already exist? (i.e. it has an implementation that's been called before)?
			if( this.objectFactories.TryGetValue( serviceType, out ObjectFactory existingObjectFactory ) )
			{
				service = existingObjectFactory( serviceProvider, arguments: null );
				if( service == null ) throw new InvalidOperationException( nameof(ObjectFactory) + " returned null. ObjectFactory instances must only be added if they can create or retrieve a valid service instance." );

				return true;
			}
			else
			{
				// Return from serviceProvider directly. Do not use `DefaultObjectFactoryFactory` because we don't want to use Activator (which doesn't work with interfaces and abstract types).
				service = serviceProvider.GetService( serviceType );
				return service != null;
			}
		}

		/// <summary>Generic version of <see cref="TryGetService{T}(IServiceProvider, out T)"/>.</summary>
		public Boolean TryGetService<T>( IServiceProvider serviceProvider, out T service )
		{
			if( this.TryGetService( serviceProvider, typeof(T), out Object serviceObj ) )
			{
				service = (T)serviceObj;
				return true;
			}
			else
			{
				service = default;
				return false;
			}
		}

		// IMPORTANT NOTE: This method does not necessarily *create* (well, resolve) service instances itself - it returns a delegate which in-turn performs the resolution process - but as an optimization it returns the resolved service, if available as part of the testing process.
		private ObjectFactory RequiredObjectFactoryFactory( Type serviceType, ObjectFactoryHelper helper )
		{
			// This is a convoluted operation:
			// 1. The first pass tests that the object is available from the serviceProvider, and if so, returns an ObjectFactory that uses the serviceProvider (regardless of scope depth).
			// 2. If nothing else works, then it returns an ObjectFactory for MEDI's ActivatorUtilities or Activator.
			// Now, this COULD work by performing the test, then discarding the returned object, and then invoking the ObjectFactory anyway...
			// but instead, we use the method argument to pass the returned object back to the caller to prevent needing to invoke the ObjectFactory twice whenever a new serviceType is requested for the first time.

			// 1:
			{
				Object result = helper.ServiceProvider.GetService( serviceType ); // Btw, don't use `GetRequiredService(Type)` because that throws an exception if it fails.
				if( result != null )
				{
					helper.Instance = result;
					return new ObjectFactory( ( sp, args ) => sp.GetRequiredService( serviceType ) ); // this ObjectFactory will be used in future calls to GetService.
				}
			}

			// 2:
			try
			{
				// ActivatorUtilities.CreateFactory only throws InvalidOperationException if it cannot find a suitable constructor.
				// As the `ObjectFactory` itself isn't actually being invoked, we can be certain the exception is not being thrown from anywhere else.
				// See `CreateFactory` and `FindApplicableConstructor` in https://github.com/aspnet/DependencyInjection/blob/1.0.0-rc1/src/Microsoft.Extensions.DependencyInjection.Abstractions/ActivatorUtilities.cs

				return ActivatorUtilities.CreateFactory( instanceType: serviceType, argumentTypes: Array.Empty<Type>() );
			}
			catch( InvalidOperationException )
			{
				// Fallback to Activator:
				return ActivatorObjectFactoryFactory( serviceType );
			}
		}

		private static ObjectFactory ActivatorObjectFactoryFactory( Type serviceType )
		{
			return new ObjectFactory( ( sp, args ) => ActivatorServiceProvider.Instance.GetService( serviceType: serviceType, args: args ) ); // Yay closures.
		}

		private class ObjectFactoryHelper
		{
			public readonly IServiceProvider ServiceProvider;
			public          Object           Instance;

			public ObjectFactoryHelper(IServiceProvider serviceProvider)
			{
				this.ServiceProvider = serviceProvider;
			}
		}
	}
}
