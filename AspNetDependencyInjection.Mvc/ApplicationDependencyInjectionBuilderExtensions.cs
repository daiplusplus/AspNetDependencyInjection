using System;

using AspNetDependencyInjection.Internal;

namespace AspNetDependencyInjection
{
	public static class ApplicationDependencyInjectionBuilderExtensions
	{
		public static ApplicationDependencyInjectionBuilder AddMvcDependencyResolver( this ApplicationDependencyInjectionBuilder builder )
		{
			if( builder == null ) throw new ArgumentNullException(nameof(builder));

			return builder
				.AddClient( di => new DependencyInjectionMvcDependencyResolver( di ) );
		}
	}
}
