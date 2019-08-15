using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AspNetDependencyInjection.Configuration;

namespace AspNetDependencyInjection.Tests
{
	[TestClass]
	public class ConfigurationTests
	{
		[TestMethod]
		public void NamespacePrefix_matches_child_namespaces()
		{
			NamespacePrefix p = new NamespacePrefix( "System.Web.*" );

			Assert.IsTrue( p.Matches( "System.Web.UI.Page" ) );
		}

		[TestMethod]
		public void NamespacePrefix_matches_exact_namespaces()
		{
			NamespacePrefix p = new NamespacePrefix( "System.Web" );

			Assert.IsTrue( p.Matches( "System.Web" ) );
		}

		[TestMethod]
		public void NamespacePrefix_does_not_match_sibling_namespace_with_same_textual_prefix()
		{
			NamespacePrefix p = new NamespacePrefix( "System.Web" );

			Assert.IsFalse( p.Matches( "System.WebServer" ) );
		}

		[TestMethod]
		public void NamespacePrefix_ValidatePrefix_accepts_and_trims_Java_style_namespace_glob()
		{
			const String input = "System.*";
			String output = NamespacePrefix.ValidatePrefix( input );

			Assert.AreEqual( expected: "System", actual: output );
		}

		[TestMethod]
		public void NamespacePrefix_ValidatePrefix_rejects_catchall_namespace_glob()
		{
			try
			{
				NamespacePrefix.ValidatePrefix( ".*" );
				Assert.Fail( message: "Expected " + nameof(ArgumentException) + " to be thrown." );
			}
			catch( ArgumentException )
			{
			}
		}

		[TestMethod]
		public void NamespacePrefix_ValidatePrefix_rejects_invalid_input()
		{
			String[] invalidInputs = new[]
			{
				null,
				"",
				"    ",
				"foo bar",
				"System.Tuple`2<T1,T2>",
				"global::System.Windows.Forms"
			};

			foreach( String invalidInput in invalidInputs )
			{
				try
				{
					NamespacePrefix.ValidatePrefix( invalidInput );
					Assert.Fail( message: "Expected " + nameof(ArgumentException) + " to be thrown for input \"" + invalidInput + "\"." );
				}
				catch( ArgumentException )
				{
				}
			}
		}
	}
}
