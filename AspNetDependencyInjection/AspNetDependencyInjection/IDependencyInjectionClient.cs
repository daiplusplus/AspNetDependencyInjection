using System;
using System.Collections.Generic;

namespace AspNetDependencyInjection
{
	/// <summary>Denotes a class that will be disposed when the associated <see cref="ApplicationDependencyInjection"/> object is disposed. This is intended for use by classes that wrap <see cref="ApplicationDependencyInjection"/> to provide or adapt DI services for non-MEDI consumers, for example, ASP.NET MVC, ASP.NET SignalR, and ASP.NET Web API all have their own separate DI resolver systems.</summary>
	public interface IDependencyInjectionClient : IDisposable
	{
	}
}

namespace AspNetDependencyInjection.Internal
{
	/// <summary>Exposes the (live, not snapshotted) collection of <see cref="IDependencyInjectionClient"/> registrations in <see cref="ApplicationDependencyInjection"/>. This interface is intended for testing purposes (without needing to use <see cref="System.Runtime.CompilerServices.InternalsVisibleToAttribute"/>, which is painful to use with strong-name signed assemblies).</summary>
	public interface IHasDependencyInjectionClients
	{
		/// <summary>Returns the (live, not snapshotted) collection of <see cref="IDependencyInjectionClient"/> registrations.</summary>
		IReadOnlyList<IDependencyInjectionClient> Clients { get; }
	}
}
