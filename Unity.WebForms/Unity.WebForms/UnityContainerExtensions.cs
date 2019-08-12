using System;
using Unity.Injection;
using Unity.Lifetime;

namespace Unity.WebForms
{
	/// <summary>Extension methods for <see cref="IUnityContainer"/> to register types that will have their lifetimes limited to an ASP.NET HTTP request lifetime. Similar to &quot;Scoped Services&quot; in ASP.NET Core's dependency-injection terminology.</summary>
	public static class UnityContainerExtensions
	{
		#region RegisterRequest

		// Type t

		/// <summary>Registers a type <paramref name="t"/> with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequest( this IUnityContainer container, Type t, params InjectionMember[] injectionMembers )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterType( registeredType: null, mappedToType: t, name: null, new HierarchicalLifetimeManager(), injectionMembers );
		}

		/// <summary>Registers a named type <paramref name="t"/> with a lifetime limited to an ASP.NET request's lifetime.</summary>
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
		
		/// <summary>Registers a named type <paramref name="from"/> (implemented by <paramref name="to"/>) with a lifetime limited to an ASP.NET request's lifetime.</summary>
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
		
		/// <summary>Registers a named type <typeparamref name="T"/> with a lifetime limited to an ASP.NET request's lifetime.</summary>
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
		
		/// <summary>Registers a named type <typeparamref name="TFrom"/> (implemented by <typeparamref name="TTo"/>) with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequest<TFrom, TTo>( this IUnityContainer container, String name, params InjectionMember[] injectionMembers ) where TTo : TFrom
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterType( registeredType: typeof(TFrom), mappedToType: typeof(TTo), name: name, new HierarchicalLifetimeManager(), injectionMembers );
		}

		#endregion

		#region RegisterRequestFactory

		// Type t:

		// NOTE: There are two factory Func types:
		// 1. Func<IUnityContainer, Type, String, Object> factory   -
		// 2. Func<IUnityContainer, Object>               sfactory  - Named `sfactory` because it's "simple-factory" and allows for explicit method overload selection using named arguments.

		/// <summary>Registers a named type <paramref name="type"/> that is instantiated by a factory method, with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequestFactory( this IUnityContainer container, Type type, String name, Func<IUnityContainer, Type, String, Object> factory )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterFactory( type: type, name: name, factory, new HierarchicalLifetimeManager() );
		}

		/// <summary>Registers a named type <paramref name="type"/> that is instantiated by a simpler factory method <paramref name="sfactory"/>, with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequestFactory( this IUnityContainer container, Type type, String name, Func<IUnityContainer, Object> sfactory )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterFactory( type: type, name: name, sfactory, new HierarchicalLifetimeManager() );
		}

		/// <summary>Registers a type <paramref name="type"/> that is instantiated by a factory method, with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequestFactory( this IUnityContainer container, Type type, Func<IUnityContainer, Type, String, Object> factory )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterFactory( type, factory, new HierarchicalLifetimeManager() );
		}

		/// <summary>Registers a type <paramref name="type"/> that is instantiated by a simpler factory method <paramref name="sfactory"/>, with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequestFactory( this IUnityContainer container, Type type, Func<IUnityContainer, Object> sfactory )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterFactory( type, sfactory, new HierarchicalLifetimeManager() );
		}

		// Generic:

		/// <summary>Registers a type <typeparamref name="T"/> that is instantiated by a factory method <paramref name="factory"/>, with a lifetime limited to an ASP.NET request's lifetime.</summary>
		/// <param name="container">The container to add the registration to.</param>
		/// <param name="factory">Factory method. Parameters are: <c><see cref="IUnityContainer"/> container</c> (to resolve additional dependencies), <c><see cref="Type"/> type</c> (the specific type being requested), <c><see cref="String"/> name</c> (the registration's name, if any).</param>
		public static IUnityContainer RegisterRequestFactory<T>( this IUnityContainer container, Func<IUnityContainer, Type, String, T> factory )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterFactory<T>( factory: ( c, t, n ) => factory( c, t, n ), new HierarchicalLifetimeManager() );
		}

		/// <summary>Registers a type <typeparamref name="T"/> that is instantiated by a simpler factory method <paramref name="sfactory"/>, with a lifetime limited to an ASP.NET request's lifetime.</summary>
		/// <param name="container">The container to add the registration to.</param>
		/// <param name="sfactory">Factory method. The single <c><see cref="IUnityContainer"/> container</c> parameter can be used to resolve additional dependencies.</param>
		public static IUnityContainer RegisterRequestFactory<T>( this IUnityContainer container, Func<IUnityContainer, T> sfactory )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterFactory<T>( factory: c => sfactory( c ), new HierarchicalLifetimeManager() );
		}

		/// <summary>Registers a named type <typeparamref name="T"/> that is instantiated by a factory method <paramref name="factory"/>, with a lifetime limited to an ASP.NET request's lifetime.</summary>
		/// <param name="container">The container to add the registration to.</param>
		/// <param name="name">The registration's name.</param>
		/// <param name="factory">Factory method. Parameters are: <c><see cref="IUnityContainer"/> container</c> (to resolve additional dependencies), <c><see cref="Type"/> type</c> (the specific type being requested), <c><see cref="String"/> name</c> (the registration's name, if any).</param>
		public static IUnityContainer RegisterRequestFactory<T>( this IUnityContainer container, String name, Func<IUnityContainer, Type, String, T> factory )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterFactory<T>( name, factory: ( c, t, n ) => factory( c, t, n ), new HierarchicalLifetimeManager() );
		}

		/// <summary>Registers a named type <typeparamref name="T"/> that is instantiated by a simpler factory method <paramref name="sfactory"/>, with a lifetime limited to an ASP.NET request's lifetime.</summary>
		/// <param name="container">The container to add the registration to.</param>
		/// <param name="name">The registration's name.</param>
		/// <param name="sfactory">Factory method. The single <c><see cref="IUnityContainer"/> container</c> parameter can be used to resolve additional dependencies.</param>
		public static IUnityContainer RegisterRequestFactory<T>( this IUnityContainer container, String name, Func<IUnityContainer, T> sfactory )
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

			return container.RegisterFactory<T>( name, factory: sp => sfactory( sp ), new HierarchicalLifetimeManager() );
		}

		// Generic factory object:

		/// <summary>Registers <typeparamref name="TFactory"/> as a singleton service, then registers a type <typeparamref name="T"/> that is instantiated by a <typeparamref name="TFactory"/> instance, with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequestFactory<T,TFactory>( this IUnityContainer container )
			where TFactory : IServiceFactory<T>
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

//			return container
//				.RegisterSingleton<TFactory>()
//				.RegisterFactory<T>( ( c, type, name ) => {
//
//					TFactory factoryInstance = c.Resolve<TFactory>( name: name );
//					return factoryInstance.CreateInstance();
//
//				}, new HierarchicalLifetimeManager() );

			// Simplified:


			return container
				.RegisterSingleton<TFactory>()
				.RegisterFactory<T>( ( IUnityContainer c, Type type, String name ) => c.Resolve<TFactory>().CreateInstance(), new HierarchicalLifetimeManager() );

		}

		/// <summary>Registers <typeparamref name="TNamedFactory"/> as a singleton service, then registers a type <typeparamref name="T"/> that is instantiated by a <typeparamref name="TNamedFactory"/> instance, with a lifetime limited to an ASP.NET request's lifetime.</summary>
		public static IUnityContainer RegisterRequestFactory<T,TNamedFactory>( this IUnityContainer container, String name )
			where TNamedFactory : INamedServiceFactory<T>
		{
			if( container == null ) throw new ArgumentNullException(nameof(container));

//			return container
//				.RegisterSingleton<TFactory>()
//				.RegisterFactory<T>( ( c, type, name ) => {
//
//					TFactory factoryInstance = c.Resolve<TFactory>( name: name );
//					return factoryInstance.CreateInstance();
//
//				}, new HierarchicalLifetimeManager() );

			// Simplified:


			return container
				.RegisterSingleton<TNamedFactory>()
				.RegisterFactory<T>( name, ( IUnityContainer c, Type type, String n ) => c.Resolve<TNamedFactory>().CreateInstance( n ), new HierarchicalLifetimeManager() );

		}

		#endregion
	}
}
