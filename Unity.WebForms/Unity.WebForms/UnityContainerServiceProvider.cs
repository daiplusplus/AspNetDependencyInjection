using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

using Unity.WebForms.Configuration;
using Unity.WebForms.Services;

namespace Unity.WebForms.Internal
{
	/// <summary></summary>
	/// <remarks>While this class implements <see cref="IServiceProvider"/> it is not intended to be used as a DI service-provider for use with Microsoft.Extensions.DependencyInjection - it is only so it can be used with <see cref="System.Web.HttpRuntime.WebObjectActivator"/>.</remarks>
	public sealed class MediWebObjectActivator : IServiceProvider
	{
		private readonly IServiceProvider rootServiceProvider;
		private readonly IServiceProvider fallback;
		private readonly IAspNetDIExclusionService excluded;

		private readonly ConcurrentDictionary<Type,ObjectFactory> objectFactories = new ConcurrentDictionary<Type,ObjectFactory>(); // `ObjectFactory` is a delegate, btw.

		/// <summary>Instantiates a new instance of <see cref="MediWebObjectActivator"/>. You do not need to normally use this constructor directly - instead consider using <see cref="WebFormsUnityContainerOwner"/>.</summary>
		/// <param name="rootServiceProvider">Required. The <see cref="IServiceProvider"/> container or service-provider to use for <see cref="HttpRuntime.WebObjectActivator"/>.</param>
		/// <param name="fallback">Optional. A <see cref="IServiceProvider"/> to use as a fallback to resolve types.</param>
		/// <param name="excluded">Required. A service which indicates which types and namespaces should be excluded from DI and always constructed by <see cref="Activator"/>.</param>
		/// <exception cref="ArgumentNullException">When <paramref name="rootServiceProvider"/> or <paramref name="excluded"/> is <c>null</c>.</exception>
		public MediWebObjectActivator( IServiceProvider rootServiceProvider, IServiceProvider fallback, IAspNetDIExclusionService excluded )
		{
			this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException( nameof(rootServiceProvider) );
			this.fallback            = fallback;
			this.excluded            = excluded ?? throw new ArgumentNullException(nameof(excluded));
		}

		/// <summary>Gets the service object of the specified type from the current <see cref="HttpContext"/>. This method will be called by ASP.NET's infrastructure that makes use of <see cref="HttpRuntime.WebObjectActivator"/>.</summary>
		// IMPORTANT NOTE: This method MUST return an instantiated serviceType - or throw an exception. i.e. it cannot return null - so if the root IServiceProvider returns null then fallback to (completely different serviceProviders) - otherwise throw.
		public Object GetService( Type serviceType )
		{
			if( serviceType == null ) throw new ArgumentNullException( nameof(serviceType) );

			// Shortcut any `System.Web.*` types to always use Activator:
			if( this.excluded.IsExcluded( serviceType ) )
			{
				ObjectFactory objectFactory = this.objectFactories.GetOrAdd( key: serviceType, valueFactory: ActivatorObjectFactoryFactory );
				return objectFactory( serviceProvider: null, arguments: null ); // Note that `serviceProvider: null` because it isn't needed when using Activator.
			}
			else
			{
				IServiceProvider serviceProvider = this.GetServiceProviderForCurrentHttpContext();

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

		private IServiceProvider GetServiceProviderForCurrentHttpContext()
		{
			// As WebObjectActivator will always be called from an ASP.NET Request-Thread-Pool-thread, HttpContext.Current *should* always be non-null.
			HttpContext httpContext = HttpContext.Current;
			// TODO: Given the above, consider throwing an exception if `HttpContext.Current == null`?

			if( httpContext != null )
			{
				if( httpContext.TryGetRequestServiceScope( out IServiceScope requestServiceScope ) )
				{
					return requestServiceScope.ServiceProvider;
				}
				else if( httpContext.ApplicationInstance.TryGetApplicationServiceProvider( out IServiceProvider applicationServiceProvider ) )
				{
					return applicationServiceProvider;
				}
				else
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
			// This is a two-step operation:
			// 1. The first pass tests that the object is available from the serviceProvider, and if so, returns an ObjectFactory that uses the serviceProvider (regardless of scope depth).
			// 2. Otherwise, repeats the process for the fallback.
			// 3. If nothing else works, then it returns an ObjectFactory for MEDI's ActivatorUtilities or Activator.
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
			if( this.fallback != null )
			{
				Object result = helper.ServiceProvider.GetService( serviceType ); // Btw, don't use `GetRequiredService(Type)` because that throws an exception if it fails.
				if( result != null )
				{
					helper.Instance = result;
					return new ObjectFactory( ( sp, args ) => this.fallback.GetRequiredService( serviceType ) ); // Observe that it always uses `this.fallback` and ignores the passed-in IServiceProvider.
				}
			}

			// 3:
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
