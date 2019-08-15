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
		/// <summary>Registers <see cref="DefaultDependencyInjectionExclusionService"/> as a singleton implementation of <see cref="IDependencyInjectionExclusionService"/>.</summary>
		public static IServiceCollection AddDefaultAspNetExclusions( this IServiceCollection services, Boolean excludeAspNetNamespacesFromDI = true, IEnumerable<String> additionalExclusions = null )
		{
			return services
				.AddSingleton<IDependencyInjectionExclusionService>( sp => new DefaultDependencyInjectionExclusionService( excludeAspNetNamespacesFromDI, additionalExclusions ) );
		}

		internal static void TryAddDefaultAspNetExclusions( this IServiceCollection services )
		{
			services.TryAddSingleton<IDependencyInjectionExclusionService>( sp => new DefaultDependencyInjectionExclusionService( excludeAspNetNamespacesFromDI: true, additionalExclusions: null ) );
		}
	}
}
