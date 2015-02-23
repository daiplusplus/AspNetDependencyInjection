using System.Configuration;

namespace Unity.WebForms.Configuration
{
	/// <summary>
	///		Defines the Unity.WebForms configuration settings node inside an
	///		application configuration file.
	/// </summary>
	public class UnityWebFormsConfiguration : ConfigurationSection
	{
		#region Keys

		/// <summary>Key for the configuration collection root element.</summary>
		private const string IgnoreNamespaceKeys = "ignoreNamspaces";

		#endregion

		#region Properties

		/// <summary>
		///		The name of the Section and configuration root node that contains
		///		the settings.
		/// </summary>
		public const string SectionPath = "Unity.WebForms";

		/// <summary>
		///		Collection of namespace prefixes to ignore for dependency injection.
		/// </summary>
		[ConfigurationProperty(IgnoreNamespaceKeys, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(NamespaceConfigurationElement), AddItemName = "namespace")]
		public IgnoreNamespaceConfigurationCollection Prefixes
		{
			get { return (IgnoreNamespaceConfigurationCollection)base[IgnoreNamespaceKeys]; }
		}

		#endregion
	}
}
