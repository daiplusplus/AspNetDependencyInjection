using System;

namespace AspNetDependencyInjection.Services
{
	/// <summary>Provides access to the root <see cref="IServiceProvider"/>.</summary>
	public class DefaultServiceProviderAccessor : IServiceProviderAccessor
	{
		/// <summary>Constructs a new instance of <see cref="DefaultServiceProviderAccessor"/>.</summary>
		public DefaultServiceProviderAccessor( ImmutableApplicationDependencyInjectionConfiguration configuration, IServiceProvider serviceProvider )
		{
			this.Configuration       = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.RootServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		/// <summary>Exposes <see cref="ApplicationDependencyInjection"/>'s <see cref="ApplicationDependencyInjection.Configuration"/>.</summary>
		public ImmutableApplicationDependencyInjectionConfiguration Configuration { get; }

		/// <summary>The root <see cref="IServiceProvider"/> configured during startup.</summary>
		public IServiceProvider RootServiceProvider { get; }
	}
}
