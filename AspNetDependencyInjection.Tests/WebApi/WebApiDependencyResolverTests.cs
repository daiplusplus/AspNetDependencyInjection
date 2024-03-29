﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
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
			HttpConfiguration unitTestWebApiConfiguration = new HttpConfiguration();

			ApplicationDependencyInjection di = new ApplicationDependencyInjectionBuilder()
				.AddWebApiDependencyResolver( unitTestWebApiConfiguration )
				.Build();

			using( di )
			{
				Int32 count = di.GetClients()
					.OfType<DependencyInjectionWebApiDependencyResolver>()
					.Count();
			
				count.ShouldBe( expected: 1 );
			}
		}

		[TestMethod]
		public void WebApi_DependencyResolver_GetService_should_return_null_when_unresolved()
		{
			HttpConfiguration unitTestWebApiConfiguration = new HttpConfiguration();

			ApplicationDependencyInjection di = new ApplicationDependencyInjectionBuilder()
				.ConfigureServices( services => { } )
				.AddWebApiDependencyResolver( unitTestWebApiConfiguration )
				.Build();

			using( di )
			{
				DependencyInjectionWebApiDependencyResolver webApiResolverInstance = di.GetClients()
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
		public void WebApi_DependencyResolver_GetServices_should_return_empty_enumerable_when_unresolved()
		{
			HttpConfiguration unitTestWebApiConfiguration = new HttpConfiguration();

			ApplicationDependencyInjection di = new ApplicationDependencyInjectionBuilder()
				.ConfigureServices( services => { } )
				.AddWebApiDependencyResolver( unitTestWebApiConfiguration )
				.Build();

			using( di )
			{
				DependencyInjectionWebApiDependencyResolver webApiResolverInstance = di.GetClients()
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
