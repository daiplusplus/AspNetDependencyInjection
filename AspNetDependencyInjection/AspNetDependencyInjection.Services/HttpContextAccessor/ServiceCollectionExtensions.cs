using System;
using System.Web;

using Microsoft.Extensions.DependencyInjection;

using AspNetDependencyInjection.Services;
using AspNetDependencyInjection.Internal;

namespace AspNetDependencyInjection
{
	public static partial class ServiceCollectionExtensions
	{
		/// <summary>Creates a scoped service implementation for <see cref="IHttpContextAccessor"/> that uses a hard reference to <see cref="HttpContextBase"/> (i.e. it does not use <see cref="HttpContext.Current"/>).</summary>
		public static IServiceCollection AddDefaultHttpContextAccessor( this IServiceCollection services )
		{
			if( services == null ) throw new ArgumentNullException(nameof(services));

			return services
				.AddScoped<DefaultHttpContextAccessorHelper>()
				.AddScoped<IHttpContextAccessor>( CreateHttpContextAccessor );
		}

		/// <summary>Use this extension method if you wish to use your own logic for getting or creating a HttpContext for services without reimplementing <see cref="IHttpContextAccessor"/> directly.</summary>
		public static IServiceCollection AddCustomHttpContextAccessor( this IServiceCollection services, Func<IServiceProvider,HttpContextBase> httpContextGetter )
		{
			if( services == null ) throw new ArgumentNullException(nameof(services));
			if( httpContextGetter == null ) throw new ArgumentNullException(nameof(httpContextGetter));

			return services
				.AddScoped<IHttpContextAccessor>( sp => CreateCustomHttpContextAccessor( sp, httpContextGetter ) );
		}

		private static IHttpContextAccessor CreateHttpContextAccessor( IServiceProvider sp )
		{
			DefaultHttpContextAccessorHelper helper = sp.GetRequiredService<DefaultHttpContextAccessorHelper>();
			if( helper.HttpContext is null ) throw new InvalidOperationException( "The " + nameof(HttpContextBase) + " has not been set for this Service Scope." );

			return new DefaultHttpContextAccessor( helper.HttpContext );
		}

		private static IHttpContextAccessor CreateCustomHttpContextAccessor( IServiceProvider sp, Func<IServiceProvider,HttpContextBase> httpContextGetter )
		{
			HttpContextBase httpContext = httpContextGetter( sp );
			if( httpContext is null ) throw new InvalidOperationException( "The configured " + nameof(HttpContextBase) + " getter function returned null." );

			return new DefaultHttpContextAccessor( httpContext );
		}
	}
}

namespace AspNetDependencyInjection.Internal
{
	internal class DefaultHttpContextAccessorHelper
	{
		public HttpContextBase HttpContext { get; set; }
	}
}