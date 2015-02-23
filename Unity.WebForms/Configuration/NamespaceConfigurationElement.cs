using System.Configuration;

namespace Unity.WebForms.Configuration
{
	/// <summary>
	///		Defines an individual namespace element for exclusion when 
	///		performing dependency injection.
	/// </summary>
	public class NamespaceConfigurationElement : ConfigurationElement
	{
		private const string PrefixKey = "prefix";

		/// <summary>
		///		Initializes a new, empty, instance.
		/// </summary>
		public NamespaceConfigurationElement()
		{
		}

		/// <summary>
		///		Initializes a new instance with the supplied values.
		/// </summary>
		/// <param name="prefix"></param>
		public NamespaceConfigurationElement( string prefix )
		{
			Prefix = prefix;
		}

		#region Properties

		/// <summary>
		///		The namespace prefix to exclude from injection scanning.
		/// </summary>
		[ConfigurationProperty( PrefixKey, DefaultValue = "System", IsKey = true, IsRequired = true )]
		[StringValidator(MinLength = 3, InvalidCharacters = "!@#$%^&*()+=[{]}\\|;:'\",<>/?~`")]
		public string Prefix
		{
			get { return (string)this[PrefixKey]; }
			set { this[PrefixKey] = value; }
		}

		#endregion
	}
}
