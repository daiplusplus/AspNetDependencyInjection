using System;
using System.Web;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>Implementation of <see cref="IServiceProvider"/> intended for use ONLY as <see cref="HttpRuntime.WebObjectActivator"/>. The behaviour of <see cref="GetService(Type)"/> always returns a requested type or throws an exception.</summary>
	/// <remarks>While this class implements <see cref="IServiceProvider"/> it is not intended to be used as a DI service-provider for use with Microsoft.Extensions.DependencyInjection - it is only so it can be used with <see cref="System.Web.HttpRuntime.WebObjectActivator"/>.</remarks>
	public sealed class DependencyInjectionWebObjectActivator : IServiceProvider, IDependencyInjectionClient
	{
		private readonly ApplicationDependencyInjection di;

		/// <summary>Instantiates a new instance of <see cref="DependencyInjectionWebObjectActivator"/>. You do not need to normally use this constructor directly - instead use <see cref="ApplicationDependencyInjection"/>.</summary>
		/// <param name="di">Required.</param>
		public DependencyInjectionWebObjectActivator( ApplicationDependencyInjection di )
		{
			this.di = di ?? throw new ArgumentNullException(nameof(di));

			HttpRuntime.WebObjectActivator = this;
		}

		/// <summary>Gets the service object of the specified type from the current <see cref="HttpContext"/>. This method should ONLY be called by ASP.NET's infrastructure via <see cref="HttpRuntime.WebObjectActivator"/>. This method never returns a <c>null</c> object reference and will throw an exception if resolution fails.</summary>
		// IMPORTANT NOTE: This method MUST return an instantiated serviceType - or throw an exception. i.e. it cannot return null - so if the root IServiceProvider returns null then fallback to (completely different serviceProviders) - otherwise throw.
		public Object GetService( Type serviceType )
		{
			return this.di.ObjectFactoryCache.GetRequiredService( this.GetServiceProviderForCurrentHttpContext, serviceType, useOverrides: true );
		}

		private IServiceProvider GetServiceProviderForCurrentHttpContext()
		{
			// As WebObjectActivator will always be called from an ASP.NET Request-Thread-Pool-thread, HttpContext.Current *should* always be non-null.
			HttpContext httpContext = HttpContext.Current;
			if( httpContext == null ) throw new InvalidOperationException( "HttpContext.Current is null." ); // This should never happen, provided only ASP.NET is using `DependencyInjectionWebObjectActivator`.

			return this.di.GetServiceProviderForCurrentHttpContext( httpContext );
		}

		/// <summary>Unsets <see cref="HttpRuntime.WebObjectActivator"/> only if its value is this instance.</summary>
		public void Dispose()
		{
			if( HttpRuntime.WebObjectActivator == this )
			{
				HttpRuntime.WebObjectActivator = null;
			}
		}
	}
}
