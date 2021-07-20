using System;
using System.Reflection;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>Does not perform any caching or object lifetime management - everything is transient.</summary>
	internal class ActivatorServiceProvider : IServiceProvider
	{
		public static ActivatorServiceProvider Instance { get; } = new ActivatorServiceProvider();

		private ActivatorServiceProvider()
		{
		}

		public Object GetService( Type serviceType )
		{
			return this.GetService( serviceType: serviceType, args: null );
		}

#pragma warning disable CA1822 // Mark members as static
		public Object GetService( Type serviceType, params Object[] args )
#pragma warning restore CA1822
		{
			return Activator.CreateInstance(
				type                : serviceType,
				bindingAttr         : BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance,
				binder              : null,
				args                : args,
				culture             : null,
				activationAttributes: null
			);
		}
	}

	/// <summary>Always returns null in calls to <see cref="GetService(Type)"/>.</summary>
	internal class NullServiceProvider : IServiceProvider
	{
		public static NullServiceProvider Instance { get; } = new NullServiceProvider();

		public Object GetService(Type serviceType)
		{
			return null;
		}
	}
}
