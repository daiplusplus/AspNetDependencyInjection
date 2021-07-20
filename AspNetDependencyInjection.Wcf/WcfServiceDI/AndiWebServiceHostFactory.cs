using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Web;

namespace AspNetDependencyInjection.Wcf
{
	// All services are singletons or non-disposable transients (via factories) btw - no support for scoped services here.

	/// <summary>Subclass of <see cref="WebServiceHostFactory"/> which creates <see cref="AndiWebServiceHost"/> instances.</summary>
	public class AndiWebServiceHostFactory : WebServiceHostFactory
	{
		private class AndiWebObjectActivatorAndiServiceProvider : IServiceProvider
		{
			public Object GetService( Type serviceType )
			{
				return HttpRuntime.WebObjectActivator.GetService( serviceType );
			}
		}

		private static readonly AndiWebObjectActivatorAndiServiceProvider _sp = new AndiWebObjectActivatorAndiServiceProvider();

		/// <summary>Creates new <see cref="AndiWebServiceHost"/> instances with a custom <see cref="IServiceProvider"/> that uses <see cref="HttpRuntime.WebObjectActivator"/>.</summary>
		protected override ServiceHost CreateServiceHost( Type serviceType, Uri[] baseAddresses )
		{
			return new AndiWebServiceHost( serviceProvider: _sp, serviceType, baseAddresses );
		}
	}

	/// <summary>Subclass of <see cref="WebServiceHost"/> that adds new <see cref="ContractDescription"/> to every <see cref="ServiceHostBase.ImplementedContracts"/> that allows access to the cutom <see cref="IServiceProvider"/> passed into the <see cref="AndiWebServiceHost.AndiWebServiceHost(IServiceProvider, Type, Uri[])"/> constructor.</summary>
	public class AndiWebServiceHost : WebServiceHost
	{
		/// <summary>Initializes a new instance of the <see cref="AndiWebServiceHost"/> class with the specified <paramref name="serviceType"/> and <paramref name="baseAddresses"/>. The <paramref name="serviceProvider"/> is used to extend <see cref="ServiceHostBase.ImplementedContracts"/>'s <see cref="ContractDescription.ContractBehaviors"/>.</summary>
		/// <param name="serviceProvider"></param>
		/// <param name="serviceType"></param>
		/// <param name="baseAddresses"></param>
		public AndiWebServiceHost( IServiceProvider serviceProvider, Type serviceType, params Uri[] baseAddresses )
			: base( serviceType, baseAddresses )
		{
			if( serviceProvider is null ) throw new ArgumentNullException(nameof(serviceProvider));
			if( serviceType     is null ) throw new ArgumentNullException(nameof(serviceType));

			//

			ConstructorInfo[] ctors = serviceType.GetConstructors();
			if( ctors.Length != 1 ) throw new InvalidOperationException( "Expected 1 public constructor for " + serviceType.FullName + "." );

			foreach( ContractDescription contractDescription in this.ImplementedContracts.Values )
			{
				// TODO: Is it one IInstanceProvider-per-Contract, or per-Injected service, or per-WCF service?
				contractDescription.ContractBehaviors.Add( new AndiInstanceProvider( serviceProvider ) );
			}
		}
	}
}
