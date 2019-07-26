using System;
using System.Web;

using Unity;
using Unity.WebForms;

[assembly: WebActivatorEx.PostApplicationStartMethod( typeof($rootnamespace$.UnityWebFormsStart), nameof($rootnamespace$.UnityWebFormsStart.PostStart) )]

namespace $rootnamespace$
{
	/// <summary>Unity.WebForms startup and configuration class for your ASP.NET WebForms project.</summary>
	internal static class UnityWebFormsStart
	{
		private static WebFormsUnityContainerOwner _containerOwner;

		/// <summary>Initializes the unity container when the application starts up.</summary>
		/// <remarks>Do not edit this method. Perform any modifications in the <see cref="RegisterDependencies" /> method.</remarks>
		internal static void PostStart()
		{
			IUnityContainer rootContainer = new UnityContainer();

			RegisterDependencies( rootContainer );

			_containerOwner = new WebFormsUnityContainerOwner( rootContainer );
		}

		/// <summary>Registers dependencies in the supplied container.</summary>
		/// <param name="container">Instance of the container to populate.</param>
		private static void RegisterDependencies( IUnityContainer container )
		{
			// TODO: Add any dependencies needed here
			container
				// Registers a service such that Unity.WebForms will only ever create a single instance for the life of the container (i.e. the instance is shared by all HttpApplication and HttpContext instances)
				.RegisterSingleton<ISingletonService,SingletonImplementation>()

				// Registers a service such that Unity.WebForms will create (and dispose, if necessary) a new instance for each HTTP request where that service is requested.
				// If requested outside of a HTTP request's context then the service will have the same lifetime as the root container.
				// When multiple dependents depend on one of these dependencies then they will share the same instance.
				// Examples include Entity Framework DbContexts and OAuth2/OIDC-based HttpClient factories that use request cookies to store tokens.
				.RegisterRequest<YourDbContext>()

				// Registers a service such that Unity.WebForms will create a new instance for each call to Resolve (i.e. a transient dependency).
				// Examples include services that depend on request-lifetime-limited ILogger instances.
				.RegisterType<ITransientService,TransientServiceImplementation>();
		}
	}
}