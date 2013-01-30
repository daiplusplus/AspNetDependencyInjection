# Unity.WebForms

Unity.WebForms is a collection of various ideas on Dependency Injection utilizing the [Unity](http://unity.codeplex.com/) container in ASP.NET WebForm applications, formalized into a single NuGet package for easy integration into an existing application.

This package follows the format set out by similar packages from DevTrends, specifically:

* [Unity.MVC3](http://nuget.org/packages/Unity.Mvc3/ "DevTrends Unity.MVC3")
* [Unity.WCF](http://nuget.org/packages/Unity.Wcf/ "DevTrends Unity.WCF")

The only difference is that this packages is a DLL integration with minimal source code added. The only source file added is to allow the configuration of the Unity container.

## Usage

### Installation

	Install-Package Unity.WebForms

### Usage

Once installed, the Unity.WebForms package will have added the necessary dependencies and references to your project along with a (potentially) new folder named `App_Start`. Inside this folder, a new class file named `UnityWebFormsStart.cs` that has 2 methods in it. The registration of any dependencies you have should be done in the `RegisterDependencies( IUnityContainer container )` method.

#### NOTE:
The last line of the method adds the initialized container to the Application item collection. This is where each request gets it's child container instance from. If this line get deleted or modified, then the rest of the plumbing will not work.

## References / Links

Here are some of the sources used for building out this pacakge:

* [Dependency Injection in ASP.NET WebForms](http://litemedia.info/dependency-injection-in-asp.net-webforms) - Mikael Lundin
* [Unity: How to registerType with a PARAMETER constructor](http://stackoverflow.com/a/4007337)
* [Unity.MVC3](http://unitymvc3.codeplex.com/) - The linked articles on the bottom helped a lot with infrastructure