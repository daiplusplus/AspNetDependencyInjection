
using System;
using AspNetDependencyInjection;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Owin;
using SampleMvcWebApplication;

using WebActivatorEx;

[assembly: PreApplicationStartMethod( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.PreStart ) )]
[assembly: PostApplicationStartMethod( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.PostStart ) )]
[assembly: ApplicationShutdownMethod( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.ApplicationShutdown ) )]

[assembly: Microsoft.Owin.OwinStartup( typeof(SampleApplicationStart), methodName: nameof(SampleApplicationStart.OwinStartup) )]

namespace SampleMvcWebApplication
{
//	using ADIDependencyResolver = global::AspNetDependencyInjection.Internal.DependencyInjectionSignalRDependencyResolver;
//	using ADIHubActivator       = global::AspNetDependencyInjection.Internal.DependencyInjectionSignalRHubActivator;
//	using ADIHubDispatcher      = global::AspNetDependencyInjection.Internal.DependencyInjectionSignalRHubDispatcher;

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
//				.AddSignalRDependencyResolver()
				.AddClient( (di, rootSP) => new AspNetDependencyInjection.Internal.UnscopedAspNetDiSignalRDependencyResolver( di, rootSP ) )
				.Build();
		}

		private static void ConfigureServices( IServiceCollection services )
		{
			// TODO: Add any dependencies needed here
			services
				// Useful services built-in to AspNetDependencyInjection:
				.AddDefaultHttpContextAccessor() // Adds `IHttpContextAccessor`
				.AddWebConfiguration() // Adds `IWebConfiguration`
//				.AddSingleton<AspNetDependencyInjection.Internal.UnscopedDependencyInjectionSignalRHubActivator>()
//				.AddSingleton<IHubActivator, AspNetDependencyInjection.Internal.UnscopedDependencyInjectionSignalRHubActivator>()
				.AddTransient<IUserIdProvider,SampleUserIdProvider>()
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

#if ATTEMPT_1
			// https://github.com/simpleinjector/SimpleInjector/issues/232

			IDependencyResolver dr = GlobalHost.DependencyResolver;
			if( dr is ADIDependencyResolver dr2 )
			{
			
				HubConfiguration hubConfig = new HubConfiguration()
				{
					Resolver = GlobalHost.DependencyResolver
				};

				ADIHubDispatcher hubDispatcher = dr2.CreateHubDispatcher( hubConfig );

				hubConfig.Resolver.Register( typeof(ADIHubDispatcher), () => hubDispatcher );

				appBuilder.MapSignalR<ADIHubDispatcher>( "/signalr", hubConfig );
			}
			else
			{
				throw new InvalidOperationException( nameof(ADIDependencyResolver) + " is not set-up." );
			}

#else // ATTEMPT_2

			IDependencyResolver dr = GlobalHost.DependencyResolver;
			if( dr is AspNetDependencyInjection.Internal.UnscopedAspNetDiSignalRDependencyResolver dr2 )
			{
				//GlobalHost.DependencyResolver.Register( typeof(IUserIdProvider), () => dr2.GetRootRequiredService<IUserIdProvider>() );

				HubConfiguration hubConfig = new HubConfiguration()
				{
					EnableDetailedErrors = true
				};

				appBuilder.MapSignalR( "/signalr", hubConfig );
			}
			else
			{
				throw new InvalidOperationException( nameof(AspNetDependencyInjection.Internal.UnscopedAspNetDiSignalRDependencyResolver) + " is not set-up." );
			}
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
		}
	}
}