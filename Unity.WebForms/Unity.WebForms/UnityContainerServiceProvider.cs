using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Unity.WebForms.Internal
{
	public sealed class MediWebObjectActivatorServiceProvider : IServiceProvider
	{
		private readonly IServiceProvider rootServiceProvider;
		private readonly IServiceProvider next;
		private readonly Action<Exception,Type> onUnresolvedType;
//		private readonly IServiceProvider applicationServiceProvider;

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

			Object resolvedInstance;
			try
			{
				HttpContext httpContext = HttpContext.Current;
				if( httpContext != null )
				{
					if( httpContext.TryGetRequestServiceScope( out IServiceScope requestServiceScope ) )
					{
						return requestServiceScope.ServiceProvider.GetRequiredService( serviceType );
					}
					else if( httpContext.ApplicationInstance.TryGetApplicationServiceProvider( out IServiceProvider applicationServiceProvider ) )
					{
						return applicationServiceProvider.GetRequiredService( serviceType );
					}
				}

				// Fallback to root:
				return this.rootServiceProvider.GetRequiredService( serviceType );
			}
			catch( Exception ex )
			{
				this.onUnresolvedType?.Invoke( ex, serviceType );
			}

			if( this.next != null )
			{
				resolvedInstance = this.next.GetService( serviceType );
				if( resolvedInstance != null ) return resolvedInstance;
			}

			resolvedInstance = DefaultCreateInstance( serviceType );

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
	}
}
