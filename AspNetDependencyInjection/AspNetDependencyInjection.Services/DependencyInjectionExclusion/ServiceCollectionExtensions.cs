using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using AspNetDependencyInjection.Services;

namespace AspNetDependencyInjection
{
	/// <summary>Extension methods for services bundled with <see cref="AspNetDependencyInjection"/>.</summary>
	public static partial class ServiceCollectionExtensions
	{
		/// <summary>Registers <see cref="DefaultDependencyInjectionFallbackService"/> as a singleton implementation of <see cref="IDependencyInjectionFallbackService"/>.</summary>
		public static IServiceCollection AddDefaultAspNetFallbackService( this IServiceCollection services, Boolean excludeAspNetNamespacesFromDI = true, IEnumerable<String> additionalExclusions = null )
		{
			return services
				.AddSingleton<IDependencyInjectionFallbackService>( sp => new DefaultDependencyInjectionFallbackService( excludeAspNetNamespacesFromDI, additionalExclusions ) );
		}

		/// <summary>Registers <see cref="DefaultDependencyInjectionFallbackService"/> as a singleton implementation of <see cref="IDependencyInjectionFallbackService"/> if no existing implementation of <see cref="IDependencyInjectionFallbackService"/> is registered.</summary>
		public static void TryAddDefaultAspNetFallbackService( this IServiceCollection services )
		{
			services
				.TryAddSingleton<IDependencyInjectionFallbackService>( sp => new DefaultDependencyInjectionFallbackService( excludeAspNetNamespacesFromDI: true, additionalExclusions: null ) );
		}
	}
}
