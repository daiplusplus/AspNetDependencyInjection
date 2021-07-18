using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.ModelBinding;

using AspNetDependencyInjection.Internal;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shouldly;

namespace AspNetDependencyInjection.Tests.WebApi
{
	/// <summary>Ripped from AspNetWebStack's test suite.</summary>
	[TestClass]
	public class ModelBinderAttributeTest : BaseWebApiWebStackTests
	{


		/// <summary>
		/// <c>SSystem.Web.Http.ModelBinding.ModelBinderAttributeTest</c><br />
		/// <c>AspNetWebStack\test\System.Web.Http.Test\ModelBinding\ModelBinderAttributeTest.cs</c>
		/// </summary>
		[TestMethod]
		public void BinderType_From_DependencyResolver()
		{
			this.WrapTest( services => services.AddScoped<IActionValueBinder,TestActionValueBinder>(), this.Controller_Overrides_DependencyInjection_Impl );
		}

		private void BinderType_From_DependencyResolver_Impl( IDependencyResolver resolver )
		{
			// To test dependency resolver, the registered type and actual type should be different.
			HttpConfiguration config = new HttpConfiguration();
			var mockDependencyResolver = new Mock<IDependencyResolver>();
			mockDependencyResolver.Setup(r => r.GetService(typeof(CustomModelBinderProvider))).Returns(new SecondCustomModelBinderProvider());
			config.DependencyResolver = mockDependencyResolver.Object;

			ModelBinderAttribute attr = new ModelBinderAttribute(typeof(CustomModelBinderProvider));

			ModelBinderProvider provider = attr.GetModelBinderProvider(config);
			Assert.IsType<SecondCustomModelBinderProvider>(provider);
		}

		/// <summary>
		/// <c>System.Web.Http.ModelBinding.ModelBinderAttributeTest</c><br />
		/// <c>AspNetWebStack\test\System.Web.Http.Test\ModelBinding\ModelBinderAttributeTest.cs</c>
		/// </summary>
		[TestMethod]
		public void BinderType_From_DependencyResolver_ReleasedWhenConfigIsDisposed()
		{
			// Arrange
			HttpConfiguration config = new HttpConfiguration();
			var mockDependencyResolver = new Mock<IDependencyResolver>();
			SecondCustomModelBinderProvider provider = new SecondCustomModelBinderProvider();
			mockDependencyResolver.Setup(r => r.GetService(typeof(CustomModelBinderProvider))).Returns(provider);
			config.DependencyResolver = mockDependencyResolver.Object;

			ModelBinderAttribute attr = new ModelBinderAttribute(typeof(CustomModelBinderProvider));
			attr.GetModelBinderProvider(config);

			// Act
			config.Dispose();

			// Assert
			mockDependencyResolver.Verify(dr => dr.Dispose(), Times.Once());
		}
	}
}
