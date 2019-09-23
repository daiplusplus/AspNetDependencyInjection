using System;

using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using AspNetDependencyInjection.Internal;

namespace AspNetDependencyInjection
{
	/// <summary>Mutable object. Provides a fluent interface. Used to build an immutable <see cref="ApplicationDependencyInjection"/> object.</summary>
	public class ApplicationDependencyInjectionBuilder
	{
		private ApplicationDependencyInjectionConfiguration configuration;

		private readonly List<Action<IServiceCollection>> configureServices = new List<Action<IServiceCollection>>();
		
		private readonly List<Func<ApplicationDependencyInjection,IDependencyInjectionClient>> clientFactories = new List<Func<ApplicationDependencyInjection,IDependencyInjectionClient>>();

		/// <summary>Creates a new <see cref="ApplicationDependencyInjectionBuilder"/> and adds a factory for <see cref="DependencyInjectionWebObjectActivator"/> to the internal client factories list.</summary>
		public ApplicationDependencyInjectionBuilder()
		{
			this.clientFactories.Add( di => new DependencyInjectionWebObjectActivator( di ) );
		}

		#region Fluent builders

		/// <summary>Adds <paramref name="configureServices"/> to an internal callback list to be called when <see cref="Build"/> is called.</summary>
		public virtual ApplicationDependencyInjectionBuilder ConfigureServices( Action<IServiceCollection> configureServices )
		{
			if( configureServices == null ) throw new ArgumentNullException(nameof(configureServices));
			
			//

			this.configureServices.Add( configureServices );
			return this;
		}

		/// <summary>Adds the specified <see cref="IDependencyInjectionClient"/> factories to the internal collection. <paramref name="clientFactories"/> MAY be null or empty.</summary>
		public virtual ApplicationDependencyInjectionBuilder AddClient( params Func<ApplicationDependencyInjection,IDependencyInjectionClient>[] clientFactories )
		{
			if( clientFactories != null )
			{
				this.clientFactories.AddRange( clientFactories.Where( cf => cf != null ) );
			}
			
			return this;
		}

		/// <summary>Sets the internal <see cref="ApplicationDependencyInjectionConfiguration"/> field. Can be null (in which case the defualt <see cref="ApplicationDependencyInjectionConfiguration"/> will be used).</summary>
		public virtual ApplicationDependencyInjectionBuilder UseConfiguration( ApplicationDependencyInjectionConfiguration configuration )
		{
			this.configuration = configuration;
			return this;
		}

		#endregion

		#region Build

		// There exists a mutual-reference dependenecy problem here:
		// * `IDependencyInjectionClient` depends on a valid `ApplicationDependencyInjection`
		// * `ApplicationDependencyInjection` needs to own the `IDependencyInjectionClient` instances.
		// * But `ApplicationDependencyInjection` can't simply create the `IDependencyInjectionClient` instances from within its constructor because that would have to be a virtual method (constructors should not call virtual methods).

		// However, there's two solutions:
		// * Define a new *outer* class that creates the `ApplicationDependencyInjection` via a virtual method (which can create a subclass)
		// * And this outer-class then owns the `IDependencyInjectionClient` instances.
		// Ugh, I wish C# had a way to define immutable aggregates easily.

		// The other solution is that `ApplicationDependencyInjection` still owns the `IDependencyInjectionClient` instances, but the outer-object that creates them is this (ApplicationDependencyInjectionBuilder).
		// This is acceptable because `ApplicationDependencyInjection` never exposes an intermediate-state `ApplicationDependencyInjection` instance to consumers - it all happens inside `Build`.

		/// <summary>Calls <see cref="CreateAndConfigureServiceCollection"/>, then <see cref="Create(ApplicationDependencyInjectionConfiguration,IServiceCollection)"/>, then <see cref="ApplicationDependencyInjection.CreateClients(IEnumerable{Func{ApplicationDependencyInjection,IDependencyInjectionClient}})"/></summary>
		public virtual ApplicationDependencyInjection Build()
		{
			ApplicationDependencyInjectionConfiguration configuration = this.configuration ?? new ApplicationDependencyInjectionConfiguration();

			IServiceCollection services = this.CreateAndConfigureServiceCollection();

			// If a consumer subclasses `ApplicationDependencyInjectionConfiguration` then their constructor has to complete first before creating Clients.
			// This is so that we don't pass an incomplete `ApplicationDependencyInjectionConfiguration` instance into a Client's constructor.
			// After they're created, they're then saved in `built`.

			ApplicationDependencyInjection built = this.Create( configuration, services );
			try
			{
				built.CreateClients( this.clientFactories );
				return built;
			}
			catch
			{
				built.Dispose();
				throw;
			}
		}

		/// <summary>Factory for the <see cref="ApplicationDependencyInjection"/> instance. Overriden versions may return a subclass of <see cref="ApplicationDependencyInjection"/>. Implementations should not call any other virtual methods on this object.</summary>
		protected virtual ApplicationDependencyInjection Create( ApplicationDependencyInjectionConfiguration configuration, IServiceCollection services )
		{
			return new ApplicationDependencyInjection( configuration, services );
		}

		/// <summary>Calls <see cref="CreateServiceCollection"/>, then <see cref="ConfigureAllServices(IServiceCollection)"/> and returns the collection.</summary>
		protected virtual IServiceCollection CreateAndConfigureServiceCollection()
		{
			IServiceCollection services = this.CreateServiceCollection();
			this.ConfigureAllServices( services );
			return services;
		}

		/// <summary>Factory for the <see cref="ServiceCollection"/>.</summary>
		protected virtual IServiceCollection CreateServiceCollection()
		{
			return new ServiceCollection();
		}

		/// <summary>Invokes all registered <see cref="ConfigureServices"/> calls.</summary>
		protected virtual void ConfigureAllServices( IServiceCollection services )
		{
			foreach( Action<IServiceCollection> action in this.configureServices )
			{
				action( services );
			}
		}

		#endregion

		#region Ergonomics

		/// <summary>Does nothing other than adding <see cref="EditorBrowsableAttribute"/>.</summary>
		[EditorBrowsable( EditorBrowsableState.Never ) ]
		[Browsable( browsable: false )]
		public override Boolean Equals( Object obj )
		{
			return base.Equals( obj );
		}

		/// <summary>Does nothing other than adding <see cref="EditorBrowsableAttribute"/>.</summary>
		[EditorBrowsable( EditorBrowsableState.Never ) ]
		[Browsable( browsable: false )]
		public override Int32 GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>Does nothing other than adding <see cref="EditorBrowsableAttribute"/>.</summary>
		[EditorBrowsable( EditorBrowsableState.Never ) ]
		[Browsable( browsable: false )]
		public override String ToString()
		{
			return base.ToString();
		}

		#endregion
	}
}
