using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AspNetDependencyInjection.Configuration;
using AspNetDependencyInjection.Services;

using Shouldly;

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
			Assert.IsTrue( p.IsDefinitelyGlob );
		}

		[TestMethod]
		public void NamespacePrefix_matches_exact_namespaces()
		{
			NamespacePrefix p = new NamespacePrefix( "System.Web" );

			Assert.IsTrue( p.Matches( "System.Web" ) );
			Assert.IsFalse( p.IsDefinitelyGlob );
		}

		[TestMethod]
		public void NamespacePrefix_does_not_match_sibling_namespace_with_same_textual_prefix()
		{
			NamespacePrefix p = new NamespacePrefix( "System.Web" );

			Assert.IsFalse( p.Matches( "System.WebServer" ) );
			Assert.IsFalse( p.IsDefinitelyGlob );
		}

		[TestMethod]
		public void NamespacePrefix_ValidatePrefix_accepts_and_trims_Java_style_namespace_glob()
		{
			const String input = "System.*";
			( String output, Boolean excl, Boolean isGlob ) = NamespacePrefix.ValidatePrefix( input );

			Assert.AreEqual( expected: "System", actual: output );
			Assert.IsFalse( excl );
			Assert.IsTrue( isGlob );
		}

		[TestMethod]
		public void NamespacePrefix_ValidatePrefix_accepts_exclusion_rules()
		{
			const String input = "!System.*";
			( String output, Boolean excl, Boolean isGlob ) = NamespacePrefix.ValidatePrefix( input );

			Assert.AreEqual( expected: "System", actual: output );
			Assert.IsTrue( excl );
			Assert.IsTrue( isGlob );
		}

		[TestMethod]
		public void NamespacePrefix_ValidatePrefix_rejects_catchall_namespace_glob()
		{
			try
			{
				_ = NamespacePrefix.ValidatePrefix( ".*" );
				Assert.Fail( message: "Expected " + nameof(ArgumentException) + " to be thrown." );
			}
			catch( ArgumentException e ) when ( e.ParamName == "prefix" )
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
					_ = NamespacePrefix.ValidatePrefix( invalidInput );
					Assert.Fail( message: "Expected " + nameof(ArgumentException) + " to be thrown for input \"" + invalidInput + "\"." );
				}
				catch( ArgumentException e ) when ( e.ParamName == "prefix" )
				{
				}
			}
		}

		[TestMethod]
		public void NamespacePrefix_matches_excluded_more_specific_match()
		{
			DefaultDependencyInjectionOverrideService svc = new DefaultDependencyInjectionOverrideService(
				useConfigured                : false,
				excludeAspNetNamespacesFromDI: false,
				additionalExclusions         : new String[]
				{
					"System.Foobar",
					"!System.Foobar.Barbaz",
					"System.Foobar.Barbaz.SantaBarbara",
					"!System.Foobar.Barbaz.SantaBarbara.SantaCruz",
				}
			);

			svc.IsIgnored( "Foo" ).ShouldBeFalse();
			svc.IsIgnored( "System" ).ShouldBeFalse();
			svc.IsIgnored( "System.Windows" ).ShouldBeFalse();
			svc.IsIgnored( "System.Windows.Forms" ).ShouldBeFalse();
			svc.IsIgnored( "System.Windows.Forms.Buttons" ).ShouldBeFalse();

			svc.IsIgnored( "System.Foobar" ).ShouldBeTrue();
			svc.IsIgnored( "System.Foobar.Barbaz" ).ShouldBeFalse();
			svc.IsIgnored( "System.Foobar.Barbaz.a" ).ShouldBeFalse();
			svc.IsIgnored( "System.Foobar.Barbaz.SantaBarbara" ).ShouldBeTrue();
			svc.IsIgnored( "System.Foobar.Barbaz.SantaBarbara.a" ).ShouldBeTrue();
			svc.IsIgnored( "System.Foobar.Barbaz.SantaBarbara.SantaCruz" ).ShouldBeFalse();
			svc.IsIgnored( "System.Foobar.Barbaz.SantaBarbara.SantaCruz.a" ).ShouldBeFalse();

			svc.IsIgnored( "System.Foobar.Barbaz1" ).ShouldBeTrue();
		}

		[TestMethod]
		public void NamespacePrefix_rejects_excluded_more_specific_match()
		{
			DefaultDependencyInjectionOverrideService svc = new DefaultDependencyInjectionOverrideService(
				useConfigured                : false,
				excludeAspNetNamespacesFromDI: false,
				additionalExclusions         : new String[]
				{
					"System",
					"System.Foobar",
					"!System.Foobar.Barbaz",
				}
			);

			svc.IsIgnored( "Foo" ).ShouldBeFalse();
			svc.IsIgnored( "System" ).ShouldBeTrue(); // Because of "System"
			svc.IsIgnored( "System.Windows" ).ShouldBeTrue(); // Because of "System"
			svc.IsIgnored( "System.Windows.Forms" ).ShouldBeTrue(); // Because of "System"
			svc.IsIgnored( "System.Windows.Forms.Buttons" ).ShouldBeTrue(); // Because of "System"

			svc.IsIgnored( "System.Foobar" ).ShouldBeTrue();
			svc.IsIgnored( "System.Foobar.Barbaz" ).ShouldBeFalse();
			svc.IsIgnored( "System.Foobar.Barbaz.Nested" ).ShouldBeFalse();
			svc.IsIgnored( "System.Foobar.Barbaz.Nested2" ).ShouldBeFalse();

			svc.IsIgnored( "System.Foobar.Barbaz1" ).ShouldBeTrue();
		}
	}
}
