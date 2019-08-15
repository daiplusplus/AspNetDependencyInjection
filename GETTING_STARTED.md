# Installation, Getting Started and Troubleshooting guide for `AspNetDependencyInjection` (`Jehoel.AspNetDependencyInjection`)

## Installation and Getting Started steps:

### Step 1: Install the package:

1. In Visual Studio, open the Package Manager Console and ensure the "Default project" selector is your web-application project.
2. Run `Install-Package Jehoel.AspNetDependencyInjection`
3. This will add the package reference to your project in addition to its other dependencies such as `Microsoft.Extensions.DependencyInjection`.
4. Now you need to configure the Dependency Injection `IServiceProvider` in your application's startup logic. Continue to step 2 below.

### Step 2: Copy this `ConfigureServices` file into your project:

(As NuGet does not recommend providing project files in NuGet packages anymore, so no startup or service configuration file will be added to your project when you install the package, instead copy the code below to a new file and provide values for the placeholders)

```
using System;

using Microsoft.Extensions.DependencyInjection;
using SampleWebApplication;

using WebActivatorEx;
using AspNetDependencyInjection;

[assembly: PreApplicationStartMethod ( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.PreStart  ) )]
//[assembly: PostApplicationStartMethod( typeof( SampleApplicationStart ), methodName: nameof( SampleApplicationStart.PostStart ) )] // uncomment this if you have any Post-start logic you want to run.

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

			_di = ApplicationDependencyInjection.Configure( ConfigureServices );
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

			//_di.Reconfigure( ReconfigureServices );
		}

		private static void ReconfigureServices( IServiceCollection services )
		{
			
		}
	}
}
```

### Step 3: Customize `ConfigureServices` by adding your service registrations.

This README assumes you're already familiar with `Microsoft.Extensions.DependencyInjection`.

## Frequently asked questions

(Actually, I have never been asked these questions, but I imagine people attempting to use this library might ask these questions)

### Can I use constructor dependency-injection with my HttpModule and HttpHandler classes? (`IHttpModule`, `IHttpHandler`, etc)

Yes! Provided that your HttpModule or HttpHandler is instantiated after the Dependency Injection system is set-up so that `WebObjectActivator` is set, as ASP.NET will use `WebObjectActivator` to instantiate the HttpModule and HttpHandler instances.

### What if you need to access the `HttpContext` (`HttpContextBase`) associated with the current request in a request-scoped service?

This project adds a built-in service: `AspNetDependencyInjection.IHttpContextAccessor`. You can add this service by using `.AddDefaultHttpContextAccessor()` in your `ConfigureServices` method.

### What if I need access to values in web.config like `<appSettings>`?

To avoid making a static reference to `WebConfigurationManager` and to make your components testable, use `AspNetDependencyInjection.IWebConfiguration`. Add it using `container.AddWebConfiguration()`.

`IWebConfiguration` provides access to:

* `<appSettings>`
* `<connectionStrings>`
* And any other section that uses `DictionarySectionHandler`, `NameValueSectionHandler`, `SingleTagSectionHandler`, and others.

### What about Entity Framework's `DbContext`?

`DbContext` should be registered using `RegisterRequestFactory`. Supply a factory method (or `IServiceFactory` implementation) that uses the `DbContext(String connectionString)` constructor with the connection-string pulled from your configuration system.

If you're using `web.config` for configuration then you can define an `IServiceFactory<DbContext>` which itself takes a dependency on `IWebConfiguration` to get the connection-string from your web.config file:

```
public class MyDbContextFactory : IServiceFactory<MyDbContext>
{
    private readonly String connectionString;

    public MyDbContextFactory( IWebConfiguration webConfig )
    {
        if( webConfig == null ) throw new ArgumentNullException(nameof(webConfig));

        this.connectionString = webConfig.RequireConnectionString( "myDbConnectionString" ); // `RequireConnectionString` is an extension method.
    }

    public MyDbContext CreateInstance()
    {
        return new MyDbContext( this.connectionString );
    }
}
```

Then your registration code should look like this:

```
container
    .AddWebConfiguration()
    .RegisterRequestFactory<MyDbContext,MyDbContextFactory>();
```

If you need to choose your connection-string at runtime based on an `<appSetting>` value, then use `IWebConfigurationExtensions.RequireIndirectConnectionString` instead of `RequireConnectionString`.

Another advantage of this approach is that if you need to use a `DbContext` inside a non-page-lifetime component of your web-application you can take add `MyDbContextFactory` to your constructor parameters and get a short-lived `DbContext` that way without needing to create a new `IServiceScope` though you would be responsible for disposing it.


## Included services

All included services are exposed as interfaces so you can replace them with your own implementation for testing purposes or for different production scenarios.

### `AspNetDependencyInjection.IHttpContextAccessor`

* Provides thread-safe access to `HttpContext` (as a `HttpContextBase`).
* This service is must be registered by your application by using `services.AddDefaultHttpContextAccessor()`.

### `AspNetDependencyInjection.IWebConfiguration`

* Provides access to `WebConfigurationManager`.
* Requires manual registration. Call `container.AddWebConfiguration()`.
* Comes with extension methods to easily get connection-strings directly by name or indirectly via a named `<appSettings>` entry.

## Troubleshooting

When performing any troubleshooting involving ASP.NET's built-in support for constructor dependency injection in `*.aspx`, `*.ascx`, and `*.master` files, it is important to verify that the project is *fully* targeting .NET Framework 4.7.2:

* Verify that your `web.config` file specifically targets .NET Framework 4.7.2, as below:

    <configuration>
        <system.web>
            <compilation targetFramework="4.7.2" />
            <httpRuntime targetFramework="4.7.2" />
        </system.web>
    </configuration>

* Verify that your `*.csproj` is targeting .NET Framework 4.7.2 or later:
	* Project Properties > Application > Target framework > .NET Framework 4.7.2
		* If you don't see  ".NET Framework 4.7.2" listed, ensure you have the [NET Framework 4.7.2 SDK installed](https://dotnet.microsoft.com/download/visual-studio-sdks).
		* Also verify you're using a version of Visual Studio that supports .NET Framework 4.7.2, such as Visual Studio 2017 or Visual Studio 2019.

	* Or edit your `*.csproj` file in a text editor:

	    <Project>
		    <PropertyGroup>
	            <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>

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

    [MissingMethodException: Constructor on type 'ASP.login_aspx' not found.]
       System.RuntimeType.CreateInstanceImpl(BindingFlags bindingAttr, Binder binder, Object[] args, CultureInfo culture, Object[] activationAttributes, StackCrawlMark& stackMark) +1431
       System.Activator.CreateInstance(Type type, BindingFlags bindingAttr, Binder binder, Object[] args, CultureInfo culture, Object[] activationAttributes) +184
       [...]
       __ASP.FastObjectFactory_app_web_a1b2c3d4.Create_ASP_Defaultaspx() +118

This happens when the configured `IServiceProvider` is unable to resolve constructor parameters for your Page, User Control or 

First, perform the .NET Framework target version verification checks described immediately underneath the "Troubleshooting" header above.

This can happen 

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
