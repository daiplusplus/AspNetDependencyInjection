using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.WebForms.Configuration
{
	/// <summary>Represents a CLR namespace prefix selector. If a prefix is `MyCompany.MyProduct` then it will match `MyCompany.MyProduct` and `MyCompany.MyProduct.Component` (a child namespace) but not `MyCompany.MyProducts` (as that is a different namespace at the same level).</summary>
	public class NamespacePrefix
	{
		private static readonly Char[] _trimChars = new Char[] { '*', '.' };

		internal const String InvalidChars = "!@#$%^&*()+=[{]}\\|;:'\",<>/?~`";

		private static readonly HashSet<Char> _invalidChars = new HashSet<Char>( InvalidChars );

		public static String ValidatePrefix( String prefix )
		{
			if( String.IsNullOrWhiteSpace( prefix ) ) throw new ArgumentNullException(nameof(prefix));

			prefix = prefix.TrimEnd( _trimChars ); // e.g. so if `prefix == "MyCompany.MyProduct.*` it becomes `MyCompany.MyProduct`.

			if( String.IsNullOrWhiteSpace( prefix ) ) throw new ArgumentException( message: "Namespace was empty or white-space after trimming.", paramName: nameof(prefix) );

			if( prefix.Any( c => _invalidChars.Contains( c ) || Char.IsWhiteSpace( c ) ) ) throw new ArgumentException( message: "Value contains an invalid character or whitespace.", paramName: nameof(prefix) );

			return prefix;
		}

		public NamespacePrefix( String prefix )
		{
			this.Value = ValidatePrefix( prefix );
		}

		public String Value { get; }

		public Boolean Matches( String typeName )
		{
			if( String.IsNullOrWhiteSpace( typeName ) ) throw new ArgumentNullException(nameof(typeName));

			if( typeName.StartsWith( this.Value, StringComparison.Ordinal ) )
			{
				if( typeName.Length == this.Value.Length ) return true;

				Char expectedDot = typeName[ this.Value.Length ];
				return expectedDot == '.';
			}
			else
			{
				return false;
			}
		}
	}
}
