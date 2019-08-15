using System;

namespace AspNetDependencyInjection.Services
{
	/// <summary>Provides access to the root <see cref="IServiceProvider"/>.</summary>
	public class DefaultServiceProviderAccessor : IServiceProviderAccessor
	{
		/// <summary>Constructs a new instance of <see cref="DefaultServiceProviderAccessor"/>.</summary>
		public DefaultServiceProviderAccessor( IServiceProvider serviceProvider )
		{
			this.RootServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		/// <summary>The root <see cref="IServiceProvider"/> configured during startup.</summary>
		public IServiceProvider RootServiceProvider { get; }
	}
}
