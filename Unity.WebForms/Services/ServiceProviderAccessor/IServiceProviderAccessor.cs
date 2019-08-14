using System;

namespace Unity.WebForms.Services
{
	/// <summary>Allows objects to have a dependency on the root <see cref="IServiceProvider"/> in the application.</summary>
	public interface IServiceProviderAccessor
	{
		/// <summary>Gets the root <see cref="IServiceProvider"/> in this AppDomain. It is managed by <see cref="WebFormsUnityContainerOwner"/>.</summary>
		IServiceProvider RootServiceProvider { get; }
	}

	internal class DefaultServiceProviderAccessor : IServiceProviderAccessor
	{
		public DefaultServiceProviderAccessor( IServiceProvider serviceProvider )
		{
			this.RootServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public IServiceProvider RootServiceProvider { get; }
	}
}

