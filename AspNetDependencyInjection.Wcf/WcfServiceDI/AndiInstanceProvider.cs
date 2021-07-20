using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace AspNetDependencyInjection.Wcf
{
	/// <summary>Implements <see cref="IContractBehavior"/> and <see cref="IInstanceProvider"/>.</summary>
	public sealed class AndiInstanceProvider : IInstanceProvider, IContractBehavior
	{
		private readonly IServiceProvider serviceProvider;

		/// <summary>Constructor. This is called by <see cref="AndiWebServiceHost.AndiWebServiceHost(IServiceProvider, Type, Uri[])"/> when extending <see cref="ContractDescription"/>'s <see cref="ContractDescription.ContractBehaviors"/>.</summary>
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
			IInstanceProvider selfAsIInstanceProvider = this;
			return selfAsIInstanceProvider.GetInstance( instanceContext );
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
