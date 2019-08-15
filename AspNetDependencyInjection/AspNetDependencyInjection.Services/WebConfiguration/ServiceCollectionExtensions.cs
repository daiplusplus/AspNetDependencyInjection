
using AspNetDependencyInjection.Services;

using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection
{
	/// <summary>Extension methods for services bundled with <see cref="AspNetDependencyInjection"/>.</summary>
	public static partial class ServiceCollectionExtensions
	{
		/// <summary>Registers <see cref="DefaultWebConfiguration"/> as a singleton implementation of <see cref="IWebConfiguration"/>.</summary>
		public static IServiceCollection AddWebConfiguration( this IServiceCollection services )
		{
			return services
				.AddSingleton<IWebConfiguration,DefaultWebConfiguration>();
		}
	}
}
