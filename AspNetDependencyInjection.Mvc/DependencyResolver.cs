using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>Implements <see cref="IDependencyResolver"/> by using <see cref="DependencyInjectionWebObjectActivator"/>></summary>
	public class DependencyInjectionWebObjectActivatorDependencyResolver : IDependencyResolver
	{
		private readonly DependencyInjectionWebObjectActivator webObjectActivator;

		public DependencyInjectionWebObjectActivatorDependencyResolver( DependencyInjectionWebObjectActivator webObjectActivator )
		{
			this.webObjectActivator = webObjectActivator ?? throw new ArgumentNullException(nameof(webObjectActivator));
		}

		public Object GetService(Type serviceType)
		{
			return this.webObjectActivator.GetService( serviceType );
		}

		// TODO: Is this a correct implementation of IDependencyResolver.GetServices?
		public IEnumerable<Object> GetServices(Type serviceType)
		{
			return this.webObjectActivator.GetServices( serviceType );
		}
	}

	// It isn't necessary to subclass DefaultControllerFactory or implement IControllerFactory.
	// Because DefaultControllerFactory uses the registered IDependencyResolver anyway.
}
