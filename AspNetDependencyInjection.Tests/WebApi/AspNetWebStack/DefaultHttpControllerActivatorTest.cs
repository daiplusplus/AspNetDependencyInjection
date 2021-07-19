using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;

using AspNetDependencyInjection.Internal;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shouldly;

namespace AspNetDependencyInjection.Tests.WebApi
{
	/// <summary>Ripped from AspNetWebStack's test suite.</summary>
	[TestClass]
	public class DefaultHttpControllerActivatorTest : BaseWebApiWebStackTests
	{
		/// <summary>
		/// <c>System.Web.Http.Dispatcher.DefaultHttpControllerActivatorTest</c><br />
		/// <c>AspNetWebStack\test\System.Web.Http.Test\Dispatcher\DefaultHttpControllerActivatorTest.cs</c>
		/// </summary>
		[TestMethod]
		public void Create_ThrowsForNullDependencyScope()
		{
			this.WrapTest( services => services.AddTransient<IActionValueBinder,TestActionValueBinder>(), this.Create_ThrowsForNullDependencyScope_Impl );
		}

		private void Create_ThrowsForNullDependencyScope_Impl( IDependencyResolver resolverOrig )
		{
			IDependencyResolver resolver2 = new DependencyResolverWrapper( inner: resolverOrig, allowBeginScope: false );

			// Arrange
			HttpConfiguration config = new HttpConfiguration();
			config.DependencyResolver = resolver2;

			HttpRequestMessage request = new HttpRequestMessage();
			request.SetConfiguration(config);

			HttpControllerDescriptor descriptorSimpleController = new HttpControllerDescriptor(config, "Simple", typeof(SimpleController));

			DefaultHttpControllerActivator activator = new DefaultHttpControllerActivator();

			// Act & Assert
			try
			{
				IHttpController controller = activator.Create( request, descriptorSimpleController, typeof(SimpleController) );
				
				true.ShouldBeFalse( "Method did not throw an exception." );
			}
			catch( InvalidOperationException ioEx )
			{
				ioEx.Message.ShouldBe( "An error occurred when trying to create a controller of type 'SimpleController'. Make sure that the controller has a parameterless public constructor." );

				// 'BeginScopeNull_DependencyResolver'
				// 'ObjectProxy(_\\d+)?'

				ioEx.InnerException.Message.ShouldBe(
					"A dependency resolver of type '" + nameof(DependencyResolverWrapper) + "' returned an invalid value of null from its " +
					"BeginScope method. If the container does not have a concept of scope, consider returning a scope " +
					"that resolves in the root of the container instead."
				);

			}
		}
	}
}
