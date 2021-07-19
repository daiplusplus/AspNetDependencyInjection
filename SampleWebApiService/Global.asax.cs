using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace SampleWebApiService
{
	public class WebApiApplication : HttpApplication
	{
		protected void Application_Start()
		{
			GlobalConfiguration.Configure( ConfigureSelf );
		}

		public static void ConfigureSelf( HttpConfiguration config )
		{
			config.MapHttpAttributeRoutes();
		}
	}
}
