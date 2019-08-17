using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

using AspNetDependencyInjection.Internal;
using AspNetDependencyInjection.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AspNetDependencyInjection
{
	/// <summary>Uses </summary>
	public class MvcDependencyInjectionFallbackService : DefaultDependencyInjectionFallbackService
	{
		private readonly IDependencyResolver mvcDependencyResolver;
		private readonly DependencyResolverToServiceProviderAdapter adapter;

		public MvcDependencyInjectionFallbackService( Boolean excludeAspNetNamespacesFromDI, IEnumerable<String> additionalExclusions, IDependencyResolver mvcDependencyResolver )
			: base( excludeAspNetNamespacesFromDI, additionalExclusions )
		{
			this.mvcDependencyResolver = mvcDependencyResolver ?? throw new ArgumentNullException(nameof(mvcDependencyResolver));
			this.adapter               = new DependencyResolverToServiceProviderAdapter( mvcDependencyResolver );
		}

		public override Boolean TryGetServiceProvider( Type type, out IServiceProvider serviceProvider )
		{
			if( base.TryGetServiceProvider( type, out serviceProvider ) )
			{
				return true;
			}
			else
			{
//				if( type.FullName.StartsWith( "System.Web.Mvc.", StringComparison.Ordinal ) )
//				{
//					serviceProvider = this.adapter;
//					return true;
//				}
//				else
				{
					serviceProvider = default;
					return false;
				}
			}
		}
	}

	/// <summary>Extension methods for services bundled with <see cref="AspNetDependencyInjection"/>.</summary>
	public static class MvcServiceCollectionExtensions
	{
		public static IServiceCollection AddAspNetMvcFallbackService( this IServiceCollection services, IDependencyResolver mvcDependencyResolver, Boolean excludeAspNetNamespacesFromDI = true, IEnumerable<String> additionalExclusions = null )
		{
			return services
				.AddSingleton<IDependencyInjectionFallbackService>( sp => new MvcDependencyInjectionFallbackService( excludeAspNetNamespacesFromDI, additionalExclusions, mvcDependencyResolver ) );
		}

		internal static void TryAddAspNetMvcFallbackService( this IServiceCollection services, IDependencyResolver mvcDependencyResolver )
		{
			services
				.TryAddSingleton<IDependencyInjectionFallbackService>( sp => new MvcDependencyInjectionFallbackService( excludeAspNetNamespacesFromDI: true, additionalExclusions: null, mvcDependencyResolver: mvcDependencyResolver ) );
		}
	}
}

namespace AspNetDependencyInjection.Internal
{
	public class DependencyResolverToServiceProviderAdapter : IServiceProvider
	{
		private readonly IDependencyResolver dependencyResolver;

		public DependencyResolverToServiceProviderAdapter( IDependencyResolver dependencyResolver )
		{
			this.dependencyResolver = dependencyResolver ?? throw new ArgumentNullException(nameof(dependencyResolver));
		}

		public Object GetService(Type serviceType)
		{
			return this.dependencyResolver.GetService( serviceType );
		}
	}
}