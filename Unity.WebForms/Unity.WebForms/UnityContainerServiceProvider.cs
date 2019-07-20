using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Hosting;

namespace Unity.WebForms
{
	/// <summary>Implements <see cref="IServiceProvider"/> for use with <see cref="System.Web.HttpRuntime.WebObjectActivator"/>. Based on Microsoft's <c>AspNetWebFormsDependencyInjection</c>'s <c>UnityAdapter</c> available at https://github.com/aspnet/AspNetWebFormsDependencyInjection/tree/master/src/UnityAdapter.</summary>
	public sealed class UnityContainerServiceProvider : IServiceProvider
	{
		private readonly IUnityContainer  container;
		private readonly IServiceProvider next;

		private const    Int32 _defaultUnresolvableTypeCacheSizeLimit = 100000;
		private readonly Int32 unresolvedTypesCacheSizeLimit = _defaultUnresolvableTypeCacheSizeLimit;

		private readonly ConcurrentDictionary<Type,Byte> unresolvedTypesCache = new ConcurrentDictionary<Type,Byte>();

		/// <summary>Instantiates a new instance of <see cref="UnityContainerServiceProvider"/>. You do not need to normally use this constructor directly - instead consider using <see cref="WebObjectActivatorSetup"/>.</summary>
		/// <param name="container">Required. The Unity <see cref="IUnityContainer"/> to use for <see cref="System.Web.HttpRuntime.WebObjectActivator"/>.</param>
		/// <param name="next">Optional. A <see cref="IServiceProvider"/> to use as a fallback to resolve types.</param>
		/// <param name="unresolvableTypeCacheSizeLimit">Optional. The maximum size of the cache of unresolvable types. Defaults to 100,000 types.</param>
		/// <exception cref="ArgumentNullException">When <paramref name="container"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">When <paramref name="unresolvableTypeCacheSizeLimit"/> is below zero.</exception>
		public UnityContainerServiceProvider( IUnityContainer container, IServiceProvider next = null, Int32 unresolvableTypesCacheSizeLimit = _defaultUnresolvableTypeCacheSizeLimit )
		{
			this.container                     = container ?? throw new ArgumentNullException( nameof( container ) );
			this.next                          = next;
			this.unresolvedTypesCacheSizeLimit = unresolvableTypesCacheSizeLimit;

			if( this.unresolvedTypesCacheSizeLimit < 0 ) throw new ArgumentOutOfRangeException( message: "Value cannot be less than 0.", paramName: nameof(unresolvableTypesCacheSizeLimit) );
		}

		public Object GetService( Type serviceType )
		{
			if( serviceType == null ) throw new ArgumentNullException( nameof( serviceType ) );

			if( this.unresolvedTypesCache.ContainsKey( serviceType ) )
			{
				return DefaultCreateInstance( serviceType );
			}

			Object resolvedInstance;
			try
			{
				resolvedInstance = this.container.Resolve( serviceType );
				if( resolvedInstance != null ) return resolvedInstance;
			}
			catch( ResolutionFailedException )
			{
				// Ignore and continue to the fallback IServiceProvider.
			}

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

		private static readonly Object _httpRuntimeLock = new Object();

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
