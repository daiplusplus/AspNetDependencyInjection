using System.Web;

namespace Unity.WebForms
{
	internal class DefaultChildContainerConfiguration : IChildContainerConfiguration
	{
		public void ConfigureRequestContainer( HttpContextBase httpContext, IUnityContainer childContainer )
		{
			// NOOP.
		}
	}
}
