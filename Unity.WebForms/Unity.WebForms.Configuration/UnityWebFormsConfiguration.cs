using System;
using System.Configuration;

namespace Unity.WebForms.Configuration
{
	/// <summary>Defines the Unity.WebForms configuration settings node inside an application configuration file.</summary>
	public class UnityWebFormsConfiguration : ConfigurationSection
	{
		/// <summary>Key for the configuration collection root element.</summary>
		private const String IgnoreNamespaceKeys = "ignoreNamspaces";

		/// <summary>The name of the Section and configuration root node that contains the settings.</summary>
		public const String SectionPath = "Unity.WebForms";

		/// <summary>Collection of namespace prefixes to ignore for dependency injection.</summary>
		[ConfigurationProperty(IgnoreNamespaceKeys, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(NamespaceConfigurationElement), AddItemName = "namespace")]
		public IgnoreNamespaceConfigurationCollection Prefixes
		{
			get { return (IgnoreNamespaceConfigurationCollection)base[IgnoreNamespaceKeys]; }
		}
	}
}
