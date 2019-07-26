using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Web;

namespace Unity.WebForms
{
	/// <summary>
	/// Implements <see cref="IServiceProvider"/> for use with <see cref="System.Web.HttpRuntime.WebObjectActivator"/>. Based on Microsoft's <c>AspNetWebFormsDependencyInjection</c>'s <c>UnityAdapter</c> available at https://github.com/aspnet/AspNetWebFormsDependencyInjection/tree/master/src/UnityAdapter.
	/// This type does not normally need to be used by web-application code. Registration the <see cref="System.Web.HttpRuntime.WebObjectActivator"/> is handled by <see cref="WebFormsUnityContainerOwner"/>.
	/// </summary>
	public sealed class UnityContainerServiceProvider : IServiceProvider
	{
		private readonly IUnityContainer  container;
		private readonly IServiceProvider next;

		private const    Int32 _defaultUnresolvableTypeCacheSizeLimit = 100000;
		private readonly Int32 unresolvedTypesCacheSizeLimit = _defaultUnresolvableTypeCacheSizeLimit;

		private readonly ConcurrentDictionary<Type,Byte> unresolvedTypesCache = new ConcurrentDictionary<Type,Byte>();

		/// <summary>Instantiates a new instance of <see cref="UnityContainerServiceProvider"/>. You do not need to normally use this constructor directly - instead consider using <see cref="WebFormsUnityContainerOwner"/>.</summary>
		/// <param name="container">Required. The Unity <see cref="IUnityContainer"/> to use for <see cref="System.Web.HttpRuntime.WebObjectActivator"/>.</param>
		/// <param name="next">Optional. A <see cref="IServiceProvider"/> to use as a fallback to resolve types.</param>
		/// <param name="unresolvableTypesCacheSizeLimit">Optional. The maximum size of the cache of unresolvable types. Defaults to 100,000 types.</param>
		/// <exception cref="ArgumentNullException">When <paramref name="container"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">When <paramref name="unresolvableTypesCacheSizeLimit"/> is below zero.</exception>
		public UnityContainerServiceProvider( IUnityContainer container, IServiceProvider next = null, Int32 unresolvableTypesCacheSizeLimit = _defaultUnresolvableTypeCacheSizeLimit )
		{
			this.container                     = container ?? throw new ArgumentNullException( nameof( container ) );
			this.next                          = next;
			this.unresolvedTypesCacheSizeLimit = unresolvableTypesCacheSizeLimit;

			if( this.unresolvedTypesCacheSizeLimit < 0 ) throw new ArgumentOutOfRangeException( message: "Value cannot be less than 0.", paramName: nameof(unresolvableTypesCacheSizeLimit) );
		}

		/// <summary>Gets the service object of the specified type from the current <see cref="HttpContext"/>. This method will be called by ASP.NET's infrastructure that makes use of <see cref="HttpRuntime.WebObjectActivator"/>.</summary>
		public Object GetService( Type serviceType )
		{
			if( serviceType == null ) throw new ArgumentNullException( nameof( serviceType ) );

			if( this.unresolvedTypesCache.ContainsKey( serviceType ) )
			{
				return DefaultCreateInstance( serviceType );
			}

			// At present, with Unity 5, all types will be resolved by the IUnityContainer instance - even if the requested type wasn't explicitly registered with Unity.
			// This can be changed: https://stackoverflow.com/questions/31420619/unity-change-default-lifetime-manager-for-implicit-registrations-and-or-disable
			// ...because right now it means that `unresolvedTypesCache` will never be used.

			Object resolvedInstance;
			try
			{
				HttpContext httpContext = HttpContext.Current;
				if( httpContext != null && httpContext.TryGetChildContainer( out IUnityContainer childContainer ) )
				{
					// The `TryGetChildContainer` is used here when handling 'special' HttpApplication lifetimes. If the Child-Container isn't available then this GetService method isn't being called inside a HTTP request context.
					resolvedInstance = childContainer.Resolve( serviceType );
					if( resolvedInstance != null )
					{
						return resolvedInstance;
					}
				}
				else
				{
					System.Diagnostics.Trace.WriteLine( "HttpContext.Current is null." );
				}

				resolvedInstance = this.container.Resolve( serviceType );				

				if( resolvedInstance != null )
				{
					return resolvedInstance;
				}
			}
			catch( ResolutionFailedException )
			{
				// Ignore and continue to the fallback IServiceProvider.
			}

			System.Diagnostics.Trace.WriteLine( "Could not resolve " + serviceType.FullName + "." );

			if( this.next != null )
			{
				resolvedInstance = this.next.GetService( serviceType );
				if( resolvedInstance != null ) return resolvedInstance;
			}

			resolvedInstance = DefaultCreateInstance( serviceType );

			if( this.unresolvedTypesCache.Count < this.unresolvedTypesCacheSizeLimit )
			{
				this.unresolvedTypesCache.TryAdd( serviceType, 0 );
			}

			return resolvedInstance;
		}

		private static Object DefaultCreateInstance( Type type )
		{
			return Activator.CreateInstance
			(
				type                : type,
				bindingAttr         : BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance,
				binder              : null,
				args                : null,
				culture             : null,
				activationAttributes: null
			);
		}

		/// <summary>The set of unresolved types that have been encountered so far.</summary>
		public IEnumerable<Type> UnresolvedTypes => this.unresolvedTypesCache.Keys;

		/// <summary>Removes this instance as the <see cref="HttpRuntime.WebObjectActivator"/> container. Returns <c>true</c> if this instance was removed as the container. Returns <c>false</c> if this instance was not currently registered as the container.</summary>
		internal Boolean RemoveAsWebObjectActivator()
		{
			lock( _httpRuntimeLock )
			{
				if( HttpRuntime.WebObjectActivator == this )
				{
					HttpRuntime.WebObjectActivator = this.next;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		private static readonly Object _httpRuntimeLock = new Object();

		/// <summary>Sets the provided container as the <see cref="HttpRuntime.WebObjectActivator"/> by wrapping it in a <see cref="UnityContainerServiceProvider"/> which is then returned.</summary>
		public static UnityContainerServiceProvider SetWebObjectActivatorContainer( IUnityContainer container )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			lock( _httpRuntimeLock )
			{
				IServiceProvider currentServiceProvider = HttpRuntime.WebObjectActivator;

				UnityContainerServiceProvider ucsp = new UnityContainerServiceProvider( container, currentServiceProvider );

				HttpRuntime.WebObjectActivator = ucsp;

				return ucsp;
			}
		}
	}
}
