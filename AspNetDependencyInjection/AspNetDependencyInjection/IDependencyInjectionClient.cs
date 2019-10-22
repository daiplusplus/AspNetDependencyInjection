using System;

namespace AspNetDependencyInjection
{
	/// <summary>Denotes a class that will be disposed when the associated <see cref="ApplicationDependencyInjection"/> object is disposed. This is intended for use by classes that wrap <see cref="ApplicationDependencyInjection"/> to provide or adapt DI services for non-MEDI consumers, for example, ASP.NET MVC, ASP.NET SignalR, and ASP.NET Web API all have their own separate DI resolver systems.</summary>
	public interface IDependencyInjectionClient : IDisposable
	{
	}
}
