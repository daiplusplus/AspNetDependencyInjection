using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.WebForms.Configuration
{
	/// <summary>Represents a CLR namespace prefix selector. If a prefix is `MyCompany.MyProduct` then it will match `MyCompany.MyProduct` and `MyCompany.MyProduct.Component` (a child namespace) but not `MyCompany.MyProducts` (as that is a different namespace at the same level).</summary>
	public class NamespacePrefix
	{
		#region Static

		private static readonly Char[] _trimChars = new Char[] { '*', '.' };

		internal const String InvalidChars = "!@#$%^&*()+=[{]}\\|;:'\",<>/?~`";

		private static readonly HashSet<Char> _invalidChars = new HashSet<Char>( InvalidChars );

		/// <summary>Verifies that <paramref name="prefix"/> is a syntactically valid .NET namespace prefix name and returns it if it is valid, otherwise it throws an <see cref="ArgumentException"/> if <paramref name="prefix"/> is invalid.</summary>
		/// <param name="prefix">A .NET namespace name. The value may have a Java-style package import suffix (<c>".*"</c>) which will be trimmed away.</param>
		public static String ValidatePrefix( String prefix )
		{
			if( String.IsNullOrWhiteSpace( prefix ) ) throw new ArgumentNullException(nameof(prefix));

			prefix = prefix.TrimEnd( _trimChars ); // e.g. so if `prefix == "MyCompany.MyProduct.*` it becomes `MyCompany.MyProduct`.

			if( String.IsNullOrWhiteSpace( prefix ) ) throw new ArgumentException( message: "Namespace was empty or white-space after trimming.", paramName: nameof(prefix) );

			if( prefix.Any( c => _invalidChars.Contains( c ) || Char.IsWhiteSpace( c ) ) ) throw new ArgumentException( message: "Value contains an invalid character or whitespace.", paramName: nameof(prefix) );

			return prefix;
		}

		#endregion

		/// <summary>Constructs a new <see cref="NamespacePrefix"/> object after first validating the provided prefix value. Throws <see cref="ArgumentException"/> if the prefix is syntactically invalid. For more information see the documentation for <see cref="ValidatePrefix(string)"/>.</summary>
		/// <param name="prefix">A .NET namespace name. The value may have a Java-style package import suffix (<c>".*"</c>) which will be trimmed away.</param>
		public NamespacePrefix( String prefix )
		{
			this.Value = ValidatePrefix( prefix );
		}

		/// <summary>Gets the validated prefix input passed into the constructor.</summary>
		public String Value { get; }

		/// <summary>Returns <c>true</c> if this namespace prefix matches the specified type full name (i.e. the specified type's exists in a namespace that is matched by this prefix. Otherwise returns <c>false</c>.</summary>>
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
