using System;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Unity.WebForms.Services
{
	/// <summary>Gets the <see cref="HttpContextBase"/> for the current request in a thread-safe manner (by using an immutable instance field reference to the <see cref="HttpContextBase"/> available when the request was started).</summary>
	public class DefaultHttpContextAccessor : IHttpContextAccessor
	{
		/// <summary>Constructs a new instance of <see cref="DefaultHttpContextAccessor"/> using the provided <paramref name="httpContextBase"/>.</summary>
		/// <param name="httpContextBase">Required. Cannot be null (otherwise an <see cref="ArgumentNullException"/> will be thrown).</param>
		public DefaultHttpContextAccessor( HttpContextBase httpContextBase )
		{
			this.HttpContext = httpContextBase ?? throw new ArgumentNullException(nameof(httpContextBase));
		}

		/// <summary>Always returns the same <see cref="HttpContextBase"/> instance that was passed into this instance's constructor (i.e. it does not use <see cref="HttpContext.Current"/>).</summary>
		public HttpContextBase HttpContext { get; }
	}

	// TODO: When would *anyone* ever want to use `ThreadLocalStorageHttpContextAccessor`?
	// If those types need some form of compatibility with the thread-unsafe `HttpContext.Current` then they can use it directly.

	#if ANY_GOOD_REASONS_FOR_THIS

	/// <summary>Implements <see cref="IHttpContextAccessor"/> by simply returning <see cref="HttpContext.Current"/> wrapped in a new <see cref="HttpContextWrapper"/>.</summary>
	public class ThreadLocalStorageHttpContextAccessor : IHttpContextAccessor
	{
		/// <summary>Always returns <c>new HttpContextWrapper( System.Web.HttpContext.Current )</c>.</summary>
		public HttpContextBase HttpContext
		{
			get
			{
				return new HttpContextWrapper( System.Web.HttpContext.Current );
			}
		}
	}

	#endif
	
	public static partial class WebFormsServiceExtensions
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

	internal class DefaultHttpContextAccessorHelper
	{
		public HttpContextBase HttpContext { get; set; }
	}
}
