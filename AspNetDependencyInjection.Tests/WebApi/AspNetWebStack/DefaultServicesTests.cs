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
			TestActionValueBinder instance = new TestActionValueBinder();

			this.WrapTest(
				configure: services => services.AddSingleton<IActionValueBinder>( implementationInstance: instance ),
				testImpl : this.GetService_PrefersServiceInDependencyInjectionContainer_Impl,
				arg      : instance
			);
		}

		private void GetService_PrefersServiceInDependencyInjectionContainer_Impl( IDependencyResolver resolver, TestActionValueBinder serviceInstance )
		{
			// Arrange
			HttpConfiguration config = new HttpConfiguration();
			DefaultServices defaultServices = new DefaultServices(config);
			IActionValueBinder filterProvider = serviceInstance;
			config.DependencyResolver = resolver;

			// Act
			object service = defaultServices.GetService(typeof(IActionValueBinder));

			// Assert
			Object.ReferenceEquals( filterProvider, service ).ShouldBeTrue();
		}

		[TestMethod]
		public void GetService_CachesResultFromDependencyInjectionContainer()
		{
			this.WrapTest(
				configure: services => services.AddTransient<IActionValueBinder,TestActionValueBinder>(),
				testImpl : this.GetService_CachesResultFromDependencyInjectionContainer_Impl
			);
		}

		private void GetService_CachesResultFromDependencyInjectionContainer_Impl( IDependencyResolver resolver )
		{
			DependencyResolverWrapper resolverWrapper = new DependencyResolverWrapper( resolver, allowBeginScope: true );

			// Arrange
			HttpConfiguration config = new HttpConfiguration();
			DefaultServices defaultServices = new DefaultServices(config);
			config.DependencyResolver = resolverWrapper;

			// Act
			IActionValueBinder a = (IActionValueBinder)defaultServices.GetService(typeof(IActionValueBinder));
			IActionValueBinder b = (IActionValueBinder)defaultServices.GetService(typeof(IActionValueBinder));

			// Assert

			resolverWrapper.GetServiceCalls.Count.ShouldBe( 1 );
			resolverWrapper.GetServicesCalls.Count.ShouldBe( 0 );
			Object.ReferenceEquals( a, b ).ShouldBeTrue();
		}

		private class TestFilterProvider : IFilterProvider
		{
			public IEnumerable<FilterInfo> GetFilters( HttpConfiguration configuration, HttpActionDescriptor actionDescriptor )
			{
				throw new NotImplementedException();
			}
		}

		[TestMethod]
		public void GetServices_PrependsServiceInDependencyInjectionContainer()
		{
			IFilterProvider filterProvider = new TestFilterProvider();

			this.WrapTest(
				services => services.TryAddSingleton<IFilterProvider>( instance: filterProvider ),//,TestFilterProvider>(), // or transient but have its factory return the same instance? hmm
				this.GetServices_PrependsServiceInDependencyInjectionContainer_Impl,
				filterProvider
			);
		}

		private void GetServices_PrependsServiceInDependencyInjectionContainer_Impl( IDependencyResolver resolver, IFilterProvider filterProviderInstance )
		{
			// Arrange
			HttpConfiguration config = new HttpConfiguration();
			DefaultServices defaultServices = new DefaultServices(config);

			IEnumerable<object> servicesBefore = defaultServices.GetServices(typeof(IFilterProvider));
			
//			var mockDependencyResolver = new Mock<IDependencyResolver>();
//			mockDependencyResolver.Setup(dr => dr.GetServices(typeof(IFilterProvider))).Returns(new[] { filterProvider });
			config.DependencyResolver = resolver;

			// Act
			IEnumerable<object> servicesAfter = defaultServices.GetServices(typeof(IFilterProvider));

			// Assert
			Enumerable
				.SequenceEqual(
					new Object[] { filterProviderInstance }.Concat( servicesBefore ),
					servicesAfter
				)
				.ShouldBeTrue();
		}

		[TestMethod]
		public void GetServices_CachesResultFromDependencyInjectionContainer()
		{
			this.WrapTest( services => services.AddScoped<IActionValueBinder,TestActionValueBinder>(), this.GetServices_CachesResultFromDependencyInjectionContainer_Impl );
		}

		private void GetServices_CachesResultFromDependencyInjectionContainer_Impl( IDependencyResolver resolver )
		{
			DependencyResolverWrapper rw = new DependencyResolverWrapper( resolver, allowBeginScope: true );

			// Arrange
			HttpConfiguration config = new HttpConfiguration();
			DefaultServices defaultServices = new DefaultServices(config);

			config.DependencyResolver = rw;

			// Act
			Object a = defaultServices.GetServices(typeof(IFilterProvider));
			Object b = defaultServices.GetServices(typeof(IFilterProvider));

			_ = a.ShouldNotBeNull();
			_ = b.ShouldNotBeNull();

			// Assert
			rw.GetServicesCalls.Count.ShouldBe( 1 );
//			mockDependencyResolver.Verify(dr => dr.GetServices(typeof(IFilterProvider)), Times.Once());
		}
	}
}
