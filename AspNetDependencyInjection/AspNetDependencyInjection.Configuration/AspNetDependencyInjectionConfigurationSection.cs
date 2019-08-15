using System;
using System.Configuration;

namespace AspNetDependencyInjection.Configuration
{
	/// <summary>Defines the AspNetDependencyInjection configuration settings node inside an application configuration file.</summary>
	public class AspNetDependencyInjectionConfigurationSection : ConfigurationSection
	{
		/// <summary>Key for the configuration collection root element.</summary>
		private const String IgnoreNamespaceKeys = "ignoreNamspaces";

		/// <summary>The name of the Section and configuration root node that contains the settings.</summary>
		public const String SectionPath = "AspNetDependencyInjection";

		/// <summary>Collection of namespace prefixes to ignore for dependency injection.</summary>
		[ConfigurationProperty(IgnoreNamespaceKeys, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(NamespaceConfigurationElement), AddItemName = "namespace")]
		public IgnoreNamespaceConfigurationElementCollection Prefixes
		{
			get { return (IgnoreNamespaceConfigurationElementCollection)base[IgnoreNamespaceKeys]; }
		}
	}
}
