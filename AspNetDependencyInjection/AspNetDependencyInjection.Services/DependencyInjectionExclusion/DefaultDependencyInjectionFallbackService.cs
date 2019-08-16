using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using AspNetDependencyInjection.Configuration;

namespace AspNetDependencyInjection.Services
{
	/// <summary>Uses web.config to determine which types and namespaces should be excluded from DI.</summary>
	public class DefaultDependencyInjectionFallbackService : IDependencyInjectionFallbackService
	{
		private static IEnumerable<String> LoadIgnoredNamespacePrefixes()
		{
			Object configurationSectionObj = ConfigurationManager.GetSection( AspNetDependencyInjectionConfigurationSection.SectionPath );
			if( configurationSectionObj is AspNetDependencyInjectionConfigurationSection configuration )
			{
				return configuration.Prefixes
					.OfType<NamespaceConfigurationElement>()
					.Select( el => el.Prefix );
			}
			else
			{
				return Array.Empty<String>();
			}
		}

		private readonly IReadOnlyList<NamespacePrefix> ignoreNamespacePrefixes;
		private readonly HashSet<String>                ignoreTypeNames;

		/// <summary>Constructs a new instance of <see cref="DefaultDependencyInjectionFallbackService"/> and adds <paramref name="additionalExclusions"/> (if any) to the exclusion list.</summary>
		public DefaultDependencyInjectionFallbackService( Boolean excludeAspNetNamespacesFromDI, IEnumerable<String> additionalExclusions )
		{
			String[] aspnetNamespaces = new[]
			{
				"System.ServiceModel.*", // for WCF
				"System.Web.*",
				// obviously don't exclude `Microsoft.*` because that would break Microsoft.Extensions.Logging
//				"Microsoft.Web.Infrastructure.*",  // excluded because I don't think there are any types in that namespace?
				"WebActivatorEx.*"
			};

			this.ignoreNamespacePrefixes = LoadIgnoredNamespacePrefixes()
				.Concat( excludeAspNetNamespacesFromDI ? aspnetNamespaces : Array.Empty<String>() )
				.Concat( additionalExclusions ?? Array.Empty<String>() )
				.Select( p => new NamespacePrefix( p ) )
				.ToList();

			this.ignoreTypeNames = this.ignoreNamespacePrefixes
				.Select( p => p.Value )
				.Concat( additionalExclusions ?? Array.Empty<String>() )
				.ToHashSet();
		}

		/// <summary>See the documentation of <see cref="IDependencyInjectionFallbackService.TryGetServiceProvider(Type, out IServiceProvider)"/>.</summary>
		public Boolean TryGetServiceProvider( Type type, out IServiceProvider serviceProvider )
		{
			if( type == null ) throw new ArgumentNullException(nameof(type));

			String fn = type.FullName;

			if( this.ignoreTypeNames.Contains( fn ) || this.ignoreNamespacePrefixes.Any( np => np.Matches( fn ) ) )
			{
				serviceProvider = AspNetDependencyInjection.Internal.ActivatorServiceProvider.Instance;
				return true;
			}
			else
			{
				serviceProvider = default;
				return false;
			}
		}
	}
}
