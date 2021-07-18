using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;

using AspNetDependencyInjection.Internal;

using Microsoft.Extensions.DependencyInjection;

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

		protected class DependencyResolverWrapper : IDependencyResolver
		{
			private readonly IDependencyResolver inner;
			private readonly Boolean allowBeginScope;

			public DependencyResolverWrapper( IDependencyResolver inner, Boolean allowBeginScope )
			{
				this.inner = inner;
				this.allowBeginScope = allowBeginScope;
			}

			public IDependencyScope BeginScope()
			{
				if( this.allowBeginScope )
				{
					return this.inner.BeginScope();
				}
				else
				{
					// This is the custom logic set in the original `Create_ThrowsForNullDependencyScope`.
					return null;
				}
			}

			public List<Type> GetServiceCalls { get; } = new List<Type>();
			public List<Type> GetServicesCalls { get; } = new List<Type>();

			public Object GetService( Type serviceType )
			{
				this.GetServiceCalls.Add( serviceType );
				return this.inner.GetService( serviceType );
			}

			public IEnumerable<Object> GetServices( Type serviceType )
			{
				this.GetServicesCalls.Add( serviceType );
				return this.inner.GetServices( serviceType );
			}

			public void Dispose()
			{
				this.inner.Dispose();
			}
		}

		protected void WrapTest( Action<IServiceCollection> configure, Action<IDependencyResolver> testImpl )
		{
			this.WrapTest<Object>( configure, testImpl: ( resolver, arg ) => testImpl( resolver ), arg: default );
		}

		protected void WrapTest<T>( Action<IServiceCollection> configure, Action<IDependencyResolver,T> testImpl, T arg )
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

				testImpl( webApiResolver, arg );
			}
		}
	}
}
