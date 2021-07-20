using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using AspNetDependencyInjection.Services;

namespace AspNetDependencyInjection
{
	/// <summary>Extension methods for services bundled with <see cref="AspNetDependencyInjection"/>.</summary>
	[CLSCompliant(false)] // IServiceCollection is not CLS-compliant.
	public static partial class ServiceCollectionExtensions
	{
		/// <summary>Registers <see cref="DefaultDependencyInjectionOverrideService"/> as a singleton implementation of <see cref="IDependencyInjectionOverrideService"/>.</summary>
		public static IServiceCollection AddDefaultDependencyInjectionOverrideService( this IServiceCollection services, Boolean excludeAspNetNamespacesFromDI = true, IEnumerable<String> additionalExclusions = null )
		{
			return services
				.AddSingleton<IDependencyInjectionOverrideService>( sp => new DefaultDependencyInjectionOverrideService( useConfigured: true, excludeAspNetNamespacesFromDI, additionalExclusions ) );
		}

		/// <summary>Registers <see cref="DefaultDependencyInjectionOverrideService"/> as a singleton implementation of <see cref="IDependencyInjectionOverrideService"/> if no existing implementation of <see cref="IDependencyInjectionOverrideService"/> is registered.</summary>
		public static void TryAddDefaultDependencyInjectionOverrideService( this IServiceCollection services )
		{
			services
				.TryAddSingleton<IDependencyInjectionOverrideService>( sp => new DefaultDependencyInjectionOverrideService( useConfigured: true, excludeAspNetNamespacesFromDI: true, additionalExclusions: null ) );
		}
	}
}
