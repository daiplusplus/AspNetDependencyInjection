
using SampleWebApplication;

using Unity;
using Unity.Lifetime;
using Unity.WebForms;

using WebActivatorEx;

[assembly: PostApplicationStartMethod( typeof( SampleApplicationUnityWebFormsStart ), methodName: nameof( SampleApplicationUnityWebFormsStart.PostStart ) )]

namespace SampleWebApplication
{
	/// <summary>Startup class for the Unity.WebForms NuGet package.</summary>
	internal static class SampleApplicationUnityWebFormsStart
	{
		private static WebFormsUnityContainerOwner _containerOwner;

		/// <summary>Initializes the unity container when the application starts up.</summary>
		/// <remarks>Do not edit this method. Perform any modifications in the <see cref="RegisterDependencies" /> method.</remarks>
		internal static void PostStart()
		{
			IUnityContainer rootContainer = new UnityContainer();

			RegisterDependencies( rootContainer );

			_containerOwner = new WebFormsUnityContainerOwner( rootContainer );
			_containerOwner.Install();
		}

		/// <summary>Registers dependencies in the supplied container.</summary>
		/// <param name="container">Instance of the container to populate.</param>
		private static void RegisterDependencies( IUnityContainer container )
		{
			// TODO: Add any dependencies needed here
			container
				// registers Service1 as '1 instance per child container' (new object for each request)
				.RegisterType<Service1, Service1>( new HierarchicalLifetimeManager() )
				// registers Service2 as 'new instance per resolution' (each call to resolve = new object)
				.RegisterType<Service2, Service2>()
				.RegisterRequest<IExampleRequestLifelongService,ExampleRequestLifelongService>()
				.RegisterRequest<Service4>();
		}
	}
}