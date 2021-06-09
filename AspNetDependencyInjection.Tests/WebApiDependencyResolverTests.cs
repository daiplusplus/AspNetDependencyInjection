using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;

using AspNetDependencyInjection.Internal;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shouldly;

namespace AspNetDependencyInjection.Tests.WebApi
{
	[TestClass]
	public class WebApiDependencyResolverTests
	{
		[TestMethod]
		public void AddWebApiDependencyResolver_should_add_single_DependencyInjectionWebApiDependencyResolver()
		{
			ApplicationDependencyInjection di = new ApplicationDependencyInjectionBuilder()
				.AddWebApiDependencyResolver()
				.Build();

			using( di )
			{
				Int32 count = di.Clients
					.OfType<DependencyInjectionWebApiDependencyResolver>()
					.Count();
			
				count.ShouldBe( expected: 1 );
			}
		}

		[TestMethod]
		public void DependencyResolver_GetService_should_return_null_when_unresolved()
		{
			ApplicationDependencyInjection di = new ApplicationDependencyInjectionBuilder()
				.ConfigureServices( services => { } )
				.AddWebApiDependencyResolver()
				.Build();

			using( di )
			{
				DependencyInjectionWebApiDependencyResolver webApiResolverInstance = di.Clients
					.OfType<DependencyInjectionWebApiDependencyResolver>()
					.Single();

				//

				IDependencyResolver webApiResolver = webApiResolverInstance;

				//

				{
					Object implementation = webApiResolver.GetService( typeof(INonRegisteredService) );
					implementation.ShouldBeNull();
				}

				// And call it again for good measure:
				{
					Object implementation = webApiResolver.GetService( typeof(INonRegisteredService) );
					implementation.ShouldBeNull();
				}
			}
		}

		[TestMethod]
		public void DependencyResolver_GetServices_should_return_empty_enumerable_when_unresolved()
		{
			ApplicationDependencyInjection di = new ApplicationDependencyInjectionBuilder()
				.ConfigureServices( services => { } )
				.AddWebApiDependencyResolver()
				.Build();

			using( di )
			{
				DependencyInjectionWebApiDependencyResolver webApiResolverInstance = di.Clients
					.OfType<DependencyInjectionWebApiDependencyResolver>()
					.Single();

				//

				IDependencyResolver webApiResolver = webApiResolverInstance;

				//

				IEnumerable<Object> services = webApiResolver.GetServices( typeof(INonRegisteredService) );
				_ = services.ShouldNotBeNull();
				services.Count().ShouldBe( expected: 0 );

				// Check twice, in case of IEnumerable<T> implementation shenanigans:
				services.Count().ShouldBe( expected: 0 );
			}
		}
	}

	internal interface INonRegisteredService
	{
	}
}
