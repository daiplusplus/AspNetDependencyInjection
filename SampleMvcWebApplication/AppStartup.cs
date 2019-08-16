using System;

using Microsoft.Extensions.DependencyInjection;

using WebActivatorEx;
using AspNetDependencyInjection;

[assembly: PreApplicationStartMethod ( typeof( global::SampleMvcWebApplication.SampleApplicationStart ), methodName: nameof( global::SampleMvcWebApplication.SampleApplicationStart.PreStart  ) )]
//[assembly: PostApplicationStartMethod( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.PostStart ) )] // uncomment this if you have any Post-start logic you want to run.

namespace SampleMvcWebApplication
{
	/// <summary>Startup class for the AspNetDependencyInjection NuGet package.</summary>
	internal static class SampleApplicationStart
	{
		private static ApplicationDependencyInjection _di;

		/// <summary>Invoked when the ASP.NET application starts up, before Global's Application_Start method runs. Dependency-injection should be configured here.</summary>
		internal static void PreStart()
		{
			System.Diagnostics.Debug.WriteLine( nameof(SampleApplicationStart) + "." + nameof(PreStart) + "() called." );

			// If you are using ASP.NET Web Forms without any ASP.NET MVC functionality, use `ApplicationDependencyInjection`:
			//_di = ApplicationDependencyInjection.Configure( ConfigureServices );

			// If you are using ASP.NET MVC, regardless of whether you're using ASP.NET Web Forms, use `MvcApplicationDependencyInjection`:
			_di = MvcApplicationDependencyInjection.ConfigureMvc( ConfigureServices );
		}

		private static void ConfigureServices( IServiceCollection services )
		{
			// TODO: Add any dependencies needed here
			services
				// Useful services built-in to AspNetDependencyInjection:
				.AddDefaultHttpContextAccessor() // Adds `IHttpContextAccessor`
				.AddWebConfiguration() // Adds `IWebConfiguration`
			;
		}

		/// <summary>Invoked at the end of ASP.NET application start-up, after Global's Application_Start method runs. Dependency-injection re-configuration may be called here if you have services that depend on Global being initialized.</summary>
		internal static void PostStart()
		{
			System.Diagnostics.Debug.WriteLine( nameof(SampleApplicationStart) + "." + nameof(PostStart) + "() called." );

			//_di.Reconfigure( ReconfigureServices );
		}

		private static void ReconfigureServices( IServiceCollection services )
		{
			
		}
	}
}