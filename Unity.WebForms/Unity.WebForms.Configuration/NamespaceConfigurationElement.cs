using System;
using System.Configuration;

namespace AspNetDependencyInjection.Configuration
{
	/// <summary>Defines an individual namespace element for exclusion when performing dependency injection.</summary>
	public class NamespaceConfigurationElement : ConfigurationElement
	{
		private const String PrefixKey = "prefix";

		/// <summary>Initializes a new, empty, instance.</summary>
		public NamespaceConfigurationElement()
		{
		}

		/// <summary>Initializes a new instance with the supplied values.</summary>
		/// <param name="prefix">The namespace prefix to exclude from dependency injection.</param>
		/// <exception cref="ArgumentException">When <paramref name="prefix"/> is not a valid namespace prefix.</exception>
		public NamespaceConfigurationElement( String prefix )
		{
			this.Prefix = NamespacePrefix.ValidatePrefix( prefix );
		}

		/// <summary>The namespace prefix to exclude from injection scanning.</summary>
		/// <exception cref="ArgumentException">When the provided value is not a valid namespace prefix.</exception>
		[ConfigurationProperty( PrefixKey, DefaultValue = "System", IsKey = true, IsRequired = true )]
		[StringValidator( MinLength = 3, InvalidCharacters = NamespacePrefix.InvalidChars )]
		public String Prefix
		{
			get { return (String)this[PrefixKey]; }
			set { this[PrefixKey] = NamespacePrefix.ValidatePrefix( value ); }
		}
	}
}
