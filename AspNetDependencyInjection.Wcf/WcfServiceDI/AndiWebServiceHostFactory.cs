using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Web;

namespace AspNetDependencyInjection.Wcf
{
	// All services are singletons or non-disposable transients (via factories) btw - no support for scoped services here.

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

		protected override ServiceHost CreateServiceHost( Type serviceType, Uri[] baseAddresses )
		{
			return new AndiWebServiceHost( _sp, serviceType, baseAddresses );
		}
	}

	public class AndiWebServiceHost : WebServiceHost
	{
		public AndiWebServiceHost( IServiceProvider serviceProvider, Type serviceType, params Uri[] baseAddresses )
			: base( serviceType, baseAddresses )
		{
			if( serviceProvider == null ) throw new ArgumentNullException(nameof(serviceProvider));

			//

			ConstructorInfo[] ctors = serviceType.GetConstructors();
			if( ctors.Length != 1 ) throw new InvalidOperationException( "Expected 1 public constructor for " + serviceType.FullName + "." );

			foreach( ContractDescription contractDescription in this.ImplementedContracts.Values )
			{
				// Is it one IInstanceProvider-per-Contract, or per-Injected service, or per-WCF service?
				contractDescription.ContractBehaviors.Add( new AndiInstanceProvider( serviceProvider ) );
			}
		}
	}

	public sealed class AndiInstanceProvider : IInstanceProvider, IContractBehavior
	{
		private readonly IServiceProvider serviceProvider;

		public AndiInstanceProvider( IServiceProvider serviceProvider )
		{
			this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		#region IContractBehavior

		void IContractBehavior.AddBindingParameters( ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters )
		{
		}

		void IContractBehavior.ApplyClientBehavior( ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime )
		{
		}

		void IContractBehavior.ApplyDispatchBehavior( ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime )
		{
			dispatchRuntime.InstanceProvider = this;
		}

		void IContractBehavior.Validate( ContractDescription contractDescription, ServiceEndpoint endpoint )
		{
		}

		#endregion

		#region IInstanceProvider

		Object IInstanceProvider.GetInstance( InstanceContext instanceContext )
		{
			Type serviceType = instanceContext.Host.Description.ServiceType; // TODO: Is this an injected-service type or a WCF service type?
			return this.serviceProvider.GetService( serviceType );
		}

		Object IInstanceProvider.GetInstance( InstanceContext instanceContext, Message message )
		{
			return ((IInstanceProvider)this).GetInstance( instanceContext );
		}

		void IInstanceProvider.ReleaseInstance( InstanceContext instanceContext, Object instance )
		{
			if( instance is IDisposable disposable )
			{
				disposable.Dispose();
			}
		}

		#endregion


	}
}