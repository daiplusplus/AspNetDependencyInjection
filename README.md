
# AspNetDependencyInjection

**AspNetDependencyInjection** allows "Classic" ASP.NET Web Forms applications to use `Microsoft.Extensions.DependencyInjection` to inject dependencies directly into `System.Web.UI.Page`, `UserControl`, and `MasterPage` constructors (and more!) all thanks to `WebObjectActivator`.

Currently listed on NuGet.org as:

* [`Jehoel.AspNetDependencyInjection`](https://www.nuget.org/packages/Jehoel.AspNetDependencyInjection/)
* [`Jehoel.AspNetDependencyInjection.Mvc`](https://www.nuget.org/packages/Jehoel.AspNetDependencyInjection.Mvc/)
* [`Jehoel.AspNetDependencyInjection.SignalR`](https://www.nuget.org/packages/Jehoel.AspNetDependencyInjection.SignalR/)
* [`Jehoel.AspNetDependencyInjection.WebApi`](https://www.nuget.org/packages/Jehoel.AspNetDependencyInjection.WebApi/)

## Background

* I'm working on a large ASP.NET WebForms application written almost a decade ago that needs to be moved to ASP.NET Core - I felt the best approach to transition is to work on a page-by-page basis: first updating each page to render directly instead of using WebControls (no more `if( this.IsPostBack )`!), then updating it to use constructor injected dependencies - which means that each `Page` behaves almost identically to an ASP.NET Core `Controller` which makes the transition far easier.

* This project is derived from my earlier `Unity.WebForms` project which provided DI using `Unity`, located at [https://github.com/Jehoel/Unity.WebForms](https://github.com/Jehoel/Unity.WebForms).
	* ...which itself is a derivative and fork of S. Kyle Korndoerfer's original project at [https://bitbucket.org/KyleK/unity.webforms](https://bitbucket.org/KyleK/unity.webforms)
		* ...which cites [DevTrends Unity.MVC3](http://nuget.org/packages/Unity.Mvc3/) and [DevTrends Unity.WCF](http://nuget.org/packages/Unity.Wcf/) as original works.

* This particular package's main objective is supporting ASP.NET Web Forms 4.7.2's new `WebObjectActivator` which means that `Page`, `UserControl` and other types can use true _constructor_ dependency injection. Previously applications had to use "property injection" which many consider to be an anti-pattern.

## Supported ASP.NET platforms

* This project now targets:
	* ASP.NET WebForms - By supporting `System.Web.HttpRuntime.WebObjectActivator`.
	* ASP.NET MVC - Using MVC's `System.Web.Mvc.IDependencyResolver`. Use the `Jehoel.AspNetDependencyInjection.Mvc` NuGet package.
	* ASP.NET SignalR - By subclassing `Microsoft.AspNet.SignalR.DefaultDependencyResolver`. Has optional support for `IServiceScope`. Use the `Jehoel.AspNetDependencyInjection.SignalR` NuGet package.
	* ASP.NET Web API - Using `System.Web.Http.Dependencies.IDependencyResolver`. Use the `Jehoel.AspNetDependencyInjection.WebApi` NuGet package.
	* WCF is not currently targeted by a published NuGet package, but a WCF example is included in this repository.

## Current Version
* 1.4 - S. Kyle Korndoerfer's most recent version of `Unity.WebForms`, released in 2015. See [https://bitbucket.org/KyleK/unity.webforms](https://bitbucket.org/KyleK/unity.webforms).
* 2.0 - My `Unity.WebForms` project, updated in 2019 for ASP.NET 4.7.2 and WebObjectActivator). See [https://github.com/Jehoel/Unity.WebForms](https://github.com/Jehoel/Unity.WebForms).
* 3.0 - After being extensively modified to use `Microsoft.Extensions.DependencyInjection` and renamed to `AspNetDependencyInjection` (as it now is unrelated to `Unity`.
* 3.2 - Adding support for ASP.NET MVC (which turned out to be broken).
* 3.3 - Bugfixing ASP.NET MVC support.
* 4.0 - Major refactoring and API redesign. Adding support for SignalR (both scoped and unscoped) and ASP.NET Web API.

## NuGet Gallery

```
Install-Package Jehoel.AspNetDependencyInjection

Install-Package Jehoel.AspNetDependencyInjection.Mvc

Install-Package Jehoel.AspNetDependencyInjection.SignalR

Install-Package Jehoel.AspNetDependencyInjection.WebApi
```

## Installation and Getting Started

Please see the `GETTING_STARTED.md` file in the GitHub repository: [https://github.com/Jehoel/AspNetDependencyInjection/blob/master/GETTING_STARTED.md](https://github.com/Jehoel/AspNetDependencyInjection/blob/master/GETTING_STARTED.md)

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
	* [Official Unity project for ASP.NET MVC](https://github.com/unitycontainer/aspnet-mvc)

## Copyright
* Copyright 2013 - 2015 [S. Kyle Korndoerfer](https://bitbucket.org/KyleK)
* Copyright 2019 [Dai Rees](https://github.com/Jehoel)


## License
`AspNetDependencyInjection` and all related prior projects are licensed under the MIT license - [http://www.opensource.org/licenses/mit-license](http://www.opensource.org/licenses/mit-license)
