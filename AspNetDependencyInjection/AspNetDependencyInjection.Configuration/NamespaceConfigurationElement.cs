using System;
using System.Configuration;

namespace AspNetDependencyInjection.Configuration
{
	/// <summary>Defines an individual namespace element for exclusion when performing dependency injection.</summary>
	public class NamespaceConfigurationElement : ConfigurationElement
	{
		private const String PrefixKey  = "prefix";

		/// <summary>Initializes a new, empty, instance.</summary>
		public NamespaceConfigurationElement()
		{
		}

		/// <summary>Initializes a new instance with the supplied values.</summary>
		/// <param name="prefix">The namespace prefix to exclude from dependency injection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="prefix"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">When <paramref name="prefix"/> is not a valid namespace prefix.</exception>
		public NamespaceConfigurationElement( String prefix )
		{
			_ = NamespacePrefix.ValidatePrefix( prefix ); // we don't care about the extracted result: we only care if it throws or not.

			this.Prefix = prefix;
		}

		/// <summary>The namespace prefix to exclude from injection scanning. This may include a leading ! and trailing .*.</summary>
		/// <exception cref="ArgumentException">When the provided value is not a valid namespace prefix.</exception>
		[ConfigurationProperty( name: PrefixKey, DefaultValue = "System", IsKey = true, IsRequired = true )]
		[StringValidator( MinLength = 3, InvalidCharacters = NamespacePrefix.InvalidChars )]
		public String Prefix
		{
			get { return (String)this[PrefixKey]; }
			set
			{
				_ = NamespacePrefix.ValidatePrefix( value ); // As with the ctor, provided that `ValidatePrefix` doesn't throw then it's okay.
				this[PrefixKey] = value;
			}
		}
	}
}
