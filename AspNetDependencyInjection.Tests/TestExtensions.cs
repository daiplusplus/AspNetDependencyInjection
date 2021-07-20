using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AspNetDependencyInjection.Internal;

namespace AspNetDependencyInjection.Tests
{
	public static class TestExtensions
	{
		public static IReadOnlyList<IDependencyInjectionClient> GetClients( this ApplicationDependencyInjection di )
		{
			IHasDependencyInjectionClients asInterface = (IHasDependencyInjectionClients)di;

			return asInterface.Clients;
		}
	}
}
