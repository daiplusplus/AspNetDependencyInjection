using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web.Configuration;

namespace Unity.WebForms.Services
{
	/// <summary>Implementation of <see cref="IWebConfiguration"/> that populates itself using <see cref="WebConfigurationManager"/>'s static properties. The dictionaries have case-insensitive keys.</summary>
	public class DefaultWebConfiguration : IWebConfiguration
	{
		private static Dictionary<String,String> ToDictionary( NameValueCollection nvc )
		{
			Dictionary<String,String> dict = new Dictionary<String,String>( capacity: nvc.Count, comparer: StringComparer.OrdinalIgnoreCase );

			foreach( String key in nvc.Keys )
			{
				String firstValue = nvc.GetValues( key ).First();
				dict[ key ] = firstValue;
			}

			return dict;
		}

		/// <summary>Constructs a new instance of <see cref="DefaultWebConfiguration"/> that populates itself using <see cref="WebConfigurationManager"/>'s static properties.</summary>
		public DefaultWebConfiguration()
		{
			this.AppSettings = ToDictionary( WebConfigurationManager.AppSettings );

			this.ConnectionStrings = WebConfigurationManager.ConnectionStrings
				.OfType<ConnectionStringSettings>()
				.ToDictionary( cs => cs.Name, comparer: StringComparer.OrdinalIgnoreCase );
		}

		/// <summary>Provides read-only access to <see cref="System.Web.Configuration.WebConfigurationManager.AppSettings"/>. The dictionary uses a case-insensitive key comparer.</summary>
		public IReadOnlyDictionary<String, String> AppSettings { get; }

		/// <summary>Provides read-only access to <see cref="System.Web.Configuration.WebConfigurationManager.ConnectionStrings"/>.  The dictionary uses a case-insensitive key comparer.</summary>
		public IReadOnlyDictionary<String, ConnectionStringSettings> ConnectionStrings { get; }

		/// <summary>Returns an <see cref="IReadOnlyDictionary{String,String}"/> representation of a .NET configuration section provided by either <see cref="DictionarySectionHandler"/>, <see cref="NameValueSectionHandler"/>, <see cref="NameValueFileSectionHandler"/> or <see cref="SingleTagSectionHandler"/> (or any configuration section that is resolvable to <see cref="System.Collections.IDictionary"/> or <see cref="NameValueCollection"/>).  Returns <c>null</c> if the section does not exist.</summary>
		/// <remarks>Unlike the <see cref="AppSettings"/> and <see cref="ConnectionStrings"/> properties of <see cref="DefaultWebConfiguration"/> values are not eagerly-loaded inside <see cref="DefaultWebConfiguration"/>'s constructor.</remarks>
		public IReadOnlyDictionary<String,String> GetKeyValueSection( String sectionName )
		{
			if( String.IsNullOrWhiteSpace(sectionName) ) throw new ArgumentNullException(nameof(sectionName));

			//

			Object sectionObj = WebConfigurationManager.GetSection( sectionName );
			if( sectionObj is null ) return null;

			if( sectionObj is IReadOnlyDictionary<String,String> rodict )
			{
				return rodict;
			}
			else if( sectionObj is System.Collections.Hashtable hashtable )
			{
				return hashtable.Keys
					.Cast<Object>()
					.Select( keyObj => ( key: keyObj as String, value: hashtable[keyObj] as String ) )
					.Where( t => t.key != null )
					.ToDictionary( t => t.key, t => t.value, comparer: StringComparer.OrdinalIgnoreCase );

			}
			else if( sectionObj is System.Collections.IDictionary dict )
			{
				return dict.Keys
					.Cast<Object>()
					.Select( keyObj => ( key: keyObj as String, value: dict[keyObj] as String ) )
					.Where( t => t.key != null )
					.ToDictionary( t => t.key, t => t.value, comparer: StringComparer.OrdinalIgnoreCase );
			}
			else if( sectionObj is NameValueCollection nvc )
			{
				return ToDictionary( nvc );
			}
			else
			{
				throw new InvalidOperationException( "The configuration section named \"" + sectionName + "\" is of type " + sectionObj.GetType().FullName + " and cannot be converted to an " + nameof(IReadOnlyDictionary<String,String>) + "." );
			}
		}
	}
}
