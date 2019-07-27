using System;

namespace Unity.WebForms
{
	/// <summary>An interface for service factory types to implement. For use in conjunction with <see cref="UnityContainerExtensions.RegisterRequestFactory{T, TFactory}(IUnityContainer)"/>.</summary>
	/// <typeparam name="T">The type of service that this factory will instantiate.</typeparam>
	public interface IServiceFactory<T>
	{
		/// <summary>Creates a new instance of <typeparamref name="T"/>.</summary>
		T CreateInstance();
	}

	/// <summary>An interface for service factory types to implement. For use in conjunction with <see cref="UnityContainerExtensions.RegisterRequestFactory{T, TNamedFactory}(IUnityContainer, string)"/> and related overloads.</summary>
	/// <typeparam name="T">The type of service that this factory will instantiate.</typeparam>
	public interface INamedServiceFactory<T>
	{
		/// <summary>Creates a new instance of <typeparamref name="T"/>.</summary>
		T CreateInstance(String name);
	}

	// TODO: Can we get this to play-nice with Microsoft.Extensions.Configuration.Options?
}
