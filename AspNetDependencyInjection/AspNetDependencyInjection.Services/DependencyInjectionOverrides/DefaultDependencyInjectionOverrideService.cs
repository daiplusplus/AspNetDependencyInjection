using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using AspNetDependencyInjection.Configuration;

namespace AspNetDependencyInjection.Services
{
	/// <summary>Uses web.config to determine which types and namespaces should be excluded from DI.</summary>
	public class DefaultDependencyInjectionOverrideService : IDependencyInjectionOverrideService
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

		private static IEnumerable<NamespacePrefix> BuildIgnored( Boolean enabled, IEnumerable<String> values )
		{
			if( enabled )
			{
				return values.Select( e => new NamespacePrefix( e ) );
			}

			return Array.Empty<NamespacePrefix>();
		}

		private readonly IReadOnlyList<NamespacePrefix> ignoreNamespacePrefixes; // For O(1) lookup of known-types.
		private readonly HashSet<String>                ignoreTypeNames; // For slower O(n) lookup, but maybe log-n if I do binary-search? But this is probably unnecessary...

		/// <summary>Constructs a new instance of <see cref="DefaultDependencyInjectionOverrideService"/> and adds <paramref name="additionalExclusions"/> (if any) to the exclusion list.</summary>
		public DefaultDependencyInjectionOverrideService( Boolean useConfigured, Boolean excludeAspNetNamespacesFromDI, IEnumerable<String> additionalExclusions )
		{
			String[] aspnetNamespaces = new[]
			{
				"Microsoft.WebTools.BrowserLink.*", // VS debugging
				"System.ServiceModel.*", // for WCF
				"System.Web.*", // Note this does include System.Web.Mvc
				"!System.Web.Http.**", // We need to return null for ASP.NET Web API services, methinks?
				"WebActivatorEx.*",
				// obviously don't exclude `Microsoft.*` because that would break Microsoft.Extensions.Logging
			};



			IEnumerable<NamespacePrefix> fromCfg    = BuildIgnored( useConfigured, useConfigured ? LoadIgnoredNamespacePrefixes() : Array.Empty<String>() ); // The extra ternary is to prevent eager-entry of `LoadIgnoredNamespacePrefixes` when it's disabled.
			IEnumerable<NamespacePrefix> aspnet     = BuildIgnored( excludeAspNetNamespacesFromDI, aspnetNamespaces );
			IEnumerable<NamespacePrefix> additional = BuildIgnored( enabled: true, additionalExclusions ?? Array.Empty<String>() );

			List<NamespacePrefix> all = Array
				.Empty<NamespacePrefix>()
				.Concat( fromCfg ).Concat( aspnet ).Concat( additional )
				.OrderBy( p => p.Value ) // <-- This is done for *potential* future performance optimization to search by binary-search, if necessary. But having under 10-15 items means linear-search is fine.
				.ToList();


			this.ignoreNamespacePrefixes = all;

			this.ignoreTypeNames = all
				.Where( p => p.Exclude == false && p.IsDefinitelyGlob == false )
				.Select( p => p.Value )
				.ToHashSet( StringComparer.Ordinal );
		}

		/// <summary>See the documentation of <see cref="IDependencyInjectionOverrideService.TryGetServiceProvider(Type, out IServiceProvider)"/>.</summary>
		public virtual Boolean TryGetServiceProvider( Type serviceType, out IServiceProvider serviceProvider )
		{
			if( serviceType is null ) throw new ArgumentNullException(nameof(serviceType));

			String fn = serviceType.FullName;
			if( String.IsNullOrWhiteSpace( fn ) )
			{
				serviceProvider = default; // This should never happen, but y'never know...
				return false;
			}

			//

			if( this.IsIgnored( fn ) )
			{
				serviceProvider = AspNetDependencyInjection.Internal.ActivatorServiceProvider.Instance;
				return true;
			}

			serviceProvider = default;
			return false;
		}

		/// <summary>Not intended for normal use. This method is public only for testing purposes (TODO: Use InternalsVisibleTo?)</summary>
		public Boolean IsIgnored( String typeFullName )
		{
			if( String.IsNullOrWhiteSpace( typeFullName ) ) throw new ArgumentException( message: "Value cannot be null, empty, nor whitespace.", paramName: nameof( typeFullName ) );

			//

			if( this.ignoreTypeNames.Contains( typeFullName ) )
			{
				return true;
			}
			else
			{
				NamespacePrefix np = this.GetLongestMatch( typeFullName );
				if( np != null && !np.Exclude )
				{
					return true;
				}
			}

			return false;
		}

		private NamespacePrefix GetLongestMatch( String fn )
		{
			NamespacePrefix longestMatch = null;
			foreach( NamespacePrefix np in this.ignoreNamespacePrefixes )
			{
				// Quick single-char check:
				if( np.Value[0] < fn[0] )
				{
					continue;
				}
				else if( np.Value[0] > fn[0] )
				{
					// No point continuing.
					return longestMatch;
				}

				//

				if( np.Matches( fn ) )
				{
					if( longestMatch is null )
					{
						longestMatch = np;
					}
					else if( np.Length > longestMatch.Length )
					{
						longestMatch = np;
					}
				}
			}

			return longestMatch;
		}
	}
}
