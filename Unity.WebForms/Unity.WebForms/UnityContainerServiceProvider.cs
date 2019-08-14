using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

using Unity.WebForms.Configuration;

namespace Unity.WebForms.Internal
{
	public sealed class MediWebObjectActivatorServiceProvider : IServiceProvider
	{
		private static readonly NamespacePrefix _systemWebNamespacePrefix          = new NamespacePrefix( "System.Web" );
		private static readonly NamespacePrefix _systemServiceModelNamespacePrefix = new NamespacePrefix( "System.ServiceModel" );

		private readonly IServiceProvider rootServiceProvider;
		private readonly IServiceProvider next;
		private readonly Action<Exception,Type> onUnresolvedType;
//		private readonly IServiceProvider applicationServiceProvider;

		private readonly ConcurrentDictionary<Type,ObjectFactory> typeFactories = new ConcurrentDictionary<Type,ObjectFactory>(); // `ObjectFactory` is a delegate, btw.

		/// <summary>Instantiates a new instance of <see cref="MediWebObjectActivatorServiceProvider"/>. You do not need to normally use this constructor directly - instead consider using <see cref="WebFormsUnityContainerOwner"/>.</summary>
		/// <param name="rootServiceProvider">Required. The <see cref="IServiceProvider"/> container or service-provider to use for <see cref="HttpRuntime.WebObjectActivator"/>.</param>
		/// <param name="next">Optional. A <see cref="IServiceProvider"/> to use as a fallback to resolve types.</param>
		/// <param name="onUnresolvedType">Optional. A callback invoked when the specified <see cref="Type"/> cannot be resolved. This is intended to be used for logging.</param>
		/// <exception cref="ArgumentNullException">When <paramref name="rootServiceProvider"/> is <c>null</c>.</exception>
		public MediWebObjectActivatorServiceProvider( IServiceProvider rootServiceProvider, IServiceProvider next, Action<Exception,Type> onUnresolvedType )
		{
			this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException( nameof(rootServiceProvider) );
			this.next                = next;
		}

		/// <summary>Gets the service object of the specified type from the current <see cref="HttpContext"/>. This method will be called by ASP.NET's infrastructure that makes use of <see cref="HttpRuntime.WebObjectActivator"/>.</summary>
		public Object GetService( Type serviceType )
		{
			if( serviceType == null ) throw new ArgumentNullException( nameof(serviceType) );

			// Shortcut any `System.Web.*` types to use an equivalent code-path (unfortunately we can't simply return `null`):
			if( _systemWebNamespacePrefix.Matches( serviceType.FullName ) || _systemServiceModelNamespacePrefix.Matches( serviceType.FullName ) )
			{
				ObjectFactory objectFactory = this.typeFactories.GetOrAdd( key: serviceType, valueFactory: SystemWebObjectFactoryFactory );
				return objectFactory( serviceProvider: null, arguments: null );
			}

			try
			{
				IServiceProvider serviceProvider = this.GetServiceProvider();

				return this.GetService( serviceType, serviceProvider );
			}
			catch( Exception ex )
			{
				this.onUnresolvedType?.Invoke( ex, serviceType );

				throw; // TODO: Remove this try/catch as we don't need it anymore...
			}
		}

		private IServiceProvider GetServiceProvider()
		{
			HttpContext httpContext = HttpContext.Current;
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

		private Object GetService( Type serviceType, IServiceProvider serviceProvider )
		{
			// Don't use `GetRequiredService(Type)` because that throws an exception if it fails.
			// However ASP.NET will request LOTS of types using WebObjectActivator, including many of its own, like `HttpEncoder` not just those that are used as constructor parameters.
			if( serviceProvider != null )
			{
				Object result = serviceProvider.GetService( serviceType );
				if( result != null ) return result;
			}

			// Fallback 1: `next` (or `prev` depending on your perspective):
			if( this.next != null )
			{
				Object result = this.next.GetService( serviceType );
				if( result != null ) return result;
			}

			// Fallback 2: MEDI's object factory-factory (ActivatorUtilities.CreateFactory). Though the ServiceProvider passed-in will be ignored.
			try
			{
				ObjectFactory objectFactory = this.typeFactories.GetOrAdd( key: serviceType, valueFactory: MediObjectFactoryFactory );
				return objectFactory( serviceProvider: serviceProvider, arguments: null );
			}
			catch( InvalidOperationException ex )
			{
				// Fallback 3: Activator.CreateInstance.

				// Replace any existing factory, because it causes InvalidOperationException.
				ObjectFactory fallbackFactory = this.typeFactories[ key: serviceType ] = SystemWebObjectFactoryFactory( serviceType );
				return fallbackFactory( serviceProvider, arguments: null );
			}
		}

		private static ObjectFactory MediObjectFactoryFactory( Type serviceType )
		{
			return ActivatorUtilities.CreateFactory( instanceType: serviceType, argumentTypes: Array.Empty<Type>() );
		}

		private static ObjectFactory SystemWebObjectFactoryFactory( Type serviceType )
		{
			ObjectFactoryHolder holder = new ObjectFactoryHolder( serviceType ); // ...or just use a closure?
			return new ObjectFactory( holder.Create );
		}

		private class ObjectFactoryHolder
		{
			private readonly Type type;

			public ObjectFactoryHolder( Type type )
			{
				this.type = type ?? throw new ArgumentNullException(nameof(type));
			}

			/// <summary><paramref name="sp"/> is ignored.</summary>
			public Object Create( IServiceProvider sp, Object[] args )
			{
				return DefaultCreateInstance( type: this.type, args: args );
			}
		}

		/// <summary>An <see cref="ObjectFactory"/>.</summary>
		private static Object DefaultCreateInstance( Type type )
		{
			return DefaultCreateInstance( type, args: null );
		}

		private static Object DefaultCreateInstance( Type type, params Object[] args )
		{
			return ActivatorServiceProvider.Instance.GetService( serviceType: type, args: args );
		}
	}

	/// <summary>Does not perform any caching or object lifetime management - everything is transient.</summary>
	internal class ActivatorServiceProvider : IServiceProvider
	{
		public static ActivatorServiceProvider Instance { get; } = new ActivatorServiceProvider();

		private ActivatorServiceProvider()
		{
		}

		public Object GetService( Type serviceType )
		{
			return GetService( serviceType );
		}

		public Object GetService( Type serviceType, params Object[] args )
		{
			return Activator.CreateInstance
			(
				type                : serviceType,
				bindingAttr         : BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance,
				binder              : null,
				args                : args,
				culture             : null,
				activationAttributes: null
			);
		}
	}
}
