using System;

namespace AspNetDependencyInjection.Services
{
	/// <summary>Provides access to the root <see cref="IServiceProvider"/>.</summary>
	public class DefaultServiceProviderAccessor : IServiceProviderAccessor
	{
		/// <summary>Constructs a new instance of <see cref="DefaultServiceProviderAccessor"/>.</summary>
		public DefaultServiceProviderAccessor( ImmutableApplicationDependencyInjectionConfiguration configuration, IServiceProvider rootServiceProvider )
		{
			this.Configuration       = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.RootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException(nameof(rootServiceProvider));
		}

		/// <summary>Exposes <see cref="ApplicationDependencyInjection"/>'s <see cref="ApplicationDependencyInjection.Configuration"/>.</summary>
		public ImmutableApplicationDependencyInjectionConfiguration Configuration { get; }

		/// <summary>The root <see cref="IServiceProvider"/> configured during startup.</summary>
		public IServiceProvider RootServiceProvider { get; }
	}
}
