using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

		private static Int32 _sessionCounter = 0;

		protected void Session_Start( Object sender, EventArgs e ) 
		{
			Debug.WriteLine( nameof(Session_Start) + " called." );

			this.Session[ "counter" ] = Interlocked.Increment( ref _sessionCounter );
		}
	}
}
