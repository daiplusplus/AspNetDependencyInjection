using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Services;

using AspNetDependencyInjection.Internal;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shouldly;

namespace AspNetDependencyInjection.Tests.WebApi
{
	/// <summary>Ripped from AspNetWebStack's test suite.</summary>
	[TestClass]
	public class DefaultServicesTests : BaseWebApiWebStackTests
	{
		[TestMethod]
        public void GetService_PrefersServiceInDependencyInjectionContainer()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider = new Mock<IActionValueBinder>().Object;
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            mockDependencyResolver.Setup(dr => dr.GetService(typeof(IActionValueBinder))).Returns(filterProvider);
            config.DependencyResolver = mockDependencyResolver.Object;

            // Act
            object service = defaultServices.GetService(typeof(IActionValueBinder));

            // Assert
            Object.ReferenceEquals( filterProvider, service );
        }

        [TestMethod]
        public void GetService_CachesResultFromDependencyInjectionContainer()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            config.DependencyResolver = mockDependencyResolver.Object;

            // Act
            defaultServices.GetService(typeof(IActionValueBinder));
            defaultServices.GetService(typeof(IActionValueBinder));

            // Assert
            mockDependencyResolver.Verify(dr => dr.GetService(typeof(IActionValueBinder)), Times.Once());
        }

		[TestMethod]
        public void GetServices_PrependsServiceInDependencyInjectionContainer()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            IEnumerable<object> servicesBefore = defaultServices.GetServices(typeof(IFilterProvider));
            var filterProvider = new Mock<IFilterProvider>().Object;
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            
			mockDependencyResolver.Setup(dr => dr.GetServices(typeof(IFilterProvider))).Returns(new[] { filterProvider });
            config.DependencyResolver = mockDependencyResolver.Object;

            // Act
            IEnumerable<object> servicesAfter = defaultServices.GetServices(typeof(IFilterProvider));

            // Assert
			Enumerable
				.SequenceEqual(
					new Object[] { filterProvider }.Concat( servicesBefore ),
					servicesAfter
				)
				.ShouldBeTrue();
        }

		[TestMethod]
        public void GetServices_CachesResultFromDependencyInjectionContainer()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            config.DependencyResolver = mockDependencyResolver.Object;

            // Act
            defaultServices.GetServices(typeof(IFilterProvider));
            defaultServices.GetServices(typeof(IFilterProvider));

            // Assert
            mockDependencyResolver.Verify(dr => dr.GetServices(typeof(IFilterProvider)), Times.Once());
        }
	}
}
