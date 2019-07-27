
# Unity.WebForms

(Currently listed on NuGet.org as `Jehoel.Unity.AspNetWebForms`)

**Unity.WebForms** is a collection of various ideas on Dependency Injection utilizing the [Unity](http://unity.codeplex.com/) container in ASP.NET WebForm applications, formalized into a single NuGet package for easy integration into an existing application.

This package was inspired by similar packages from DevTrends, specifically:

* [DevTrends Unity.MVC3](http://nuget.org/packages/Unity.Mvc3/)
* [DevTrends Unity.WCF](http://nuget.org/packages/Unity.Wcf/)

The only difference is that this packages is a DLL integration with minimal source code added. The only source file added is to allow the configuration of the Unity container with your types.

This particular package (`Jehoel.Unity.AspNetWebForms`) and repo (at [https://github.com/Jehoel/Unity.WebForms](https://github.com/Jehoel/Unity.WebForms)) is a git fork of S. Kyle Korndoerfer's original project at [https://bitbucket.org/KyleK/unity.webforms](https://bitbucket.org/KyleK/unity.webforms) with a main objective of supporting ASP.NET WebForms 4.7.2's new `WebObjectActivator` which means that `Page`, `UserControl` and other types can use true constructor dependency injection.

This repo's objectives are:
* Update to .NET Framework 4.7.2 (done)
* Update existing dependencies, including Unity 5 (done)
* Add in support for WebObjectActivator, as per Microsoft's example at https://github.com/aspnet/AspNetWebFormsDependencyInjection (done)
* Update the sample project (done)
* Publish to NuGet (`Jehoel.Unity.AspNetWebForms`) (done)
* Add more tests (in progress)

## Current Version
* 1.4 (S. Kyle Korndoerfer's most recent version, released in 2015)
* 2.0 (this repo and project, updated in 2019 for ASP.NET 4.7.2 and WebObjectActivator)

## NuGet Gallery

### Version 1.4 (.NET 4.5, no support for WebObjectActivator)

[http://nuget.org/packages/Unity.WebForms](http://nuget.org/packages/Unity.WebForms)

### Version 2.0 (.NET 4.7.2, with support for WebObjectActivator)

[http://nuget.org/packages/Jehoel.Unity.AspNetWebForms](http://nuget.org/packages/Jehoel.Unity.AspNetWebForms)

## Installation and Getting Started

### Step 1: Install the package:

1. In Visual Studio, open the Package Manager Console and ensure the "Default project" selector is your web-application project.
2. Run `Install-Package Jehoel.Unity.AspNetWebForms`
3. This will add the package reference to your project. Now you need to configure the Unity container.

### Step 2: Configure the Unity container:

Previous versions of the NuGet package added a starter file to the parent project at `App_Start\UnityWebFormsStart.cs`.
This has now been removed because NuGet packages are not meant to include mutable content files intended to be edited by the consuming project's author, instead only immutable content files are meant to be included.
So you'll then need add your own file to configure the web-application's Unity Container.

#### Step 2.1.: Copy this `ConfigureServices` file into your project:

Personally, I put this file in the same directory as `Global.asax` and name it `Global-Unity.asax.cs` just so all of my application's startup code is together (the `App_Start` directory is *not* special in ASP.NET 4 (unlike `App_Code`, `App_Data`, `App_Theme`, `App_Browser` and `App_WebReference` folders).


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

#### Step 2.2.: Add your service registrations.

This README assumes you're already familiar with Unity and/or DI service registration in general.

This project adds a new set of extension method to `IUnityContainer` to simplify service registration for services that should have their lifetimes constrained to their associated HTTP request. `RegisterRequest`. There are many overloads for registration using generic types or `System.Type`, named and unnamed registrations, and so on.

### Step 3: Write your services.

#### What if you need to access the `HttpContext` (`HttpContextBase`) associated with the current request in a request-scoped service?

This project adds a built-in service: `Unity.WebForms.IHttpContextAccessor`. This service is registered only in the per-request child `IUnityContainer` to prevent inadvertent resolution of `IHttpContentAccessor` 

#### What if I need access to values in web.config like `<appSettings>`?

To avoid making a static reference to `WebConfigurationManager` and to make your components testable, use `Unity.WebForms.Services.IWebConfiguration`. Add it using `container.AddWebConfiguration()`.

`IWebConfiguration` provides access to:

* `<appSettings>`
* `<connectionStrings>`
* And any other section that uses `DictionarySectionHandler`, `NameValueSectionHandler`, `SingleTagSectionHandler`, and others.

#### What about Entity Framework's `DbContext`?

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

## References / Links
Here are some of the sources used for building out this package:

* `Unity.WebForms`:
	* [Dependency Injection in ASP.NET WebForms](http://litemedia.info/dependency-injection-in-asp.net-webforms) - Mikael Lundin
	* [Unity: How to registerType with a PARAMETER constructor](http://stackoverflow.com/a/4007337)
	* [Unity.MVC3](http://unitymvc3.codeplex.com/) - The linked articles on the bottom helped a lot with infrastructure
* `Jehoel.Unity.AspNetWebForms`:
	* [Announcing the .NET Framework 4.7.2](https://devblogs.microsoft.com/dotnet/announcing-the-net-framework-4-7-2/) - This article announced support for constructor-based DI.
	* [Use Dependency Injection In WebForms Application](https://devblogs.microsoft.com/aspnet/use-dependency-injection-in-webforms-application/)
	* [AspNetWebFormsDependencyInjection on Github](https://github.com/aspnet/AspNetWebFormsDependencyInjection)
	* [Sample example of using AspNetWebFormsDependencyInjection](https://github.com/Jinhuafei/examples/tree/master/DependencyInjection) - ([Repository snapshot](https://github.com/Jinhuafei/examples/tree/c6ddec606c710dde3a3c8747067d088c261d0cff))


## Copyright
* Copyright 2013 - 2015 [S. Kyle Korndoerfer](https://bitbucket.org/KyleK)
* Copyright 2019 [Dai Rees](https://github.com/Jehoel)


## License
Unity.WebForms is under the MIT license - [http://www.opensource.org/licenses/mit-license](http://www.opensource.org/licenses/mit-license)

[wiki]:https://bitbucket.org/KyleK/unity.webforms/wiki/
