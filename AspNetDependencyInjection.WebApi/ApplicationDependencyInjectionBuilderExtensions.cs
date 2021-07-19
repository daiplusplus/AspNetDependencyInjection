using System;
using System.Reflection;
using System.Web.Http;

using AspNetDependencyInjection.Internal;

namespace AspNetDependencyInjection
{
	/// <summary>Extension methods for <see cref="ApplicationDependencyInjectionBuilder"/>.</summary>
	public static class ApplicationDependencyInjectionBuilderExtensions
	{
		/// <summary>Use this method overload to set-up AspNetDependencyInjection for ASP.NET-hosted Web API application. The current <see cref="HttpConfiguration"/> will be obtained from <c>System.Web.Http.GlobalConfiguration</c> using reflection.</summary>
		/// <param name="builder">Cannot be <see langword="null"/>.</param>
		/// <remarks>
		/// Reflection is used to access <c>System.Web.Http.GlobalConfiguration</c> because the type exists in <c>System.Web.Http.WebHost.dll</c> which is not available in non-ASP.NET-hosted Web API projects, and I didn't want to add another NuGet dependency to <c>AspNetDependencyInjection.WebApi</c> just for that one static property.<br />
		/// Internally, this method uses <see cref="TryGetGlobalHttpConfiguration"/> to get <see cref="HttpConfiguration"/> from  <c>System.Web.Http.GlobalConfiguration</c> - if it fails then it throws <see cref="InvalidOperationException"/>, and adds <see cref="DependencyInjectionWebApiDependencyResolver"/> to <paramref name="builder"/> using <see cref="ApplicationDependencyInjectionBuilder.AddClient(Func{ApplicationDependencyInjection, IDependencyInjectionClient}[])"/>.</remarks>
		/// <exception cref="InvalidOperationException">Thrown when <see cref="TryGetGlobalHttpConfiguration(out HttpConfiguration, out String)"/> fails.</exception>
		public static ApplicationDependencyInjectionBuilder AddWebApiDependencyResolver( this ApplicationDependencyInjectionBuilder builder )
		{
			if( builder is null ) throw new ArgumentNullException(nameof(builder));

			if( TryGetGlobalHttpConfiguration( out HttpConfiguration globalConfiguration, out String errorMessage ) )
			{
				return builder
					.AddClient( ( di, rootSP ) => new DependencyInjectionWebApiDependencyResolver( di, rootSP, globalConfiguration ) );
			}
			else
			{
				throw new InvalidOperationException( errorMessage );
			}
		}
		
		/// <summary>Use this method to set-up AspNetDependencyInjection for a self-hosted ASP.NET Web API application, where your application is responsible for instantiating and configuring a <see cref="HttpConfiguration"/> instance.</summary>
		/// <param name="builder">Cannot be <see langword="null"/>.</param>
		/// <param name="httpConfiguration">Cannot be null.</param>
		/// <remarks>Internally, this method adds <see cref="DependencyInjectionWebApiDependencyResolver"/> to <paramref name="builder"/> using <see cref="ApplicationDependencyInjectionBuilder.AddClient(Func{ApplicationDependencyInjection, IDependencyInjectionClient}[])"/>.</remarks>
		public static ApplicationDependencyInjectionBuilder AddWebApiDependencyResolver( this ApplicationDependencyInjectionBuilder builder, HttpConfiguration httpConfiguration )
		{
			if( builder           is null ) throw new ArgumentNullException(nameof(builder));
			if( httpConfiguration is null ) throw new ArgumentNullException(nameof(httpConfiguration));

			return builder
				.AddClient( ( di, rootSP ) => new DependencyInjectionWebApiDependencyResolver( di, rootSP, httpConfiguration ) );
		}


		/// <summary>Attempts to use reflection to get the ASP.NET Web Host's <see cref="HttpConfiguration"/> from <c>System.Web.Http.GlobalConfiguration.Configuration</c></summary>
		/// <param name="httpConfiguration">Value is undefined when this method returns <see langword="false"/>.<br />When this method returns <see langword="true"/> then <paramref name="httpConfiguration"/> will be a valid instance reference.</param>
		/// <param name="errorMessage">This value is undefined when this method returns <see langword="true"/>.<br />When this method returns <see langword="false"/> then <paramref name="errorMessage"/> is a non-empty <see cref="String"/> value.</param>
		public static Boolean TryGetGlobalHttpConfiguration( out HttpConfiguration httpConfiguration, out String errorMessage )
		{
			const String globalConfigurationFullyQualifiedTypeName = "System.Web.Http.GlobalConfiguration, System.Web.Http.WebHost";

			Type globalCfgType = Type.GetType( typeName: globalConfigurationFullyQualifiedTypeName, throwOnError: false, ignoreCase: true ); // `ignoreCase: true` in case the assembly-part of the FQTN changes case - which y'never know could happen (remember `System.configuration"?)
			if( globalCfgType is null )
			{
				httpConfiguration = default;
				errorMessage = "Couldn't get the System.Type for \"" + globalConfigurationFullyQualifiedTypeName + "\".";
				return false;
			}

			PropertyInfo configurationProperty = globalCfgType.GetProperty( name: "Configuration", BindingFlags.Public | BindingFlags.Static );
			if( configurationProperty is null )
			{
				httpConfiguration = default;
				errorMessage = "Couldn't get the \"Configuration\" PropertyInfo from \"" + globalConfigurationFullyQualifiedTypeName + "\".";
				return false;
			}

			Object propertyValue = configurationProperty.GetValue( obj: null );
			if( propertyValue is HttpConfiguration globalHttpConfigurationInstance )
			{
				errorMessage = default;
				httpConfiguration = globalHttpConfigurationInstance;
				return true;
			}
			else if( propertyValue is null )
			{
				errorMessage = "The static System.Web.Http.GlobalConfiguration.Configuration property returned a null reference.";
				httpConfiguration = default;
				return false;
			}
			else
			{
				errorMessage = "Expected the static System.Web.Http.GlobalConfiguration.Configuration property to return a " + nameof(HttpConfiguration) + " reference but encountered a " + propertyValue.GetType().FullName + " instead.";
				httpConfiguration = default;
				return false;
			}
		}
	}
}
