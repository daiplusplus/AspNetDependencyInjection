using System;

namespace AspNetDependencyInjection.Services
{
	/// <summary>Provides access to the root <see cref="IServiceProvider"/>.</summary>
	public class DefaultServiceProviderAccessor : IServiceProviderAccessor
	{
		/// <summary>Constructs a new instance of <see cref="DefaultServiceProviderAccessor"/>.</summary>
		public DefaultServiceProviderAccessor( ApplicationDependencyInjection applicationDependencyInjection, IServiceProvider serviceProvider )
		{
			this.ApplicationDI       = applicationDependencyInjection ?? throw new ArgumentNullException(nameof(applicationDependencyInjection));
			this.RootServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		/// <summary>Exposes <see cref="ApplicationDependencyInjection"/>.</summary>
		public ApplicationDependencyInjection ApplicationDI { get; }

		/// <summary>The root <see cref="IServiceProvider"/> configured during startup.</summary>
		public IServiceProvider RootServiceProvider { get; }
	}
}
