using System;
using System.Web;

using Unity;
using Unity.WebForms;

[assembly: WebActivatorEx.PreApplicationStartMethod( typeof($rootnamespace$.ApplicationStart), nameof($rootnamespace$.ApplicationStart.PostStart) )]

namespace $rootnamespace$
{
	/// <summary>Startup class for your application. Configures dependency-injection.</summary>
	internal static class ApplicationStart
	{
		private static Unity.WebForms.ApplicationDependencyInjection _di;

		/// <summary>Invoked when the ASP.NET application starts up, before Global's Application_Start method runs. Dependency-injection should be configured here.</summary>
		internal static void PreStart()
		{
			System.Diagnostics.Debug.WriteLine( nameof(SampleApplicationStart) + "." + nameof(PreStart) + "() called." );

			_di = Unity.WebForms.ApplicationDependencyInjection.Configure( ConfigureServices );
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

		/// <summary>Invoked at the end of ASP.NET application start-up, after Global's Application_Start method runs. Dependency-injection re-configuration may be called here if you have services that depend on Global being initialized.</summary>
		internal static void PostStart()
		{
			System.Diagnostics.Debug.WriteLine( nameof(SampleApplicationStart) + "." + nameof(PostStart) + "() called." );

			_di.Reconfigure( ReconfigureServices );
		}

		private static void ReconfigureServices( IServiceCollection services )
		{
			
		}
	}
}