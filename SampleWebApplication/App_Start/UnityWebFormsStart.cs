
using Microsoft.Extensions.DependencyInjection;
using SampleWebApplication;

using WebActivatorEx;

[assembly: PreApplicationStartMethod ( typeof( SampleApplicationUnityWebFormsStart ), methodName: nameof( SampleApplicationUnityWebFormsStart.PreStart  ) )]
[assembly: PostApplicationStartMethod( typeof( SampleApplicationUnityWebFormsStart ), methodName: nameof( SampleApplicationUnityWebFormsStart.PostStart ) )]

namespace SampleWebApplication
{
	using Unity.WebForms.Services;

	/// <summary>Startup class for the Unity.WebForms NuGet package.</summary>
	internal static class SampleApplicationUnityWebFormsStart
	{
		private static Unity.WebForms.WebFormsUnityContainerOwner _containerOwner;

		/// <summary>Initializes the unity container when the application starts up.</summary>
		/// <remarks>Do not edit this method. Perform any modifications in the <see cref="RegisterDependencies" /> method.</remarks>
		internal static void PreStart()
		{
			System.Diagnostics.Debug.WriteLine( nameof(SampleApplicationUnityWebFormsStart) + "." + nameof(PreStart) + "() called." );

			_containerOwner = Unity.WebForms.WebFormsUnityContainerOwner.Configure( ConfigureServices );
		}

		internal static void PostStart()
		{
			System.Diagnostics.Debug.WriteLine( nameof(SampleApplicationUnityWebFormsStart) + "." + nameof(PostStart) + "() called." );
		}

		/// <summary>Registers dependencies in the supplied container.</summary>
		/// <param name="container">Instance of the container to populate.</param>
		private static void ConfigureServices( IServiceCollection services )
		{
			// TODO: Add any dependencies needed here
			services
				.AddDefaultHttpContextAccessor()
				.AddScoped<Service1>()
				.AddTransient<Service2>()
				.AddScoped<IExampleRequestLifelongService,ExampleRequestLifelongService>()
				.AddScoped<Service4>()
				.AddSingleton<SingletonService>();
		}
	}
}