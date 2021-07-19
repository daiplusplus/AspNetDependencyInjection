using System.Web.Http;
using System.Web.Routing;

namespace SampleWebApiService
{
	public class RouteConfig
	{
		public static void RegisterRoutes( RouteCollection routes )
		{
			routes.Ignore( "{resource}.axd/{*pathInfo}" );

			routes.MapHttpRoute(
				name: "API Default",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);
		}
	}
}
