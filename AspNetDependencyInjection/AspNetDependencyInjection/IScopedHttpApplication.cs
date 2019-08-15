#define ILEMIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection
{
	/// <summary>In order to use <see cref="HttpApplication"/>-lifetime <see cref="IServiceScope"/> (see <see cref="ApplicationDependencyInjectionConfiguration.UseHttpApplicationScopes"/>) this interface must be implemented by your application's <c>Global.asax</c> <see cref="HttpApplication"/> class.</summary>
	/// <remarks>Thi is because <see cref="HttpApplication"/> does not expose any way for external code to store per-instance state, namely the <see cref="IServiceScope"/>. Note that the <see cref="HttpApplication.Application"/> state collection is actually shared by all <see cref="HttpApplication"/> instances and cannot be used to identify specific <see cref="HttpApplication"/> instances.</remarks>
	public interface IScopedHttpApplication
	{
		// We can't simply abuse `HttpApplication.Site` as a hack-way to get per-instance storage.
		// The main reason is because HttpApplication's Dispose method actually clears the .Site property FIRST before it invokes the Disposing event handler - making it impossible to dispose of any disposable objects (namely, IServiceScope).
		// This is because a Site is supposed to own and outlive its children (i.e. HttpApplication) and not the other way around (as we want the HttpApplication to own its Site (IServiceScope) and to *slightly* outlive it).

		// And because there's no other instance-state we can hook using reflection or ...wait...
		// We can abuse HttpApplication._handlerFactories which is a simple `Hashtable` - provided we use a string key value that will never be used by ASP.NET, we should be good!

		IServiceScope HttpApplicationServiceScope { get; set; }
	}
}

namespace AspNetDependencyInjection.Internal
{
	using System.Collections;
	using System.Reflection;

	/// <summary>Extension methods for <see cref="HttpApplication"/> that determine if an <see cref="HttpApplication"/> instance is "special" or normal.</summary>
	public static class HttpApplicationExtensions
	{
		private static Boolean IsSpecialImpl( Boolean isInitNormal, Boolean isInitSpecial )
		{
			// Check the value of both fields to ensure the HttpApplication is in a valid state.

			if( isInitNormal )
			{
				if( isInitSpecial )
				{
					throw new InvalidOperationException( "The HttpApplication instance appears to have been initialized as both Normal and Special. This should never happen." );
				}
				else // isInitNormal && !isInitSpecial
				{
					return false;
				}
			}
			else // !isInitNormal
			{
				if( isInitSpecial )
				{
					return true;
				}
				else // !isInitNormal && !isInitSpecial
				{
					throw new InvalidOperationException( "The HttpApplication instance has not yet been initialized." );
				}
			}
		}

#if SLOWER_REFLECTION

		private static readonly FieldInfo _httpApplication_initInternalCompleted = typeof(HttpApplication).GetField( "_initInternalCompleted", BindingFlags.Instance | BindingFlags.NonPublic ) ?? throw new InvalidOperationException( "Could not get '_initInternalCompleted' field from HttpApplication." );
		private static readonly FieldInfo _httpApplication_initSpecialCompleted  = typeof(HttpApplication).GetField( "_initSpecialCompleted" , BindingFlags.Instance | BindingFlags.NonPublic ) ?? throw new InvalidOperationException( "Could not get '_initSpecialCompleted' field from HttpApplication." );
		
		private static readonly FieldInfo _httpApplication_handlerFactories  = typeof(HttpApplication).GetField( "_handlerFactories" , BindingFlags.Instance | BindingFlags.NonPublic ) ?? throw new InvalidOperationException( "Could not get '_handlerFactories' field from HttpApplication." );

		// If you *really* want to make this fast, use IL-emit:
		// https://stackoverflow.com/questions/16073091/is-there-a-way-to-create-a-delegate-to-get-and-set-values-for-a-fieldinfo
		// But the cost of FieldInfo vs. IL-emit delegates for getting fields probably isn't worth it as this extension method is not used in a tight-loop: https://mattwarren.org/2016/12/14/Why-is-Reflection-slow/

		/// <summary>Indicates if the specified <paramref name="httpApplication"/> is a "special" instance or a normal instance. Throws <see cref="InvalidOperationException"/> if the <paramref name="httpApplication"/> is not yet initialized.</summary>
		/// <exception cref="InvalidOperationException">Thrown when <paramref name="httpApplication"/> is not yet initialized (or if it somehow has been initialized twice as both Special and Normal).</exception>
		public static Boolean IsSpecial( this HttpApplication httpApplication )
		{
			if( httpApplication == null ) throw new ArgumentNullException(nameof(httpApplication));

			Boolean isInitNormal  = (Boolean)_httpApplication_initInternalCompleted.GetValue( httpApplication );
			Boolean isInitSpecial = (Boolean)_httpApplication_initSpecialCompleted .GetValue( httpApplication );

			return IsSpecialImpl( isInitNormal, isInitSpecial ); 
		}

		public static Hashtable GetHandlerFactories( this HttpApplication httpApplication )
		{
			if( httpApplication == null ) throw new ArgumentNullException(nameof(httpApplication));

			Hashtable dict = (Hashtable)_httpApplication_handlerFactories.GetValue( httpApplication );
			return dict;
		}

#elif ILEMIT

		private static readonly Func<HttpApplication,Boolean> _httpApplication_initInternalCompleted = Internal.ReflectionUtility.CreateFieldGetter<HttpApplication,Boolean>( fieldName: "_initInternalCompleted" );
		private static readonly Func<HttpApplication,Boolean> _httpApplication_initSpecialCompleted  = Internal.ReflectionUtility.CreateFieldGetter<HttpApplication,Boolean>( fieldName: "_initSpecialCompleted"  );

		private static readonly Func<HttpApplication,Hashtable> _httpApplication_handlerFactories  = Internal.ReflectionUtility.CreateFieldGetter<HttpApplication,Hashtable>( fieldName: "_handlerFactories;" );

		/// <summary>Indicates if the specified <paramref name="httpApplication"/> is a "special" instance or a normal instance. Throws <see cref="InvalidOperationException"/> if the <paramref name="httpApplication"/> is not yet initialized.</summary>
		/// <exception cref="InvalidOperationException">Thrown when <paramref name="httpApplication"/> is not yet initialized (or if it somehow has been initialized twice as both Special and Normal).</exception>
		public static Boolean IsSpecial( this HttpApplication httpApplication )
		{
			if( httpApplication == null ) throw new ArgumentNullException(nameof(httpApplication));

			Boolean isInitNormal  = _httpApplication_initInternalCompleted( httpApplication );
			Boolean isInitSpecial = _httpApplication_initSpecialCompleted( httpApplication );

			return IsSpecialImpl( isInitNormal, isInitSpecial ); 
		}

		/// <summary>Exposes the <see cref="Hashtable"/> field inside <see cref="HttpApplication"/> that AspNetDependencyInjection abuses to store the HttpApplication-scoped service scope/lifetime.</summary>
		public static Hashtable GetHandlerFactories( this HttpApplication httpApplication )
		{
			if( httpApplication == null ) throw new ArgumentNullException(nameof(httpApplication));

			Hashtable dict = _httpApplication_handlerFactories( httpApplication );
			return dict;
		}

#else
		public static Boolean IsSpecial( this HttpApplication httpApplication )
		{
			throw new NotSupportedException( "This AspNetDependencyInjection assembly was compiled without support for exfiltrating state from " + nameof(HttpApplication) + " instances." );
		}

		public static Boolean IsSpecial( this HttpApplication httpApplication )
		{
			throw new NotSupportedException( "This AspNetDependencyInjection assembly was compiled without support for exfiltrating state from " + nameof(HttpApplication) + " instances." );
		}

#endif

		
	}
}

namespace AspNetDependencyInjection.Internal
{
	using System.Reflection;
	using System.Reflection.Emit;

	/// <summary>From https://stackoverflow.com/questions/16073091/is-there-a-way-to-create-a-delegate-to-get-and-set-values-for-a-fieldinfo</summary>
	public static class ReflectionUtility
	{
		/// <summary>Uses IL-emit to create a delegate to retrieve a field's value. The field can be an instance or static field.</summary>
		public static Func<TObject,TField> CreateFieldGetter<TObject,TField>( String fieldName )
		{
			if( String.IsNullOrWhiteSpace( fieldName ) ) throw new ArgumentNullException(nameof(fieldName));

			FieldInfo fieldInfo = typeof(TObject).GetField( fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );
			
			if( fieldInfo == null ) throw new ArgumentException( "Could not find a public or non-public, instance or static, field named \"" + fieldName + "\"." );

			return CreateFieldGetter<TObject,TField>( fieldInfo );
		}

		/// <summary>Uses IL-emit to create a delegate to retrieve a field's value. The field can be an instance or static field.</summary>
		/// <param name="fieldInfo">Can be static or instance field.</param>
		public static Func<TObject,TField> CreateFieldGetter<TObject,TField>( FieldInfo fieldInfo )
		{
			if( fieldInfo == null ) throw new ArgumentNullException(nameof(fieldInfo));

			String methodName = fieldInfo.ReflectedType.FullName + ".get_" + fieldInfo.Name; // TODO: Does the method name matter?

			DynamicMethod method = new DynamicMethod( methodName, returnType: typeof(TField), parameterTypes: new[] { typeof(TObject) }, restrictedSkipVisibility: true );
			ILGenerator gen = method.GetILGenerator();
			if( fieldInfo.IsStatic )
			{
				gen.Emit( OpCodes.Ldsfld, fieldInfo );
			}
			else
			{
				gen.Emit( OpCodes.Ldarg_0 );
				gen.Emit( OpCodes.Ldfld, fieldInfo );
			}
			gen.Emit( OpCodes.Ret );

			Func<TObject,TField> newDelegate = (Func<TObject,TField>)method.CreateDelegate( typeof(Func<TObject,TField>) );
			return newDelegate;
		}
	}
}