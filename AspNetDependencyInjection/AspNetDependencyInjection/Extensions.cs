using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace AspNetDependencyInjection
{
	/// <summary>Provides extension methods for <see cref="Type"/>.</summary>
	public static class AndiExtensions
	{
		/// <summary>Returns a new <see cref="Type"/> that refers to a concrete generic <see cref="IEnumerable{T}"/> where <c>T</c> is <paramref name="serviceType"/>.</summary>
		public static Type ToIEnumerableOf( this Type serviceType )
		{
			if( serviceType is null ) throw new ArgumentNullException(nameof(serviceType));

			//
			
			// See `Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions`:
			Type closedGenericType = typeof(IEnumerable<>).MakeGenericType( serviceType );
			return closedGenericType;
		}

		/// <summary>Calls <see cref="ToIEnumerableOf(Type)"/> and passes it into <paramref name="getService"/> and casts the result as <see cref="IEnumerable{T}"/> where <c>T</c> is <paramref name="serviceType"/>.</summary>
		public static IEnumerable<Object> ToIEnumerableOf( this Type serviceType, Func<Type,Object> getService )
		{
			if( serviceType is null ) throw new ArgumentNullException(nameof(serviceType));
			if( getService  is null ) throw new ArgumentNullException(nameof(getService));

			//

			Type enumerableOf = ToIEnumerableOf( serviceType );
			Object result = getService( enumerableOf );
			return (IEnumerable<Object>)result;
		}

		/// <summary>
		/// Converts <paramref name="nvc"/> into a <see cref="Dictionary{TKey, TValue}"/> (of String Keys and String values).<br />
		/// If <see cref="NameValueCollection"/> returns multiple values for any name/key, then only the first value returned will be added to the returned dictionary.<br />
		/// The returned dictionary will use <see cref="StringComparer.OrdinalIgnoreCase"/> to compare keys.
		/// </summary>
		public static Dictionary<String,String> ToDictionary( NameValueCollection nvc )
		{
			if( nvc is null ) throw new ArgumentNullException( nameof( nvc ) );

			Dictionary<String,String> dict = new Dictionary<String,String>( capacity: nvc.Count, comparer: StringComparer.OrdinalIgnoreCase );

			foreach( String key in nvc.Keys )
			{
				String firstValue = nvc.GetValues( key )[0];
				dict[ key ] = firstValue;
			}

			return dict;
		}
		/// <summary>Gets a &quot;classic&quot; <see cref="System.Web.HttpContext"/> from a <see cref="System.Web.HttpContextBase"/> via <c>httpContextBase.ApplicationInstance.Context</c></summary>
		public static System.Web.HttpContext GetHttpContext( this System.Web.HttpContextBase httpContextBase )
		{
			if( httpContextBase is null ) throw new ArgumentNullException( nameof( httpContextBase ) );

			// These conditions should never happen, but just-in-case:
			if( httpContextBase.ApplicationInstance         is null ) throw new ArgumentException( message: "httpContextBase.ApplicationInstance is null."        , paramName: nameof( httpContextBase ) );
			if( httpContextBase.ApplicationInstance.Context is null ) throw new ArgumentException( message: "httpContextBase.ApplicationInstance.Context is null.", paramName: nameof( httpContextBase ) );

			return httpContextBase.ApplicationInstance.Context;
	}

		/// <summary>Gets a <see cref="System.Web.HttpContextBase"/> from a &quot;classic&quot; <see cref="System.Web.HttpContext"/> via <c>httpContext.Request.RequestContext.HttpContext</c></summary>
		public static System.Web.HttpContextBase GetHttpContextBase( this System.Web.HttpContext httpContext )
		{
			if( httpContext is null ) throw new ArgumentNullException( nameof( httpContext ) );

			// These conditions should never happen, but just-in-case:
			if( httpContext.Request                            is null ) throw new ArgumentException( message: "httpContext.Request is null."                           , paramName: nameof( httpContext ) );
			if( httpContext.Request.RequestContext             is null ) throw new ArgumentException( message: "httpContext.Request.RequestContext is null."            , paramName: nameof( httpContext ) );
			if( httpContext.Request.RequestContext.HttpContext is null ) throw new ArgumentException( message: "httpContext.Request.RequestContext.HttpContext is null.", paramName: nameof( httpContext ) );

			return httpContext.Request.RequestContext.HttpContext;
		}
}
}
