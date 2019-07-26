using System;
using Unity.Injection;
using Unity.Lifetime;

namespace Unity.WebForms
{
	/// <summary>Extension methods for <see cref="IUnityContainer"/> to register types that will have their lifetimes limited to an ASP.NET HTTP request lifetime. Similar to &quot;Scoped Services&quot; in ASP.NET Core's dependency-injection terminology.</summary>
	public static class UnityContainerExtensions
	{
		// Type t

		/// <summary>Registers a type <paramref name="t"/> with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequest( this IUnityContainer container, Type t, params InjectionMember[] injectionMembers )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterType( registeredType: null, mappedToType: t, name: null, new HierarchicalLifetimeManager(), injectionMembers );
		}

		/// <summary>Registers a type <paramref name="t"/> with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequest( this IUnityContainer container, Type t, String name, params InjectionMember[] injectionMembers )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterType( registeredType: null, mappedToType: t, name: name, new HierarchicalLifetimeManager(), injectionMembers );
		}

		// Type from, Type to

		/// <summary>Registers a type <paramref name="from"/> (implemented by <paramref name="to"/>) with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequest( this IUnityContainer container, Type from, Type to, params InjectionMember[] injectionMembers )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterType( registeredType: from, mappedToType: to, name: null, new HierarchicalLifetimeManager(), injectionMembers );
		}
		
		/// <summary>Registers a type <paramref name="from"/> (implemented by <paramref name="to"/>) with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequest( this IUnityContainer container, Type from, Type to, String name, params InjectionMember[] injectionMembers )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterType( registeredType: from, mappedToType: to, name: name, new HierarchicalLifetimeManager(), injectionMembers );
		}
		
		// <T>

		/// <summary>Registers a type <typeparamref name="T"/> with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequest<T>( this IUnityContainer container, params InjectionMember[] injectionMembers )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterType( registeredType: null, mappedToType: typeof(T), name: null, new HierarchicalLifetimeManager(), injectionMembers );
		}
		
		/// <summary>Registers a type <typeparamref name="T"/> with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequest<T>( this IUnityContainer container, String name, params InjectionMember[] injectionMembers )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterType( registeredType: null, mappedToType: typeof(T), name: name, new HierarchicalLifetimeManager(), injectionMembers );
		}
		
		// <TFrom,TTo>

		/// <summary>Registers a type <typeparamref name="TFrom"/> (implemented by <typeparamref name="TTo"/>) with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequest<TFrom, TTo>( this IUnityContainer container, params InjectionMember[] injectionMembers ) where TTo : TFrom
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterType( registeredType: typeof(TFrom), mappedToType: typeof(TTo), name: null, new HierarchicalLifetimeManager(), injectionMembers );
		}
		
		/// <summary>Registers a type <typeparamref name="TFrom"/> (implemented by <typeparamref name="TTo"/>) with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequest<TFrom, TTo>( this IUnityContainer container, String name, params InjectionMember[] injectionMembers ) where TTo : TFrom
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterType( registeredType: typeof(TFrom), mappedToType: typeof(TTo), name: name, new HierarchicalLifetimeManager(), injectionMembers );
		}
	}
}
