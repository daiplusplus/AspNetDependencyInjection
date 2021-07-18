
using AspNetDependencyInjection;
using AspNetDependencyInjection.Configuration;
using AspNetDependencyInjection.Services;

using Microsoft.Extensions.DependencyInjection;

using SampleWebApplication;

using WebActivatorEx;

[assembly: PreApplicationStartMethod( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.PreStart ) )]
[assembly: PostApplicationStartMethod( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.PostStart ) )]
[assembly: ApplicationShutdownMethod( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.ApplicationShutdown ) )]

namespace SampleWebApplication
{
	/// <summary>Startup class for the AspNetDependencyInjection NuGet package.</summary>
	internal static class SampleApplicationStart
	{
		private static ApplicationDependencyInjection _di; // This is disposed in `ApplicationShutdown()`.

		/// <summary>Invoked when the ASP.NET application starts up, before Global's Application_Start method runs. Dependency-injection should be configured here.</summary>
		internal static void PreStart()
		{
			System.Diagnostics.Debug.WriteLine( nameof(SampleApplicationStart) + "." + nameof(PreStart) + "() called." );

			// If you are using ASP.NET MVC, regardless of whether you're using ASP.NET Web Forms or the ASPX View Engine, use `.AddMvcDependencyResolver()` (you will need to reference the `AspNetDependencyInjection.Mvc` package/assembly first).

			_di = new ApplicationDependencyInjectionBuilder()
				.ConfigureServices( ConfigureServices )
				.Build();
		}

		private static void ConfigureServices( IServiceCollection services )
		{
			// TODO: Add any dependencies needed here
			_ = services
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
		}

		internal static void ApplicationShutdown()
		{
			System.Diagnostics.Debug.WriteLine( nameof(SampleApplicationStart) + "." + nameof(ApplicationShutdown) + "() called." );

			_di.Dispose();
		}
	}
}
