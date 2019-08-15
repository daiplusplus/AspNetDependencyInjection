using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Unity.WebForms.Configuration;

namespace Unity.WebForms.Services
{
	/// <summary>Uses web.config to determine which types and namespaces should be excluded from DI.</summary>
	public class DefaultAspNetDIExclusionService : IAspNetDIExclusionService
	{
		private static IEnumerable<String> LoadIgnoredNamespacePrefixes()
		{
			Object configurationSectionObj = ConfigurationManager.GetSection( UnityWebFormsConfigurationSection.SectionPath );
			if( configurationSectionObj is UnityWebFormsConfigurationSection configuration )
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

		/// <summary>Constructs a new instance of <see cref="DefaultAspNetDIExclusionService"/> and adds <paramref name="additionalExclusions"/> (if any) to the exclusion list.</summary>
		public DefaultAspNetDIExclusionService( Boolean excludeAspNetNamespacesFromDI, IEnumerable<String> additionalExclusions )
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
				.Concat( excludeAspNetNamespacesFromDI ? aspnetNamespaces : Enumerable.Empty<String>() )
				.Concat( additionalExclusions ?? Enumerable.Empty<String>() )
				.Select( p => new NamespacePrefix( p ) )
				.ToList();

			this.ignoreTypeNames = this.ignoreNamespacePrefixes
				.Select( p => p.Value )
				.Concat( additionalExclusions ?? Array.Empty<String>() )
				.ToHashSet();
		}

		/// <summary>Returns <c>true</c> if the specified <paramref name="type"/> should be created using <see cref="Activator"/> (ASP.NET's default object factory) instead of using the configured <see cref="IServiceProvider"/>. The <see cref="AspNetDIExclusionServiceExtensions.IsIncluded(IAspNetDIExclusionService, Type)"/> extension method calls this method and returns the inverse (logical NOT) of this method's return value.</summary>
		public Boolean IsExcluded( Type type )
		{
			if( type == null ) throw new ArgumentNullException(nameof(type));

			String fn = type.FullName;

			return this.ignoreTypeNames.Contains( fn ) || this.ignoreNamespacePrefixes.Any( np => np.Matches( fn ) );
		}
	}

	/// <summary>Extension methods for services bundled with <see cref="Unity.WebForms"/>.</summary>
	public static partial class WebFormsServiceExtensions
	{
		/// <summary>Registers <see cref="DefaultAspNetDIExclusionService"/> as a singleton implementation of <see cref="IAspNetDIExclusionService"/>.</summary>
		public static IServiceCollection AddDefaultAspNetDIExclusions( this IServiceCollection services, Boolean excludeAspNetNamespacesFromDI = true, IEnumerable<String> additionalExclusions = null )
		{
			return services
				.AddSingleton<IAspNetDIExclusionService>( sp => new DefaultAspNetDIExclusionService( excludeAspNetNamespacesFromDI, additionalExclusions ) );
		}

		internal static void TryAddDefaultAspNetDIExclusions( this IServiceCollection services )
		{
			services.TryAddSingleton<IAspNetDIExclusionService>( sp => new DefaultAspNetDIExclusionService( excludeAspNetNamespacesFromDI: true, additionalExclusions: null ) );
		}
	}
}
