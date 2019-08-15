using System;

namespace AspNetDependencyInjection
{
	/// <summary>Informs <see cref="AspNetDependencyInjection.Internal.DependencyInjectionWebObjectActivator"/> which service-types should not be created using the configured <see cref="IServiceProvider"/> and should instead always be constructed using ASP.NET's default activator (as though no <see cref="System.Web.HttpRuntime.WebObjectActivator"/> were set).</summary>
	public interface IDependencyInjectionExclusionService
	{
		/// <summary>Returns <c>true</c> if the specified <paramref name="type"/> should be created using <see cref="Activator"/> (ASP.NET's default object factory) instead of using the configured <see cref="IServiceProvider"/>. The <see cref="IDependencyInjectionExclusionServiceExtensions.IsIncluded(IDependencyInjectionExclusionService, Type)"/> extension method calls this method and returns the inverse (logical NOT) of this method's return value.</summary>
		Boolean IsExcluded( Type type );
	}

	/// <summary>Extension methods for <see cref="IDependencyInjectionExclusionService"/>.</summary>
	public static class IDependencyInjectionExclusionServiceExtensions
	{
		/// <summary>Returns the logical NOT of <see cref="IDependencyInjectionExclusionService.IsExcluded(Type)"/>.</summary>
		public static Boolean IsIncluded( this IDependencyInjectionExclusionService exclusionService, Type type )
		{
			if( exclusionService == null ) throw new ArgumentNullException(nameof(exclusionService));

			return !exclusionService.IsExcluded( type );
		}
	}
}
