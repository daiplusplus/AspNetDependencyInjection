using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Web.Configuration;

namespace Unity.WebForms.Services
{
	/// <summary>Provides an injectable representation of <see cref="WebConfigurationManager"/> that can be mocked for testing.</summary>
	public interface IWebConfiguration
	{
		/// <summary>Provides read-only access to <see cref="WebConfigurationManager.AppSettings"/>. The dictionary uses a case-insensitive key comparer.</summary>
		/// <remarks>WebConfigurationManager returns a <see cref="NameValueCollection"/> which allows multiple values for a key, whereas the &lt;appSettings&gt; element does not allow duplicate keys - hence why this property returns a <see cref="IReadOnlyDictionary{String,String}"/>.</remarks>
		IReadOnlyDictionary<String,String> AppSettings { get; }

		/// <summary>Provides read-only access to <see cref="WebConfigurationManager.ConnectionStrings"/>. The dictionary uses a case-insensitive key comparer.</summary>
		IReadOnlyDictionary<String,ConnectionStringSettings> ConnectionStrings { get; }

		/// <summary>Returns an <see cref="IReadOnlyDictionary{String,String}"/> representation of a .NET configuration section provided by either <see cref="DictionarySectionHandler"/>, <see cref="NameValueSectionHandler"/>, <see cref="NameValueFileSectionHandler"/> or <see cref="SingleTagSectionHandler"/> (or any configuration section that is resolvable to <see cref="System.Collections.IDictionary"/> or <see cref="NameValueCollection"/>). Returns <c>null</c> if the section does not exist.</summary>
		/// <exception cref="InvalidOperationException">Thrown when the <paramref name="sectionName"/> refers to a section that cannot be converted to an <see cref="IReadOnlyDictionary{String,String}"/>.</exception>
		IReadOnlyDictionary<String,String> GetKeyValueSection( String sectionName );
	}

	/// <summary>Extension methods for services bundled with <see cref="Unity.WebForms"/>.</summary>
	public static partial class UnityContainerAddServiceExtensions
	{
		/// <summary>Registers <see cref="DefaultWebConfiguration"/> as a singleton implementation of <see cref="IWebConfiguration"/>.</summary>
		public static IUnityContainer AddWebConfiguration( this IUnityContainer container )
		{
			return container
				.RegisterSingleton<IWebConfiguration,DefaultWebConfiguration>();
		}
	}
}
