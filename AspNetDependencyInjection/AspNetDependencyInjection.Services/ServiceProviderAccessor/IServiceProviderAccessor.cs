using System;

namespace AspNetDependencyInjection
{
	/// <summary>Allows objects to have a dependency on the root <see cref="IServiceProvider"/> in the application.</summary>
	public interface IServiceProviderAccessor
	{
		/// <summary>Immutable copy of the configuration passed into <see cref="ApplicationDependencyInjection"/>.</summary>
		ImmutableApplicationDependencyInjectionConfiguration Configuration { get; }

		/// <summary>Gets the root <see cref="IServiceProvider"/> in this AppDomain. It is managed by <see cref="ApplicationDependencyInjection"/>.</summary>
		IServiceProvider RootServiceProvider { get; }
	}
}

