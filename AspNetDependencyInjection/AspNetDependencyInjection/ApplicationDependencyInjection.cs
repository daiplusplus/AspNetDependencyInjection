using System;
using System.Threading;
using System.Web;
using System.Web.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using AspNetDependencyInjection.Internal;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace AspNetDependencyInjection
{
	/// <summary>Controls the lifespan of the configured <see cref="IServiceCollection"/>. This class implements <see cref="IRegisteredObject"/> to ensure the root <see cref="IServiceProvider"/> is disposed when the <see cref="HostingEnvironment"/> shuts down. Only 1 instance of this class can exist at a time in a single AppDomain.</summary>
	public class ApplicationDependencyInjection : IDisposable, IRegisteredObject
	{
		private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim( initialCount: 1, maxCount: 1 );

		private readonly IServiceCollection                  services;
		private readonly ServiceProvider                     rootServiceProvider;
		private readonly List<IDependencyInjectionClient>    clients = new List<IDependencyInjectionClient>();
		private readonly IDependencyInjectionOverrideService serviceProviderOverrides;

		/// <summary>This dictionary contains <see cref="ObjectFactory"/> delegates that will always return a concrete implementation and never return <c>null</c>.</summary>
		private readonly ConcurrentDictionary<Type,ObjectFactory> objectFactories = new ConcurrentDictionary<Type,ObjectFactory>(); // `ObjectFactory` is a delegate, btw.

		/// <summary>Exposes <see cref="ImmutableApplicationDependencyInjectionConfiguration"/>.</summary>
		public ImmutableApplicationDependencyInjectionConfiguration Configuration { get; }

		/// <summary>Indicates if this <see cref="ApplicationDependencyInjection"/> instance has already been disposed.</summary>
		/// <remarks>This property is not public because consumers using <see cref="ApplicationDependencyInjection"/> (or a subclass) correctly do not *need* to know about this property.</remarks>
		protected internal Boolean IsDisposed { get; private set; }

		/// <summary>Constructor. Does not call any virtual methods. Calls <see cref="ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(IServiceCollection)"/> after using <c>services.TryAdd</c> to add a minimal set of required services.</summary>
		protected internal ApplicationDependencyInjection( ApplicationDependencyInjectionConfiguration configuration, IServiceCollection services )
		{
			// Validate:

			if( configuration == null ) throw new ArgumentNullException(nameof(configuration));
			if( services == null ) throw new ArgumentNullException(nameof(services));

			//

			if( !_semaphore.Wait( millisecondsTimeout: 0 ) )
			{
				throw new InvalidOperationException( "Another " + nameof(ApplicationDependencyInjection) + " has already been created in this AppDomain without being disposed first (or the previous dispose attempt failed)." );
			}

			this.Configuration = configuration.ToImmutable();

			// Register necessary internal services:
			services.TryAddDefaultDependencyInjectionOverrideService();
			services.TryAddSingleton<IServiceProviderAccessor>( sp => new AspNetDependencyInjection.Services.DefaultServiceProviderAccessor( this.Configuration, sp ) );

			// Initialize fields:

			this.services            = services ?? throw new ArgumentNullException(nameof(services));
			this.rootServiceProvider = services.BuildServiceProvider( validateScopes: true );

			this.serviceProviderOverrides = this.rootServiceProvider.GetRequiredService<IDependencyInjectionOverrideService>();

			//

			HostingEnvironment.RegisterObject( this );
			global::Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule( typeof( HttpContextScopeHttpModule ) );
			// NOTE: It is not possible to un-register a HttpModule. That's nothing to do with `Microsoft.Web.Infrastructure` - the actual module registry in `System.Web.dll` can only be added to, not removed from.
		}

		/// <summary>Invokes all of the factory delegates, passing <c>this</c> as the parameter. Then passes the clients into <see cref="UseClients(IEnumerable{IDependencyInjectionClient})"/>.</summary>
		protected internal virtual void CreateClients( IEnumerable<Func<ApplicationDependencyInjection,IDependencyInjectionClient>> clientFactories )
		{
			if( clientFactories == null ) throw new ArgumentNullException(nameof(clientFactories));

			//

			IEnumerable<IDependencyInjectionClient> clients = clientFactories
				.Select( cf => cf( this ) );

			this.UseClients( clients );
		}

		/// <summary>Copies all of the non-null <see cref="IDependencyInjectionClient"/> instances from <paramref name="clients"/> into the private clients list. The clients will be disposed inside <see cref="Dispose(bool)"/>.</summary>
		protected internal virtual void UseClients( IEnumerable<IDependencyInjectionClient> clients )
		{
			if( clients == null ) throw new ArgumentNullException(nameof(clients));

			//

			this.clients.AddRange( clients.Where( c => c != null ) );
		}

#region Lifetime

		/// <summary>Calls <see cref="Dispose()"/>. This method is called by <see cref="HostingEnvironment"/>.</summary>
		/// <param name="immediate">This parameter is unused.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "The method calls Dispose, which is exposed to child types." )]
		void IRegisteredObject.Stop(Boolean immediate)
		{
			this.Dispose();
		}

		/// <summary>Consuming applications should not need to call this method directly as it is called by <see cref="HostingEnvironment"/>. This method calls <see cref="HostingEnvironment.UnregisterObject(IRegisteredObject)"/>, un-sets the <see cref="DependencyInjectionWebObjectActivator"/> from <see cref="HttpRuntime.WebObjectActivator"/> and restores its original value, and calls the <see cref="IDisposable.Dispose"/> method of the root <see cref="ServiceProvider"/>.</summary>
		public void Dispose()
		{
			this.Dispose( disposing: true );
			GC.SuppressFinalize( this ); // NOTE: It isn't necessary to call `SuppressFinalize` if the class doesn't have a finalizer.
		}

		/// <summary>See <see cref="IDisposable.Dispose"/>.</summary>
		/// <param name="disposing">When <c>true</c>, the <see cref="Dispose()"/> method was called. When <c>false</c> the finalizer was invoked.</param>
		protected virtual void Dispose( Boolean disposing )
		{
			if( this.IsDisposed ) return;

			if( disposing )
			{
				HostingEnvironment.UnregisterObject(this);

				IEnumerable<IDependencyInjectionClient> clientsList = this.clients;
				if( clientsList != null )
				{
					foreach( IDependencyInjectionClient client in clientsList )
					{
						client.Dispose();
					}
				}

				this.rootServiceProvider.Dispose();

				_semaphore.Release();
			}

			this.IsDisposed = true;
		}

#endregion

		/// <summary>See the documentation for <see cref="GetServiceProviderForCurrentHttpContext(HttpContextBase)"/>. The <paramref name="httpContext"/> can have a null reference, in which case the root-service provider will be returned.</summary>
		public IServiceProvider GetServiceProviderForCurrentHttpContext( HttpContext httpContext )
		{
			return this.GetServiceProviderForCurrentHttpContext( httpContext == null ? (HttpContextBase)null : new HttpContextWrapper( httpContext ) );
		}

		/// <summary>Gets the current <see cref="IServiceProvider"/> from <paramref name="httpContext"/>'s <see cref="HttpContextBase.Items"/> or <see cref="HttpContextBase.ApplicationInstance"/> as set by <see cref="HttpContextScopeHttpModule"/>. If <paramref name="httpContext"/> is <c>null</c> or if the <see cref="IServiceProvider"/> was not found, a reference to the root <see cref="IServiceProvider"/> is returned.</summary>
		public IServiceProvider GetServiceProviderForCurrentHttpContext( HttpContextBase httpContext )
		{
			if( httpContext != null )
			{
				if( this.Configuration.UseRequestScopes && httpContext.TryGetRequestServiceScope( out IServiceScope requestServiceScope ) ) // This will return false when `UseRequestScopes == false`.
				{
					return requestServiceScope.ServiceProvider;
				}
				else if( this.Configuration.UseHttpApplicationScopes && httpContext.ApplicationInstance.TryGetHttpApplicationServiceScope( out IServiceScope httpApplicationServiceScope ) ) // This will return false when `UseHttpApplicationScopes == true`.
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

		#region ObjectFactories-based service resolution

		/// <summary>Gets the service object of the specified type from the current <see cref="HttpContext"/>. This method will be called by ASP.NET's infrastructure that makes use of <see cref="HttpRuntime.WebObjectActivator"/>. This method never returns a <c>null</c> object reference and will throw an exception if resolution fails.</summary>
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
		}

		/// <summary>Attempts to get the service object of the specified type from the current <see cref="HttpContext"/>. This method returns <c>false</c> if resolution fails (and the value of <paramref name="service"/> is undefined).</summary>
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

		#endregion
	}
}
