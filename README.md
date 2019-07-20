# Unity.WebForms

**Unity.WebForms** is a collection of various ideas on Dependency Injection utilizing the [Unity](http://unity.codeplex.com/) container in ASP.NET WebForm applications, formalized into a single NuGet package for easy integration into an existing application.

This package was inspired by similar packages from DevTrends, specifically:

* [Unity.MVC3](http://nuget.org/packages/Unity.Mvc3/ "DevTrends Unity.MVC3")
* [Unity.WCF](http://nuget.org/packages/Unity.Wcf/ "DevTrends Unity.WCF")

The only difference is that this packages is a DLL integration with minimal source code added. The only source file added is to allow the configuration of the Unity container with your types.

This particular repo (at https://github.com/Jehoel/Unity.WebForms) is a git fork of S. Kyle Korndoerfer's original project at https://bitbucket.org/KyleK/unity.webforms with a main objective of supporting ASP.NET WebForms 4.7.2's `WebObjectActivator` which means that `Page`, `UserControl` and other types can use true constructor dependency injection.

This repo's objectives are:
* Update to .NET Framework 4.7.2 (done)
* Update existing dependencies, including Unity 5 (done)
* Add in support for WebObjectActivator, as per Microsoft's example at https://github.com/aspnet/AspNetWebFormsDependencyInjection (done)
* Update the sample project (done)
* Publish to NuGet (under a different name) (TODO)

## Current Version
* 1.4 (right before .NET 4.7.2 support)
* 2.0 (this repo and project)

## NuGet Gallery

### Version 1.4 (.NET 4.5, no support for WebObjectActivator)

[http://nuget.org/packages/Unity.WebForms](http://nuget.org/packages/Unity.WebForms)

### Version 2.0 (.NET 4.7.2, with support for WebObjectActivator)



## Installation
	Install-Package Unity.WebForms

## Quick Start
Once installed, the Unity.WebForms package will have added the necessary dependencies and references to your project along with a (potentially) new folder named `App_Start`. Inside this folder, there will be a new class file named `UnityWebFormsStart.cs`. The registration of any dependencies you have should be done in the `RegisterDependencies( IUnityContainer container )` method.

Detailed information about how this project works can be found in the [wiki][]

## References / Links
Here are some of the sources used for building out this pacakge:

* [Dependency Injection in ASP.NET WebForms](http://litemedia.info/dependency-injection-in-asp.net-webforms) - Mikael Lundin
* [Unity: How to registerType with a PARAMETER constructor](http://stackoverflow.com/a/4007337)
* [Unity.MVC3](http://unitymvc3.codeplex.com/) - The linked articles on the bottom helped a lot with infrastructure


## Copyright
* Copyright 2013 - 2015 S. Kyle Korndoerfer
* Copyright 2019 Dai Rees ( https://github.com/Jehoel )


## License
Unity.WebForms is under the MIT license - [http://www.opensource.org/licenses/mit-license](http://www.opensource.org/licenses/mit-license)

[wiki]:https://bitbucket.org/KyleK/unity.webforms/wiki/
