using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;

namespace AspNetDependencyInjection
{
	/// <summary>Provides useful extension methods for <see cref="IWebConfiguration"/> implementations.</summary>
	public static class IWebConfigurationExtensions
	{
		/// <summary>Looks up <paramref name="appSettingName"/> in <see cref="IWebConfiguration.AppSettings"/> and throws <see cref="InvalidOperationException"/> if the specified <paramref name="appSettingName"/> does not exist or has an empty value, otherwise it returns it.</summary>
		/// <exception cref="ArgumentNullException">When <paramref name="webConfiguration"/> is null or when <paramref name="appSettingName"/> is null, empty or whitespace.</exception>
		/// <exception cref="InvalidOperationException">When <paramref name="appSettingName"/> cannot be found or is empty.</exception>
		public static String RequireAppSetting( this IWebConfiguration webConfiguration, String appSettingName )
		{
			if( webConfiguration == null ) throw new ArgumentNullException(nameof(webConfiguration));
			if( String.IsNullOrWhiteSpace(appSettingName) ) throw new ArgumentNullException( paramName: nameof(appSettingName) );

			//

			if( webConfiguration.AppSettings.TryGetValue( appSettingName, out String value ) )
			{
				if( String.IsNullOrWhiteSpace( value ) )
				{
					throw new InvalidOperationException( "The <appSettings> entry \"" + appSettingName + "\" is empty." );
				}
				else
				{
					return value;
				}
			}
			else
			{
				throw new InvalidOperationException( "The <appSettings> entry \"" + appSettingName + "\" is undefined." );
			}
		}

		/// <summary>Looks up <paramref name="connectionStringName"/> in <see cref="IWebConfiguration.ConnectionStrings"/> and returns the <see cref="ConnectionStringSettings"/>'s <see cref="ConnectionStringSettings.ConnectionString"/> string value. Otherwise throws an exception.</summary>
		/// <exception cref="ArgumentNullException">When <paramref name="webConfiguration"/> is null or when <paramref name="connectionStringName"/> is null, empty or whitespace.</exception>
		/// <exception cref="InvalidOperationException">When <paramref name="connectionStringName"/> cannot be found or is empty.</exception>
		public static String RequireConnectionString( this IWebConfiguration webConfiguration, String connectionStringName )
		{
			if( webConfiguration == null ) throw new ArgumentNullException(nameof(webConfiguration));
			if( String.IsNullOrWhiteSpace(connectionStringName) ) throw new ArgumentNullException( paramName: nameof(connectionStringName) );

			//

			if( webConfiguration.ConnectionStrings.TryGetValue( connectionStringName, out ConnectionStringSettings cs ) )
			{
				if( cs == null || String.IsNullOrWhiteSpace( cs.ConnectionString ) )
				{
					throw new InvalidOperationException( "The <connectionStrings> entry \"" + connectionStringName + "\" is empty." );
				}
				else
				{
					return cs.ConnectionString;
				}
			}
			else
			{
				throw new InvalidOperationException( "The <connectionStrings> entry \"" + connectionStringName + "\" is undefined." );
			}
		}

		/// <summary>Looks up <paramref name="connectionStringAppSettingName"/> in <see cref="IWebConfiguration.AppSettings"/> and then uses that value to get the connection-string from <see cref="IWebConfiguration.ConnectionStrings"/>.</summary>
		/// <exception cref="ArgumentNullException">When <paramref name="webConfiguration"/> is null or when <paramref name="connectionStringAppSettingName"/> is null, empty or whitespace.</exception>
		/// <exception cref="InvalidOperationException">When <paramref name="connectionStringAppSettingName"/> cannot be found, or when the connection-string it refers to does not exist or is empty.</exception>
		public static String RequireIndirectConnectionString( this IWebConfiguration webConfiguration, String connectionStringAppSettingName )
		{
			if( webConfiguration == null ) throw new ArgumentNullException(nameof(webConfiguration));
			if( String.IsNullOrWhiteSpace(connectionStringAppSettingName) ) throw new ArgumentNullException( paramName: nameof(connectionStringAppSettingName) );

			//

			if( webConfiguration.AppSettings.TryGetValue( connectionStringAppSettingName, out String connectionStringName ) )
			{
				if( String.IsNullOrWhiteSpace( connectionStringName ) )
				{
					throw new InvalidOperationException( "The <appSettings> entry \"" + connectionStringAppSettingName + "\" is empty." );
				}
				else
				{
					return RequireConnectionString( webConfiguration, connectionStringAppSettingName );
				}
			}
			else
			{
				throw new InvalidOperationException( "The <appSettings> entry \"" + connectionStringAppSettingName + "\" does not exist." );
			}
		}

		/// <summary>Attempts to parse the &lt;appSettings&gt; value with the name <paramref name="name"/> as a <see cref="Boolean"/> value by using <see cref="Internal.BooleanUtility.TryParse(string, out bool)"/>. If the value is not defined or cannot be parsed then an <see cref="InvalidOperationException"/> is thrown.</summary>
		public static Boolean RequireBooleanAppSetting( this IWebConfiguration webConfiguration, String name )
		{
			if( webConfiguration == null ) throw new ArgumentNullException(nameof(webConfiguration));

			if( webConfiguration.AppSettings.TryGetValue( name, out String valueStr ) )
			{
				if( Internal.BooleanUtility.TryParse( text: valueStr, out Boolean value ) )
				{
					return value;
				}
				else
				{
					throw new InvalidOperationException( "The <appSettings> entry \"" + name + "\" could not be parsed as a boolean value." );
				}
			}
			else
			{
				throw new InvalidOperationException( "The <appSettings> entry \"" + name + "\" does not exist." );
			}
		}

		/// <summary>Attempts to parse the &lt;appSettings&gt; value with the name <paramref name="name"/> as a <see cref="Int32"/> value by using <see cref="Int32.TryParse(string, NumberStyles, IFormatProvider, out int)"/> (using <see cref="NumberStyles.Integer"/> and <see cref="CultureInfo.InvariantCulture"/>). If the value is not defined or cannot be parsed then an <see cref="InvalidOperationException"/> is thrown.</summary>
		public static Int32 RequireInt32AppSetting( this IWebConfiguration webConfiguration, String name )
		{
			if( webConfiguration == null ) throw new ArgumentNullException(nameof(webConfiguration));

			if( webConfiguration.AppSettings.TryGetValue( name, out String valueStr ) )
			{
				if( Int32.TryParse( valueStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 value ) )
				{
					return value;
				}
				else
				{
					throw new InvalidOperationException( "The <appSettings> entry \"" + name + "\" could not be parsed as a boolean value." );
				}
			}
			else
			{
				throw new InvalidOperationException( "The <appSettings> entry \"" + name + "\" does not exist." );
			}
		}
	}
}

namespace AspNetDependencyInjection.Internal
{
	/// <summary>Contains a boolean parser method.</summary>
	public static class BooleanUtility
	{
		private static readonly HashSet<String> _boolTrueValues = new HashSet<String>( StringComparer.OrdinalIgnoreCase ) { "true", "1", "yes", "on", "T", "Y", "-1" }; // I've seen browsers send `value="on"` for checkbox inputs when a `value=""` wasn't explicitly set. Rss1 also stores boolean values as "Yes" for some reason.

		private static readonly HashSet<String> _booFalseValues = new HashSet<String>( StringComparer.OrdinalIgnoreCase ) { "false", "0", "no", "off", "F", "N" };

		/// <summary>
		/// <para>Attempts to parse <paramref name="text"/> as a boolean value.</para>
		/// <para>If <paramref name="text"/> is equal to any of <c>"true", "1", "yes", "on", "T", "Y", "-1"</c> then <paramref name="value"/> is set to <c>true</c> and this method returns <c>true</c>.</para>
		/// <para>If <paramref name="text"/> is equal to any of <c>"false", "0", "no", "off", "F", "N"</c> then <paramref name="value"/> is set to <c>false</c> and this method returns <c>true</c>.</para>
		/// <para>Otherwise, this method returns <c>false</c> and the value of <paramref name="value"/> is undefined. This method also returns <c>false</c> when <paramref name="text"/> is null, empty, or whitespace.</para>
		/// </summary>
		public static Boolean TryParse( String text, out Boolean value )
		{
			if( String.IsNullOrWhiteSpace( text ) )
			{
				value = default;
				return false;
			}
			else
			{
				if( _boolTrueValues.Contains( text ) )
				{
					value = true;
					return true;
				}
				else if( _booFalseValues.Contains( text ) )
				{
					value = false;
					return true;
				}
				else
				{
					value = default;
					return false;
				}
			}
		}
	}
}