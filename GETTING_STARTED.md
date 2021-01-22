# Installation, Getting Started and Troubleshooting guide for `AspNetDependencyInjection` (`Jehoel.AspNetDependencyInjection`)

## Installation and Getting Started steps:

### Step 1: Install the package:

1. In Visual Studio, open the Package Manager Console and ensure the "Default project" selector is your web-application project.
2. Run `Install-Package Jehoel.AspNetDependencyInjection`
3. This will add the package reference to your project in addition to its other dependencies such as `Microsoft.Extensions.DependencyInjection`.
4. Now you need to configure the Dependency Injection `IServiceProvider` in your application's startup logic. Continue to step 2 below.

### Step 2: Copy this `ConfigureServices` file into your project:

As NuGet does not recommend providing project files in NuGet packages anymore no startup or service configuration file will be added to your project when you install the `Jehoel.AspNetDependencyInjection` package, instead copy the code below to a new file and uncomment portions as-required, and provide values for the placeholders.

```
using System;

using Microsoft.Extensions.DependencyInjection;
using SampleWebApplication;

using Owin;
using Microsoft.Owin;
using AspNetDependencyInjection;
using WebActivatorEx;

[assembly: PreApplicationStartMethod ( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.PreStart  ) )]
//[assembly: PostApplicationStartMethod( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.PostStart ) )] // Uncomment this if you have any Post-start logic you want to run.

//[assembly: OwinStartup( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.OwinStartup ) )] // Uncomment this if you're using SignalR or other features that depend on OWIN.

namespace SampleWebApplication
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
				//.AddMvcDependencyResolver() // Uncomment this out if you're using ASP.NET MVC.
				//.AddWebApiDependencyResolver() // Uncomment this out if you're using ASP.NET Web API.
				//.AddScopedSignalRDependencyResolver() // Uncomment this out if you're using ASP.NET SignalR (and want to use services scoped to request or operation lifetime). NOTE: You cannot have both `AddScopedSignalRDependencyResolver` and `AddUnscopedSignalRDependencyResolver` at the same time. You must also configure SignalR below.
				//.AddUnscopedSignalRDependencyResolver() // Uncomment this out if you're using ASP.NET SignalR (and only need Singleton or Transient lifetime services).
				.Build();
		}

		private static void ConfigureServices( IServiceCollection services )
		{
			// TODO: Add any dependencies needed here
			services
				// Useful services built-in to AspNetDependencyInjection:
				.AddDefaultHttpContextAccessor() // Adds `IHttpContextAccessor`
				.AddWebConfiguration() // Adds `IWebConfiguration`

				// Example services:
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

	}
}
```

### Step 3: Customize `ConfigureServices` by adding your service registrations.

This README assumes you're already familiar with `Microsoft.Extensions.DependencyInjection` so no further instructions are provided for this step.

## Frequently asked questions

(Actually, I have never been asked these questions, but I imagine people attempting to use this library might ask these questions)

### Can I use constructor dependency-injection with my `HttpModule` and `HttpHandler` classes? (`IHttpModule`, `IHttpHandler`, etc)

**Yes!** Provided that your HttpModule or HttpHandler is instantiated after the Dependency Injection system is set-up so that `WebObjectActivator` is set, as ASP.NET will use `WebObjectActivator` to instantiate the HttpModule and HttpHandler instances.

### Can I use constructor dependency-injection with `*.asmx` files?

I don't know - I haven't tested `.asmx` files. However _in theory_ it should work. If your ASMX file inherits from an `abstract class` with a `protected` constructor you might have issues unless the constructor is `public` (but the class itself can still be `abstract`).

### What if you need to access the `HttpContext` (`HttpContextBase`) associated with the current request in a request-scoped service?

This project adds a built-in service: `AspNetDependencyInjection.IHttpContextAccessor`. You can add this service by using `.AddDefaultHttpContextAccessor()` in your `ConfigureServices` method.

The implementation of `IHttpContextAccessor` does not use `HttpContext.Current`, but uses an internal strong reference to the HttpContextWrapper created for the `IServiceScope` in the `HttpContextScopeHttpModule` which means that **`IHttpContextAccessor` is thread-safe!** and can safely be used in `async` contexts regardless of the current `System.Threading.SynchronizationContext`.

### What if I need access to values in web.config like `<appSettings>`?

To avoid making a static reference to `WebConfigurationManager` and to make your components testable, use `AspNetDependencyInjection.IWebConfiguration`. Add it using `services.AddWebConfiguration()`.

`IWebConfiguration` provides read-only access to:

* `<appSettings>` (as a `IReadOnlyDictionary<String,String>` in `IWebConfiguration.AppSettings`)
* `<connectionStrings>` (as a `IReadOnlyDictionary<String,ConnectionStringSettings>` in `IWebConfiguration.ConnectionStrings`)
* And any other section that uses `DictionarySectionHandler`, `NameValueSectionHandler`, `NameValueFileSectionHandler`, `SingleTagSectionHandler`, or any `ConfigurationSection` that is readable via `System.Collections.IDictionary` or `NameValueCollection`.
    * These configuration sections are exposed as a `IReadOnlyDictionary<String,String>` via `IWebConfiguration.GetKeyValueSection`.

### What about Entity Framework's `DbContext`?

To have a DbContext that is lifetime-limited to each HTTP request ("scoped") with a named connection-string pulled from your `web.config` file, use the `AddScopedWithFactory` extension. You can use the code below:

```
public class MyDbContextFactory : IServiceFactory<MyDbContext>
{
    private readonly String connectionString;

    public MyDbContextFactory( IWebConfiguration webConfig )
    {
        if( webConfig == null ) throw new ArgumentNullException(nameof(webConfig));

        this.connectionString = webConfig.RequireConnectionString( "myDbConnectionString" ); // Note that `RequireConnectionString` is an extension method.
    }

    public MyDbContext CreateInstance()
    {
        return new MyDbContext( this.connectionString );
    }
}
```

Then your registration code should look like this:

```
services
    .AddWebConfiguration()
    .AddScopedWithFactory<MyDbContext,MyDbContextFactory>();
```

Or if you don't wish to implement `IServiceFactory<TService>`, the equivalent long-form of the above is:

```
services
    .AddWebConfiguration()
    .AddSingleton<MyDbContextFactory>()
    .AddScoped<MyDbContext,MyDbContextFactory>( sp => sp.GetRequiredService<MyDbContextFactory>().CreateInstance() );
```

If you need to choose your connection-string at runtime based on an `<appSetting>` value, then modify the `MyDbContextFactory` constructor to use `IWebConfigurationExtensions.RequireIndirectConnectionString` instead of `RequireConnectionString`.

Another advantage of this approach is that if you need to use a `DbContext` inside a non-page-lifetime component of your web-application you can take add `MyDbContextFactory` to your constructor parameters and get a short-lived `DbContext` that way without needing to create a new `IServiceScope`, though you would be responsible for disposing of the `DbContext`.

### How does the `PreStart` / `WebActivatorEx.PreApplicationStartMethod` method work with or interact with OWIN's Startup method?

* `WebActivatorEx`'s `PreApplicationStartMethod` (the `PreStart` method in our sample above) runs **before** OWIN's Startup method.
	* See this StackOverflow post: https://stackoverflow.com/questions/21462777/webactivatorex-vs-owinstartup

## Included services

All included services are exposed as interfaces so you can replace them with your own implementation for testing purposes or for different production scenarios. They are listed below:

### `AspNetDependencyInjection.IHttpContextAccessor`

* Provides thread-safe access to `HttpContext` (as a `HttpContextBase`).
* This service is not added by default.
* It is registered by your application by using `services.AddDefaultHttpContextAccessor()`.

### `AspNetDependencyInjection.IWebConfiguration`

* Provides access to `WebConfigurationManager`.
* This service is not added by default.
* It is registered by your application by using `services.AddWebConfiguration()`.

## Troubleshooting

When performing any troubleshooting involving ASP.NET's built-in support for constructor dependency injection in `*.aspx`, `*.ascx`, and `*.master` files, it is important to verify that the project is *fully* targeting .NET Framework 4.7.2 or later:

* Verify that your `web.config` file specifically targets .NET Framework 4.7.2 or later, as below:

    ```
    <configuration>
        <system.web>
            <compilation targetFramework="4.7.2" />
            <httpRuntime targetFramework="4.7.2" />
        </system.web>
    </configuration>
    ```

* Verify that your `*.csproj` is targeting .NET Framework 4.7.2 or later:
	* Project Properties > Application > Target framework > ".NET Framework 4.7.2" (or later)
		* If you don't see  ".NET Framework 4.7.2" listed, ensure you have the [NET Framework 4.7.2 SDK installed](https://dotnet.microsoft.com/download/visual-studio-sdks).
		* Also verify you're using a version of Visual Studio that supports .NET Framework 4.7.2, such as Visual Studio 2017 or Visual Studio 2019.

	* Or edit your `*.csproj` file in a text editor:

    ```
    <Project>
        <PropertyGroup>
            <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
    ```

* Also, make sure you're not using an older version of any CodeDom, Roslyn or CompilerServices packages or assembly references.
	* This includes these NuGet packages:
		* `Microsoft.Net.Compilers` (currently at version `3.2.0` at time of writing)
		* `Microsoft.CodeDom.Providers.DotNetCompilerPlatform` (currently at version `2.0.1` at time of writing)

	* These packages don't seem to be required to use WebObjectActivator-based constructors.

* Delete any potentially stale temporary ASP.NET intermediate files and runtime compilation files

	* Issues can arise when the ASP.NET's intermediate files (when your `*.aspx`, `*.ascx`, and `*.master` files are transpiled to `*.cs`) were created without support for `WebObjectActivator`.
	* Delete all intermediate and output files from your project to cause Visual Studio and ASP.NET to recreate them with WebObjectActivator support. You should only have to do this once after updating your project to target the .NET Framework 4.7.2
	* See the heading "Steps to take to delete all output, intermediate, and temporary files" below.

### <abbr title="Yellow Screen of Death">YSoD</abbr> error: "Constructor on type 'ASP.Default_aspx' not found."

(Where `Default_aspx` is the web-page, master-page or user-control being used)

When running your webapplication you may get a yellow-screen-of-death with a stack-trace similar to this:

```
    [MissingMethodException: Constructor on type 'ASP.login_aspx' not found.]
       System.RuntimeType.CreateInstanceImpl(BindingFlags bindingAttr, Binder binder, Object[] args, CultureInfo culture, Object[] activationAttributes, StackCrawlMark& stackMark) +1431
       System.Activator.CreateInstance(Type type, BindingFlags bindingAttr, Binder binder, Object[] args, CultureInfo culture, Object[] activationAttributes) +184
       [...]
       __ASP.FastObjectFactory_app_web_a1b2c3d4.Create_ASP_Defaultaspx() +118
```

This happens when the configured `IServiceProvider` is unable to resolve constructor parameters for your Page (`.aspx`,) User Control (`.ascx`), Master Page (`.master`), Handler (`.ashx`) or XML Web Service (`.asmx`) files' `Inherits=""` classes.

* First, perform the .NET Framework target version verification checks described immediately underneath the "Troubleshooting" header above.

* This can also happen happen with User Controls (`.ascx`) which `Inherits=""` from an `abstract class` (deriving from `UserControl`) with only a `protected` constructor. 
  * Changing the constructor to `public` and removing the `abstract` class modifier will get it working again.
  * Oddly, this only affects User Controls - Pages can still inherit from abstract classes with protected constructors, however. I don't know why ASP.NET allows pages to have protected constructors but not user-controls.
  * Also, the code-behind's parent class can still be `abstract` and can also have a `protected` constructor - this is okay because ASP.NET and `WebObjectActivator` doesn't call that constructor directly.

For example, if you have this:

**MyUserControl.ascx**
```
<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="MyUserControl.ascx.cs" Inherits="MyProject.MyUserControl" %>
<blockquote>
<p>Life is short and love is always over in the morning.</p>
</blockquote>
```

**MyUserControl.ascx.cs**
```
using System.Web.UI;

namespace MyProject
{
    public abstract class MyUserControl : UserControl // <-- Change this to `public class MyUserControl`
    {
        protected MyUserControl( MyDbContext db ) // <-- Change this to `public`.
	{
	    // ...
	}
    }
}
```


### `System.MissingMethodException` - "Method not found: Void MyProject.MyPage..ctor(IService1 service1, IService2 service2)"

The stack-trace resembles this:

```
   at lambda_method(Closure , IServiceProvider , Object[] )
   at AspNetDependencyInjection.Internal.DependencyInjectionWebObjectActivator.GetService(Type serviceType)
   at __ASP.FastObjectFactory_app_web_mypage_aspx_aabbccdd_eeff_ghi.Create_ASP_mypage_aspx()
```

This happens when a single page is compiled or recompiled separately from the rest of the web-application.

* If you get this error in a website that has already been published and/or is in-production, then rebuild the entire web-application as normal (i.e. on your dev machine, build-server, CI/CD process, etc) then re-publish - ensuring that all binary files are in-sync and that no `*.dll` files from a previous publish are kept-around.

* If you get this error during development, check your `web.config` file's `<configuration> <system.web> <compilation>` element and ensure that you are not using `optimizeCompilation="true"` and `batch="false"`.
    * While these options do improve startup time during development, they do not cause the `FastObjectFactory` to be rebuilt (which had the DI constructor calls), which causes this exception.

### Steps to take to delete all output, intermediate, and temporary files:

* Nuke your `Temporary ASP.NET Files` folder - this contains the `*.cs` transpiled source versions of your `*.aspx`, `*.ascx`, and `*.master` files).
    * On your development machine, it will be located at `C:\Users\%you%\AppData\Local\Temp\Temporary ASP.NET Files`
    * In production servers, it is located at:
        * `C:\Windows\Microsoft.NET\Framework\v4.0.30319\Temporary ASP.NET Files`
        * `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files`
* Ensure that you have fully closed IIS Express (or the full IIS if applicable)
* Verify that you have performed a proper Clean and Build of your project (and it doesn't hurt to restart Visual Studio too).
* Ensure the `bin` and `obj` directories of your ASP.NET are empty before doing a Build.
* Verify that your ASP.NET project is specifically targeting .NET Framework 4.7.2 in the `*.csproj` file, as per the instructions above.
