#define USE_SCOPED_SIGNALR_RESOLVER

using System;

using Microsoft.AspNet.SignalR;
using Microsoft.Extensions.DependencyInjection;

using Owin;
using Microsoft.Owin;
using AspNetDependencyInjection;
using WebActivatorEx;

using SampleMvcWebApplication;
using SampleMvcWebApplication.SampleServices;

[assembly: PreApplicationStartMethod ( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.PreStart ) )]
[assembly: PostApplicationStartMethod( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.PostStart ) )]
[assembly: ApplicationShutdownMethod ( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.ApplicationShutdown ) )]

[assembly: OwinStartup( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.OwinStartup ) )]

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

			// If you are using ASP.NET Web Forms without any ASP.NET MVC functionality, remove the call to `.AddMvcDependencyResolver()`.
			// If you are using ASP.NET MVC, regardless of whether you're using ASP.NET Web Forms, use `.AddMvcDependencyResolver()`:

			_di = new ApplicationDependencyInjectionBuilder()
				.ConfigureServices( ConfigureServices )
				.AddMvcDependencyResolver()
#if USE_SCOPED_SIGNALR_RESOLVER
				.AddScopedSignalRDependencyResolver()
#else
				.AddUnscopedSignalRDependencyResolver()
#endif
				.Build();
		}

		private static void ConfigureServices( IServiceCollection services )
		{
			// TODO: Add any dependencies needed here
			_ = services
				// Useful services built-in to AspNetDependencyInjection:
				.AddDefaultHttpContextAccessor() // Adds `IHttpContextAccessor`
				.AddWebConfiguration() // Adds `IWebConfiguration`
				.AddSingleton<IUserIdProvider,SampleUserIdProvider>() // `IUserIdProvider` is a SignalR built-in service. SignalR's `PrincipalUserIdProvider` (the default implementation) is registered as a singleton. I'm unsure how well a transient or scoped registration would work.

				.AddSingleton<ISampleSingletonService,DefaultSingletonService>()

				.AddScoped<ISampleScopedService1,DefaultScopedService1>()
				.AddScoped<ISampleScopedService2,DefaultScopedService2>()

				.AddTransient<ISampleTransientService1,DefaultTransientService1>()
				.AddTransient<ISampleTransientService2,DefaultTransientService2>()
			;
		}

		public static void GlobalAsaxApplicationStart( System.Web.HttpApplication httpApplication )
		{
			System.Diagnostics.Debug.WriteLine( nameof(SampleApplicationStart) + "." + nameof(GlobalAsaxApplicationStart) + "() called: " + httpApplication.GetType().FullName );
		}

		/// <summary>This method must be public for <see cref="Microsoft.Owin.OwinStartupAttribute"/> to recognize it.</summary>
		public static void OwinStartup( IAppBuilder appBuilder )
		{
			System.Diagnostics.Debug.WriteLine( nameof(SampleApplicationStart) + "." + nameof(OwinStartup) + "() called." );

			HubConfiguration hubConfig = new HubConfiguration()
			{
				EnableDetailedErrors = true
			};

#if USE_SCOPED_SIGNALR_RESOLVER
			IDependencyResolver dr = GlobalHost.DependencyResolver;
			if( dr is AspNetDependencyInjection.Internal.ScopedAndiSignalRDependencyResolver dr2 )
			{
				dr2.ConfigureSignalR( appBuilder, path: "/signalr", hubConfiguration: hubConfig );
			}
			else
			{
				throw new InvalidOperationException( nameof(AspNetDependencyInjection.Internal.UnscopedAndiSignalRDependencyResolver) + " is not set-up." );
			}
#else
			appBuilder.MapSignalR( path: "/signalr", configuration: hubConfig );
#endif
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
