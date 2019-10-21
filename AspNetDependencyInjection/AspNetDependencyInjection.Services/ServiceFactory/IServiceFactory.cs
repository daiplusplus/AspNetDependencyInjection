using System;

using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection
{
	/// <summary>An interface for service factories to implement for use with <see cref="ServiceCollectionExtensions.AddScopedWithFactory{TService, TServiceFactory}(IServiceCollection)"/>.</summary>
	/// <typeparam name="TService">The type of service that implementations will produce.</typeparam>
	public interface IServiceFactory<out TService>
	{
		/// <summary>Creates a new instance of <typeparamref name="TService"/>.</summary>
		TService CreateInstance();
	}

	public static partial class ServiceCollectionExtensions
	{
		/// <summary>Convenience method that registers <typeparamref name="TServiceFactory"/> twice: first as <c>AddSingleton&lt;TServiceFactory&gt;</c> and again as <c>AddSingleton&lt;IServiceFactory&lt;TService&gt;,TServiceFactory&gt;</c>, then and registers <typeparamref name="TService"/> as a Scoped service that uses <typeparamref name="TServiceFactory"/> as a factory.</summary>
		public static IServiceCollection AddScopedWithFactory<TService,TServiceFactory>( this IServiceCollection services )
			where TService        : class
			where TServiceFactory : class, IServiceFactory<TService>
		{
			return services
				.AddSingleton<TServiceFactory>()
				.AddSingleton<IServiceFactory<TService>,TServiceFactory>()
				.AddScoped<TService>( implementationFactory: sp => sp.GetRequiredService<TServiceFactory>().CreateInstance() );
		}
	}
}
