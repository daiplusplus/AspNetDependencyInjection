using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

using AspNetDependencyInjection.Internal;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shouldly;

namespace AspNetDependencyInjection.Tests.Mvc
{
	[TestClass]
	public class MvcDependencyResolverTests
	{
		[TestMethod]
		public void AddWebApiDependencyResolver_should_add_single_DependencyInjectionWebApiDependencyResolver()
		{
			ApplicationDependencyInjection di = new ApplicationDependencyInjectionBuilder()
				.AddMvcDependencyResolver()
				.Build();

			using( di )
			{
				Int32 count = di.Clients
					.OfType<DependencyInjectionMvcDependencyResolver>()
					.Count();
			
				count.ShouldBe( expected: 1 );
			}
		}

		[TestMethod]
		public void DependencyResolver_GetService_should_return_null_for_unregistered_types()
		{
			ApplicationDependencyInjection di = new ApplicationDependencyInjectionBuilder()
				.ConfigureServices( services => { } )
				.AddMvcDependencyResolver()
				.Build();

			using( di )
			{
				DependencyInjectionMvcDependencyResolver mvcResolverInstance = di.Clients
					.OfType<DependencyInjectionMvcDependencyResolver>()
					.Single();

				//

				IDependencyResolver mvcResolver = mvcResolverInstance;

				//

				{
					Object implementation = mvcResolver.GetService( typeof(INonRegisteredService) );
					implementation.ShouldBeNull();
				}

				// And call it again for good measure:
				{
					Object implementation = mvcResolver.GetService( typeof(INonRegisteredService) );
					implementation.ShouldBeNull();
				}
			}
		}

		[TestMethod]
		public void DependencyResolver_GetServices_should_return_empty_enumerable_when_unresolved()
		{
			ApplicationDependencyInjection di = new ApplicationDependencyInjectionBuilder()
				.ConfigureServices( services => { } )
				.AddMvcDependencyResolver()
				.Build();

			using( di )
			{
				DependencyInjectionMvcDependencyResolver mvcResolverInstance = di.Clients
					.OfType<DependencyInjectionMvcDependencyResolver>()
					.Single();

				//

				IDependencyResolver mvcResolver = mvcResolverInstance;

				//

				IEnumerable<Object> services = mvcResolver.GetServices( typeof(INonRegisteredService) );
				_ = services.ShouldNotBeNull();
				services.Count().ShouldBe( expected: 0 );

				// Check twice, in case of IEnumerable<T> implementation shenanigans:
				services.Count().ShouldBe( expected: 0 );
			}
		}

	#region Reimplementing ASP.NET's own Unit Tests

		// See \AspNetWebStack\test\System.Web.Mvc.Test\Test\DependencyResolverTest.cs

		/// <summary>Don't call this method from tests that verify this logic works, like <see cref="AddWebApiDependencyResolver_should_add_single_DependencyInjectionWebApiDependencyResolver"/>.</summary>
		private static ApplicationDependencyInjection CreateMvcDIWithTestServiceRegistrations( out IDependencyResolver mvcResolver )
		{
			ApplicationDependencyInjection di = new ApplicationDependencyInjectionBuilder()
				.ConfigureServices( services => {
					_ = services.AddTransient<ITestTransient,TestTransient>();
					_ = services.AddScoped   <ITestScoped,TestScoped>();
					_ = services.AddSingleton<ITestSingleton,TestSingleton>();
					_ = services.AddSingleton<TestAbstractClass,TestAbstractImpl>();
				} )
				.AddMvcDependencyResolver()
				.Build();

			DependencyInjectionMvcDependencyResolver mvcResolverInstance = di.Clients
				.OfType<DependencyInjectionMvcDependencyResolver>()
				.Single();

			mvcResolver = mvcResolverInstance;

			return di;
		}

		#region DefaultServiceLocatorBehaviorTests

		[TestMethod]
		public void DependencyResolver_GetService_should_return_null_for_registered_abstract_types()
		{
			using( ApplicationDependencyInjection di = CreateMvcDIWithTestServiceRegistrations( out IDependencyResolver resolver ) )
			{
				TestAbstractClass service = resolver.GetService<TestAbstractClass>();
				service.ShouldBeNull();
			}
		}

		/// <summary>This behavior really surprises me - I honest thought that ASP.NET MVC would allow for using interfaces. How are controllers meant to use DI services via interfaces?</summary>
		[TestMethod]
		public void DependencyResolver_GetService_should_return_null_for_registered_interface_types()
		{
			using( ApplicationDependencyInjection di = CreateMvcDIWithTestServiceRegistrations( out IDependencyResolver resolver ) )
			{
				ITestTransient service = resolver.GetService<ITestTransient>();
				service.ShouldBeNull();
			}
		}

		[TestMethod]
		public void DependencyResolver_GetService_should_return_null_for_open_generic_types()
		{
			using( ApplicationDependencyInjection di = CreateMvcDIWithTestServiceRegistrations( out IDependencyResolver resolver ) )
			{
				Object service = resolver.GetService( typeof(ITestGeneric<>) );
				service.ShouldBeNull();
			}
		}

		[TestMethod]
		public void DependencyResolver_GetService_should_resolve_SystemObject()
		{
			using( ApplicationDependencyInjection di = CreateMvcDIWithTestServiceRegistrations( out IDependencyResolver resolver ) )
			{
				Object service = resolver.GetService<Object>();
				_ = service.ShouldNotBeNull();
				_ = service.ShouldBeOfType<Object>();
				service.GetType().ShouldBe( typeof(System.Object) );
			}
		}

		#endregion

		[TestMethod]
		public void DependencyResolver_GetService_should_return_separate_instances()
		{
			using( ApplicationDependencyInjection di = CreateMvcDIWithTestServiceRegistrations( out IDependencyResolver resolver ) )
			{
				Object object1 = resolver.GetService<Object>();
				Object object2 = resolver.GetService<Object>();
				
				Object.ReferenceEquals( object1, object2 ).ShouldBeFalse();
			}
		}

	#endregion
	}

	internal interface INonRegisteredService
	{
	}

	#region Transient

	public interface ITestTransient
	{
	}

	public class TestTransient : ITestTransient
	{
		private static Int32 _instanceCount;
		public static Int32 InstanceCount => _instanceCount;

		public TestTransient()
		{
			this.InstanceNumber = Interlocked.Increment( ref _instanceCount );
		}

		public Int32 InstanceNumber { get; }
	}

	#endregion

	#region Scoped

	public interface ITestScoped
	{
	}

	public class TestScoped : ITestScoped
	{
		private static Int32 _instanceCount;
		public static Int32 InstanceCount => _instanceCount;

		public TestScoped()
		{
			this.InstanceNumber = Interlocked.Increment( ref _instanceCount );
		}

		public Int32 InstanceNumber { get; }
	}

	#endregion

	#region Singleton

	public interface ITestSingleton
	{
	}

	public class TestSingleton : ITestSingleton
	{
		private static Int32 _instanceCount;
		public static Int32 InstanceCount => _instanceCount;

		public TestSingleton()
		{
			this.InstanceNumber = Interlocked.Increment( ref _instanceCount );
		}

		public Int32 InstanceNumber { get; }
	}

	#endregion

	#region Generic

	public interface ITestGeneric<T>
	{
	}

	public class TestestGeneric<T> : ITestGeneric<T>
	{
		private static Int32 _instanceCount;
		public static Int32 InstanceCount => _instanceCount;

		public TestestGeneric()
		{
			this.InstanceNumber = Interlocked.Increment( ref _instanceCount );
		}

		public Int32 InstanceNumber { get; }
	}

	#endregion

	#region Abstract

	public abstract class TestAbstractClass
	{
		private static Int32 _instanceCount;
		public static Int32 InstanceCount => _instanceCount;

		public TestAbstractClass()
		{
			this.InstanceNumber = Interlocked.Increment( ref _instanceCount );
		}

		public Int32 InstanceNumber { get; }
	}

	public class TestAbstractImpl : TestAbstractClass
	{
		public TestAbstractImpl()
		{
		}
	}

	#endregion

	
}
