using System;
using System.Configuration;

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
				if( String.IsNullOrWhiteSpace(connectionStringName) )
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
	}
}
