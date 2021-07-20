using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AspNetDependencyInjection.Configuration
{
	/// <summary>Represents a CLR namespace prefix selector. If a prefix is `MyCompany.MyProduct` then it will match `MyCompany.MyProduct` and `MyCompany.MyProduct.Component` (a child namespace) but not `MyCompany.MyProducts` (as that is a different namespace at the same level).</summary>
	[DebuggerDisplay("Namespace = {Value}, Exclude = {Exclude}")]
	public class NamespacePrefix
	{
		#region Static

		private static readonly Char[] _trimStartChars = new Char[] { '!' };
		private static readonly Char[] _trimEndChars   = new Char[] { '*', '.' };

		internal const String InvalidChars = "!@#$%^&*()+=[{]}\\|;:'\",<>/?~`";

		private static readonly HashSet<Char> _invalidChars = new HashSet<Char>( InvalidChars );

		/// <summary>Verifies that <paramref name="prefix"/> is a syntactically valid .NET namespace prefix name and returns it if it is valid, otherwise it throws an <see cref="ArgumentException"/> if <paramref name="prefix"/> is invalid.</summary>
		/// <param name="prefix">A .NET namespace name. The value may have a Java-style package import suffix (<c>".*"</c>) which will be trimmed away.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="prefix"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="prefix"/> is invalid.</exception>
		public static ( String ns, Boolean exclude, Boolean isGlob ) ValidatePrefix( String prefix )
		{
			if( String.IsNullOrWhiteSpace( prefix ) ) throw new ArgumentNullException(nameof(prefix));

			prefix = prefix.Trim(); // Trim only whitespace first.

			Boolean exclude = prefix[0] == '!';
			Boolean isGlob  = prefix[ prefix.Length - 1 ] == '*';

			prefix = prefix.TrimStart( _trimStartChars );
			prefix = prefix.TrimEnd  ( _trimEndChars ); // e.g. so if `prefix == "MyCompany.MyProduct.*` it becomes `MyCompany.MyProduct`.

			if( String.IsNullOrWhiteSpace( prefix ) ) throw new ArgumentException( message: "Namespace was empty or white-space after trimming.", paramName: nameof(prefix) );

			if( prefix.Any( c => _invalidChars.Contains( c ) || Char.IsWhiteSpace( c ) ) ) throw new ArgumentException( message: "Value contains an invalid character or whitespace.", paramName: nameof(prefix) );
			if( prefix.IndexOf( "..", StringComparison.Ordinal ) > -1 ) throw new ArgumentException( message: "Value contains an empty namespace name.", paramName: nameof(prefix) );

			return ( prefix, exclude, isGlob );
		}

		#endregion

		/// <summary>Constructs a new <see cref="NamespacePrefix"/> object after first validating the provided prefix value. Throws <see cref="ArgumentException"/> if the prefix is syntactically invalid. For more information see the documentation for <see cref="ValidatePrefix(string)"/>.</summary>
		/// <param name="prefix">A .NET namespace name. The value may have a Java-style package import suffix (<c>".*"</c>) which will be trimmed away.</param>
		public NamespacePrefix( String prefix )
		{
			( String ns, Boolean exclude, Boolean isGlob ) = ValidatePrefix( prefix );
			
			this.Length  = prefix.Count( c => c == '.' ) + 1;
			this.Value   = ns;
			this.Exclude = exclude;

			this.IsDefinitelyGlob = isGlob;
		}

		/// <summary>The depth of this namespace prefix. A single name, e.g. <c>System</c> and <c>System.*</c> are 1 (they're equivalent), <c>System.Web</c> is 2, <c>System.Web.Foobar</c> and <c>System.Web.Barbaz.*</c> are both 3, and so on.</summary>
		public Int32 Length { get; }

		/// <summary>Gets the validated prefix input passed into the constructor.</summary>
		public String Value { get; }

		/// <summary>When a configured rule starts with '!' then the rule is negated and the fully-qualified-type-name to test will not be matched.</summary>
		public Boolean Exclude { get; }

		/// <summary>If the source input pattern was a glob (ended in a wildcard). Note that a namespace name will not be a glob even though it is a prefix... so only use this to exclude definitely-globs from exact-type-name matches.</summary>
		public Boolean IsDefinitelyGlob { get; }

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
