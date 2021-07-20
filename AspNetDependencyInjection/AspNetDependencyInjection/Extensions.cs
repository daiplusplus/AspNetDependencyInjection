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
	}
}
