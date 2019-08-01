# Installation, Getting Started and Troubleshooting guide for `Unity.WebForms` (`Jehoel.Unity.WebForms`)

## Installation and Getting Started steps:

### Step 1: Install the package:

1. In Visual Studio, open the Package Manager Console and ensure the "Default project" selector is your web-application project.
2. Run `Install-Package Jehoel.Unity.AspNetWebForms`
3. This will add the package reference to your project. Now you need to configure the Unity container.

### Step 2: Copy this `ConfigureServices` file into your project:

* Note that previous versions of the NuGet `Unity.WebForms` package added a starter file to the parent project at `App_Start\UnityWebFormsStart.cs`.
	* This has now been removed from `Jehoel.Unity.WebForms` because NuGet packages are not meant to include mutable content files intended to be edited by the consuming project's author, instead only immutable content files are meant to be included.
	* So you need to manually add code that configure the web-application's root Unity Container.

* Personally, I put this file in the same directory as `Global.asax` and name it `Global-Unity.asax.cs` just so all of my application's startup code is together.
	* Though if you want to be consistent with other OWIN conventions, feel free to put it in `App_Start\UnityWebFormsStart.cs`.
		* Note that the `App_Start` directory is *not* special in ASP.NET 4 (unlike the `App_Code`, `App_Data`, `App_Theme`, `App_Browser` and `App_WebReference` folders).

```
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
                // Examples include services that depend on request-lifetime-limited ILogger instances
                // WARNING: Do not register services that implement `IDisposable` with `RegisterType` unless your code will be responsible for wrapping the returned object in a `using(service){}` block or otherwise manually calling `.Dispose()` as Unity will not dispose of transient objects.
                .RegisterType<ITransientService,TransientServiceImplementation>();
        }
    }
}
```

### Step 3: Customize `RegisterDependencies` by adding your service registrations.

This README assumes you're already familiar with Unity and/or DI service registration in general.

This project adds a new set of extension method to `IUnityContainer` to simplify service registration for services that should have their lifetimes constrained to their associated HTTP request. `RegisterRequest`. There are many overloads for registration using generic types or `System.Type`, named and unnamed registrations, and so on.

## Frequently asked questions

(Actually, I have never been asked these questions, but I imagine people attempting to use this library might ask these questions)

### What if you need to access the `HttpContext` (`HttpContextBase`) associated with the current request in a request-scoped service?

This project adds a built-in service: `Unity.WebForms.IHttpContextAccessor`. This service is registered only in the per-request child `IUnityContainer` to prevent inadvertent resolution of `IHttpContentAccessor` 

### What if I need access to values in web.config like `<appSettings>`?

To avoid making a static reference to `WebConfigurationManager` and to make your components testable, use `Unity.WebForms.Services.IWebConfiguration`. Add it using `container.AddWebConfiguration()`.

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

Another advantage of this approach is that if you need to use a `DbContext` inside a non-page-lifetime component of your web-application you can take add `MyDbContextFactory` to your constructor parameters and get a short-lived `DbContext` that way without needing to create a new child `IUnityContainer` though you would be responsible for disposing it.


## Included services

All included services are exposed as interfaces so you can replace them with your own implementation for testing purposes or for different production scenarios.

### `Unity.WebForms.IHttpContextAccessor`

* Provides thread-safe access to `HttpContext` (as a `HttpContextBase`).
* Always automatically registered in the per-request child `IUnityContainer`.

### `Unity.WebForms.Services.IWebConfiguration`

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

### <abbr title="Yellow Screen of Death">YSoD</abbr> error: "Constructor on type 'ASP.Default_aspx' not found."

(Where `Default_aspx` is the web-page, master-page or user-control being used)

When running your webapplication you may get a yellow-screen-of-death with a stack-trace similar to this:

    [MissingMethodException: Constructor on type 'ASP.login_aspx' not found.]
       System.RuntimeType.CreateInstanceImpl(BindingFlags bindingAttr, Binder binder, Object[] args, CultureInfo culture, Object[] activationAttributes, StackCrawlMark& stackMark) +1431
       System.Activator.CreateInstance(Type type, BindingFlags bindingAttr, Binder binder, Object[] args, CultureInfo culture, Object[] activationAttributes) +184
       Unity.WebForms.UnityContainerServiceProvider.DefaultCreateInstance(Type type) +32
       Unity.WebForms.UnityContainerServiceProvider.GetService(Type serviceType) +375
       __ASP.FastObjectFactory_app_web_xkkmukhw.Create_ASP_Defaultaspx() +118

First, perform the .NET Framework target version verification checks described immediately underneath the "Troubleshooting" header above.

This can happen when the intermediate files (when your `*.aspx`, `*.ascx`, and `*.master` files are transpiled to `*.cs`) were created without support for `WebObjectActivator`. Delete all intermediate and output files from your project to cause Visual Studio and ASP.NET to recreate them with WebObjectActivator support. You should only have to do this once.

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
