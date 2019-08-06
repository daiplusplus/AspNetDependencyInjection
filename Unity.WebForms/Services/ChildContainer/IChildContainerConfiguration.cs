using System;
using System.Web;

namespace Unity.WebForms
{
	/// <summary>Registered implementations in the root container will be invoked to register types exclusively in the per-HTTP Request Child Container.</summary>
	public interface IChildContainerConfiguration
	{
		// TODO: How expensive is this? Isn't using a single registration in the root but with a custom lifetime manager better?
		// I'm using this to override logging for requests (so it includes HTTP Request details in logged messags). Is this the best way of doing it?

		/// <summary>Called to register types with the newly-created child container. This method is called inside <see cref="UnityHttpModule.OnContextBeginRequest(object, EventArgs)"/>.</summary>
		/// <param name="httpContext">The <see cref="HttpContextBase"/> associated with the <paramref name="childContainer"/>.</param>
		/// <param name="childContainer">The HTTP request-specific container to be configured.</param>
		void ConfigureRequestContainer( HttpContextBase httpContext, IUnityContainer childContainer );
	}
}
