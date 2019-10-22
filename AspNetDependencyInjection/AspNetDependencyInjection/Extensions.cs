using System;
using System.Collections.Generic;

namespace AspNetDependencyInjection
{
	/// <summary>Provides extension methods for <see cref="Type"/>.</summary>
	public static class TypeExtensions
	{
		/// <summary>Returns a new <see cref="Type"/> that refers to a concrete generic <see cref="IEnumerable{T}"/> where <c>T</c> is <paramref name="serviceType"/>.</summary>
		public static Type ToIEnumerableOf( this Type serviceType )
		{
			if( serviceType == null ) throw new ArgumentNullException(nameof(serviceType));

			//

			// See `Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions`:
			Type closedGenericType = typeof(IEnumerable<>).MakeGenericType( serviceType );
			return closedGenericType;
		}

		/// <summary>Calls <see cref="ToIEnumerableOf(Type)"/> and passes it into <paramref name="getService"/> and casts the result as <see cref="IEnumerable{T}"/> where <c>T</c> is <paramref name="serviceType"/>.</summary>
		public static IEnumerable<Object> ToIEnumerableOf( this Type serviceType, Func<Type,Object> getService )
		{
			if( serviceType == null ) throw new ArgumentNullException(nameof(serviceType));
			if( getService == null ) throw new ArgumentNullException(nameof(getService));

			//

			Type enumerableOf = ToIEnumerableOf( serviceType );
			Object result = getService( enumerableOf );
			return (IEnumerable<Object>)result;
		}
	}
}
