using System;
using System.Collections.Concurrent;
using System.Web;

using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection.Internal
{
	/// <summary></summary>
	/// <remarks>While this class implements <see cref="IServiceProvider"/> it is not intended to be used as a DI service-provider for use with Microsoft.Extensions.DependencyInjection - it is only so it can be used with <see cref="System.Web.HttpRuntime.WebObjectActivator"/>.</remarks>
	public sealed class DependencyInjectionWebObjectActivator : IServiceProvider
	{
		private readonly ImmutableApplicationDependencyInjectionConfiguration config;

		private readonly IServiceProvider rootServiceProvider;
		private readonly IDependencyInjectionOverrideService serviceProviderOverrides;

		/// <summary>This dictionary contains <see cref="ObjectFactory"/> delegates that will always return a concrete implementation and never return <c>null</c>.</summary>
		private readonly ConcurrentDictionary<Type,ObjectFactory> objectFactories = new ConcurrentDictionary<Type,ObjectFactory>(); // `ObjectFactory` is a delegate, btw.

		/// <summary>Instantiates a new instance of <see cref="DependencyInjectionWebObjectActivator"/>. You do not need to normally use this constructor directly - instead use <see cref="ApplicationDependencyInjection"/>.</summary>
		/// <param name="configuration">Required.</param>
		/// <param name="rootServiceProvider">Required. The root <see cref="IServiceProvider"/> to use. The actual <see cref="IServiceProvider"/> used inside <see cref="GetService(Type)"/> depends on the current <see cref="HttpContext.Current"/>.</param>
		/// <param name="serviceProviderOverrideService">Required. A service which allows a custom <see cref="IServiceProvider"/> to always be used for certain types.</param>
		/// <exception cref="ArgumentNullException">When <paramref name="rootServiceProvider"/> or <paramref name="serviceProviderOverrideService"/> is <c>null</c>.</exception>
		public DependencyInjectionWebObjectActivator( ImmutableApplicationDependencyInjectionConfiguration configuration, IServiceProvider rootServiceProvider, IDependencyInjectionOverrideService serviceProviderOverrideService )
		{
			this.config                   = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.rootServiceProvider      = rootServiceProvider ?? throw new ArgumentNullException( nameof(rootServiceProvider) );
			this.serviceProviderOverrides = serviceProviderOverrideService ?? throw new ArgumentNullException(nameof(serviceProviderOverrideService));
		}

		/// <summary>Gets the service object of the specified type from the current <see cref="HttpContext"/>. This method will be called by ASP.NET's infrastructure that makes use of <see cref="HttpRuntime.WebObjectActivator"/>.</summary>
		// IMPORTANT NOTE: This method MUST return an instantiated serviceType - or throw an exception. i.e. it cannot return null - so if the root IServiceProvider returns null then fallback to (completely different serviceProviders) - otherwise throw.
		public Object GetService( Type serviceType )
		{
			if( serviceType == null ) throw new ArgumentNullException( nameof(serviceType) );

			if( this.serviceProviderOverrides.TryGetServiceProvider( serviceType, out IServiceProvider fallbackServiceProvider ) )
			{
				return fallbackServiceProvider.GetRequiredService( serviceType );
			}
			else
			{
				IServiceProvider serviceProvider = this.GetServiceProviderForCurrentHttpContext();

				// Optimziation: TryGet at first to avoid having to create ObjectFactoryHelper if we really don't need it:
				if( this.objectFactories.TryGetValue( serviceType, out ObjectFactory existingObjectFactory ) )
				{
					return existingObjectFactory( serviceProvider, arguments: null );
				}
				else
				{
					ObjectFactoryHelper helper = new ObjectFactoryHelper( serviceProvider );

					ObjectFactory objectFactory = this.objectFactories.GetOrAdd( key: serviceType, valueFactory: this.DefaultObjectFactoryFactory, factoryArgument: helper );

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
		}

		public Boolean TryGetService( Type serviceType, Boolean useOverrides, out Object service )
		{
			if( serviceType == null ) throw new ArgumentNullException( nameof(serviceType) );

			if( useOverrides && this.serviceProviderOverrides.TryGetServiceProvider( serviceType, out IServiceProvider serviceProviderOverride ) )
			{
				service = serviceProviderOverride.GetService( serviceType );
				return service != null;
			}
			else
			{
				IServiceProvider serviceProvider = this.GetServiceProviderForCurrentHttpContext();

				// Optimization: Does the serviceType already exist? (i.e. it has an implementation that's been called before)?
				if( this.objectFactories.TryGetValue( serviceType, out ObjectFactory existingObjectFactory ) )
				{
					service = existingObjectFactory( serviceProvider, arguments: null );
					return service != null; // TODO: Change this to an assertion that `service != null` because ObjectFactories in `this.objectFactories` must never return null.
				}
				else
				{
					// Return from serviceProvider directly. Do not use `DefaultObjectFactoryFactory` because we don't want to use Activator (which doesn't work with interfaces and abstract types).
					service = serviceProvider.GetService( serviceType );
					return service != null;
				}
			}
		}

		private IServiceProvider GetServiceProviderForCurrentHttpContext()
		{
			// As WebObjectActivator will always be called from an ASP.NET Request-Thread-Pool-thread, HttpContext.Current *should* always be non-null.
			HttpContext httpContext = HttpContext.Current;
			// TODO: Given the above, consider throwing an exception if `HttpContext.Current == null`?

			if( httpContext != null )
			{
				if( this.config.UseRequestScopes && httpContext.TryGetRequestServiceScope( out IServiceScope requestServiceScope ) ) // This will return false when `UseRequestScopes == false`.
				{
					return requestServiceScope.ServiceProvider;
				}
				else if( this.config.UseHttpApplicationScopes && httpContext.ApplicationInstance.TryGetHttpApplicationServiceScope( out IServiceScope httpApplicationServiceScope ) ) // This will return false when `UseHttpApplicationScopes == true`.
				{
					return httpApplicationServiceScope.ServiceProvider;
				}
				else if( httpContext.ApplicationInstance.TryGetRootServiceProvider( out IServiceProvider httpApplicationRootServiceProvider ) ) // This should never return false
				{
					return httpApplicationRootServiceProvider;
				}
				else // This should never happen, but just-in-case:
				{
					return this.rootServiceProvider;
				}
			}
			else
			{
				return this.rootServiceProvider;
			}
		}

		// IMPORTANT NOTE: This method does not necessarily *create* (well, resolve) service instances itself - it returns a delegate which in-turn performs the resolution process - but as an optimization it returns the resolved service, if available as part of the testing process.
		private ObjectFactory DefaultObjectFactoryFactory( Type serviceType, ObjectFactoryHelper helper )
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
