using System;
using System.Collections.Generic;
using System.Configuration;

namespace Unity.WebForms.Services
{
	/// <summary>Implements <see cref="IWebConfiguration"/> using values passed to its constructor. Intended for testing purposes. For production use in an ASP.NET environment, use <see cref="DefaultWebConfiguration"/>.</summary>
	public class PassthroughWebConfiguration : IWebConfiguration
	{
		private readonly IReadOnlyDictionary<String,IReadOnlyDictionary<String,String>> otherSections;

		/// <summary>Constructs a <see cref="PassthroughWebConfiguration"/> instance using provided values only. Intended for testing purposes.</summary>
		/// <param name="appSettings">Required. Cannot be null otherwise <see cref="ArgumentNullException"/> is thrown.</param>
		/// <param name="connectionStrings">Required. Cannot be null otherwise <see cref="ArgumentNullException"/> is thrown.</param>
		/// <param name="otherSections">Optional. Member dictionaries are returned by <see cref="GetKeyValueSection(string)"/>.</param>
		public PassthroughWebConfiguration( IReadOnlyDictionary<String, String> appSettings, IReadOnlyDictionary<String, ConnectionStringSettings> connectionStrings, IReadOnlyDictionary<String,IReadOnlyDictionary<String,String>> otherSections = null )
		{
			// TOOD: Is there a way to check the type of comparer an IReadOnlyDictionary is using to ensure it's case-insensitive?

			this.AppSettings       = appSettings       ?? throw new ArgumentNullException(nameof(appSettings));
			this.ConnectionStrings = connectionStrings ?? throw new ArgumentNullException(nameof(connectionStrings));
			this.otherSections     = otherSections;
		}

		/// <summary>The dictionary's key comparer is undefined and may not be case-insensitive.</summary>
		public IReadOnlyDictionary<String, String> AppSettings { get; }

		/// <summary>The dictionary's key comparer is undefined and may not be case-insensitive.</summary>
		public IReadOnlyDictionary<String, ConnectionStringSettings> ConnectionStrings { get; }

		/// <summary>Gets a dictionary from the <c>otherSections</c> dictionary passed into this object's constructor. Returns <c>null</c> if the section doesn't exist.</summary>
		public IReadOnlyDictionary<String,String> GetKeyValueSection( String name )
		{
			if( this.otherSections == null ) return null;
			
			return this.otherSections.TryGetValue( name, out IReadOnlyDictionary<String,String> section ) ? section : null;
		}
	}
}
