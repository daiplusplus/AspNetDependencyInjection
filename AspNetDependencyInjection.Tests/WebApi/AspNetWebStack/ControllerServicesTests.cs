using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;

using AspNetDependencyInjection.Internal;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shouldly;

namespace AspNetDependencyInjection.Tests.WebApi
{
	public abstract class BaseWebApiWebStackTests
	{
		protected class SimpleController : ApiController
		{
        }

		protected class TestActionValueBinder : IActionValueBinder
		{
			public HttpActionBinding GetBinding( HttpActionDescriptor actionDescriptor )
			{
				throw new NotImplementedException();
			}
		}

		protected void WrapTest( Action<IServiceCollection> configure, Action<IDependencyResolver> testImpl )
		{
			ApplicationDependencyInjection di = new ApplicationDependencyInjectionBuilder()
				.ConfigureServices( configure )
				.AddWebApiDependencyResolver()
				.Build();

			using( di )
			{
				DependencyInjectionWebApiDependencyResolver webApiResolverInstance = di.Clients
					.OfType<DependencyInjectionWebApiDependencyResolver>()
					.Single();

				IDependencyResolver webApiResolver = webApiResolverInstance;

				testImpl( webApiResolver );
			}
		}
	}

	/// <summary>Ripped from AspNetWebStack's test suite.</summary>
	[TestClass]
	public class ControllerServicesTests : BaseWebApiWebStackTests
	{
		[TestMethod]
        public void Controller_Overrides_DependencyInjection()
        {
            this.WrapTest( services => services.AddScoped<IActionValueBinder,TestActionValueBinder>(), this.Controller_Overrides_DependencyInjection_Impl );
        }

        private void Controller_Overrides_DependencyInjection_Impl( IDependencyResolver resolver )
        {
			// Setting on Controller config overrides the DI container. 
            HttpConfiguration config = new HttpConfiguration();

			IActionValueBinder newDIService = new TestActionValueBinder();

            config.DependencyResolver = resolver;
            
            ControllerServices cs = new ControllerServices(config.Services);

            IActionValueBinder newLocalService = new TestActionValueBinder();
            cs.Replace(typeof(IActionValueBinder), newLocalService);

            // Act            
            IActionValueBinder localVal = (IActionValueBinder)cs.GetService(typeof(IActionValueBinder));
            IActionValueBinder globalVal = (IActionValueBinder)config.Services.GetService(typeof(IActionValueBinder));

            // Assert
            // Local controller didn't override, should get same value as global case.            
            Object.ReferenceEquals( newDIService, globalVal ).ShouldBeTrue(); // asking the config will give back the DI service
            Object.ReferenceEquals( newLocalService, localVal ).ShouldBeTrue(); // but asking locally will get back the local service.
		}
	}
}
