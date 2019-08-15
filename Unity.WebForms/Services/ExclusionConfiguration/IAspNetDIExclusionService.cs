using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unity.WebForms.Services
{
	/// <summary>Informs <see cref="Unity.WebForms.Internal.MediWebObjectActivator"/> which service-types should not be created using the configured <see cref="IServiceProvider"/> and should instead always be constructed using ASP.NET's default activator (as though no <see cref="System.Web.HttpRuntime.WebObjectActivator"/> were set).</summary>
	public interface IAspNetDIExclusionService
	{
		/// <summary>Returns <c>true</c> if the specified <paramref name="type"/> should be created using <see cref="Activator"/> (ASP.NET's default object factory) instead of using the configured <see cref="IServiceProvider"/>. The <see cref="AspNetDIExclusionServiceExtensions.IsIncluded(IAspNetDIExclusionService, Type)"/> extension method calls this method and returns the inverse (logical NOT) of this method's return value.</summary>
		Boolean IsExcluded( Type type );
	}

	/// <summary>Extension methods for <see cref="IAspNetDIExclusionService"/>.</summary>
	public static class AspNetDIExclusionServiceExtensions
	{
		/// <summary>Returns the logical NOT of <see cref="IAspNetDIExclusionService.IsExcluded(Type)"/>.</summary>
		public static Boolean IsIncluded( this IAspNetDIExclusionService exclusionService, Type type )
		{
			if( exclusionService == null ) throw new ArgumentNullException(nameof(exclusionService));

			return !exclusionService.IsExcluded( type );
		}
	}
}
