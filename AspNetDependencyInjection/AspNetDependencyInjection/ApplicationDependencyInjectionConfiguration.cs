using System;
using System.Collections.Generic;
using System.Web;

using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection
{
	/// <summary>Controls for <see cref="ApplicationDependencyInjection"/>.</summary>
	public class ApplicationDependencyInjectionConfiguration
	{
		/// <summary>
		/// This property is <c><see langword="true"/></c> by default.<br />
		/// When <c><see langword="true"/></c>, a new <see cref="IServiceScope"/> will be created for each <see cref="HttpContext"/> instance (i.e. for each new HTTP request/response lifecycle).<br />
		/// When <c><see langword="false"/></c> all <see cref="HttpContext"/> instances will use the parent <see cref="IServiceProvider"/>, which may be the root service-provider or may be from the parent <see cref="HttpApplication"/> (when <see cref="UseHttpApplicationScopes"/> is <c>true</c>.
		/// </summary>
		public Boolean UseRequestScopes         { get; set; } = true;

		/// <summary>This property is <c><see langword="false"/></c> by default.<br />
		/// When <c><see langword="true"/></c>, a new <see cref="IServiceScope"/> will be created for each <see cref="HttpApplication"/> instance: this may be desirable if you have services that are not thread-safe.<br />
		/// When <c><see langword="false"/></c> all <see cref="HttpApplication"/> instances will use the root <see cref="IServiceProvider"/> directly. For best performance, your <c>Global.asax</c> class should implement <see cref="IScopedHttpApplication"/> to allow for storage of the <see cref="IServiceScope"/>, otherwise reflection will be used to store it inside a Hashtable inside each <see cref="HttpApplication"/> instance.</summary>
		public Boolean UseHttpApplicationScopes { get; set; } = false;

		/// <summary>Returns an instance of <see cref="ImmutableApplicationDependencyInjectionConfiguration"/> with a copy of all members of this object. May return a subclass in an overriden implementation.</summary>
		protected internal virtual ImmutableApplicationDependencyInjectionConfiguration ToImmutable()
		{
			return new ImmutableApplicationDependencyInjectionConfiguration( useRequestScopes: this.UseRequestScopes, useHttpApplicationScopes: this.UseHttpApplicationScopes );
		}
	}

	/// <summary>Immutable copy of <see cref="ApplicationDependencyInjectionConfiguration"/>.</summary>
	public class ImmutableApplicationDependencyInjectionConfiguration
	{
		/// <summary>Constructor.</summary>
		public ImmutableApplicationDependencyInjectionConfiguration( Boolean useRequestScopes, Boolean useHttpApplicationScopes )
		{
			this.UseRequestScopes         = useRequestScopes;
			this.UseHttpApplicationScopes = useHttpApplicationScopes;
		}

		/// <summary>See <see cref="ApplicationDependencyInjectionConfiguration.UseRequestScopes"/>.</summary>
		public Boolean UseRequestScopes         { get; }

		/// <summary>See <see cref="ApplicationDependencyInjectionConfiguration.UseHttpApplicationScopes"/>.</summary>
		public Boolean UseHttpApplicationScopes { get; }
	}
}
