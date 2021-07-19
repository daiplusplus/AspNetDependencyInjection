using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shouldly;

namespace AspNetDependencyInjection.Tests.WebApi
{
	/// <summary>Ripped from AspNetWebStack's test suite.</summary>
	[TestClass]
	public class ControllerServicesTests : BaseWebApiWebStackTests
	{
		[TestMethod]
		public void Controller_Overrides_DependencyInjection()
		{
			TestActionValueBinder newDIService = new TestActionValueBinder();

			this.WrapTest(
				services =>
				{
					_ = services.AddSingleton<IActionValueBinder>( implementationInstance: newDIService );
//					_ = services.AddScoped<IActionValueBinder,TestActionValueBinder>();
				},
				this.Controller_Overrides_DependencyInjection_Impl,
				newDIService
			);
		}

		private void Controller_Overrides_DependencyInjection_Impl( IDependencyResolver resolver, TestActionValueBinder newDIService )
		{
			if( resolver is null ) throw new ArgumentNullException( nameof( resolver ) );
			
			// Calling `.BeginScope()` here is to simulate what happens inside `DefaultHttpControllerActivator.GetInstanceOrActivator`
			using( IDependencyScope requestScope = resolver.BeginScope() )
			{
				_ = requestScope.ShouldNotBeNull();

				// Setting on Controller config overrides the DI container. 
				HttpConfiguration config = new HttpConfiguration();

//				IActionValueBinder newDIService = new TestActionValueBinder();

				config.DependencyResolver = resolver;
			
				ControllerServices cs = new ControllerServices(config.Services);

				IActionValueBinder newLocalService = new TestActionValueBinder();
				cs.Replace(typeof(IActionValueBinder), newLocalService);

				// Act            
				IActionValueBinder localVal = (IActionValueBinder)cs.GetService(typeof(IActionValueBinder));
				IActionValueBinder globalVal = (IActionValueBinder)config.Services.GetService(typeof(IActionValueBinder));

				_ = localVal .ShouldNotBeNull().ShouldBeOfType<TestActionValueBinder>();
				_ = globalVal.ShouldNotBeNull().ShouldBeOfType<TestActionValueBinder>();

				// Assert
				// Local controller didn't override, should get same value as global case.            
				Object.ReferenceEquals( newDIService, globalVal ).ShouldBeTrue(); // asking the config will give back the DI service
				Object.ReferenceEquals( newLocalService, localVal ).ShouldBeTrue(); // but asking locally will get back the local service.
			}
		}
	}
}
