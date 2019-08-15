using System;
using System.Web;

using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection
{
	/// <summary>Controls for <see cref="ApplicationDependencyInjection"/>.</summary>
	public class ApplicationDependencyInjectionConfiguration
	{
		/// <summary>This property is <c>true</c> by default. When <c>true</c>, a new <see cref="IServiceScope"/> will be created for each <see cref="HttpContext"/> instance (i.e. for each new HTTP request/response lifecycle). When <c>false</c> all <see cref="HttpContext"/> instances will use the parent <see cref="IServiceProvider"/>, which may be the root service-provider or may be from the parent <see cref="HttpApplication"/> (when <see cref="UseHttpApplicationScopes"/> is <c>true</c>.</summary>
		public Boolean UseRequestScopes         { get; set; } = true;

		/// <summary>This property is <c>false</c> by default. When <c>true</c>, a new <see cref="IServiceScope"/> will be created for each <see cref="HttpApplication"/> instance: this may be desirable if you have services that are not thread-safe. When <c>false</c> all <see cref="HttpApplication"/> instances will use the root <see cref="IServiceProvider"/> directly.</summary>
		public Boolean UseHttpApplicationScopes { get; set; } = false;
	}
}
