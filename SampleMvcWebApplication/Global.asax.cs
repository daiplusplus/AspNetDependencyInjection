using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SampleMvcWebApplication
{
	public class MvcApplication : HttpApplication
	{
		protected void Application_Start()
		{
			SampleApplicationStart.GlobalAsaxApplicationStart( this );

			AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisterGlobalFilters( GlobalFilters.Filters );
			RouteConfig.RegisterRoutes( RouteTable.Routes );
		}
	}
}
