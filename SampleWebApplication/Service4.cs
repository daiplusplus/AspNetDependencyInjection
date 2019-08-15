using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AspNetDependencyInjection;

namespace SampleWebApplication
{
	public class Service4
	{
		private readonly IExampleRequestLifelongService service;
		private readonly IHttpContextAccessor httpContextAccessor;

		public Service4( IExampleRequestLifelongService service3, IHttpContextAccessor httpContextAccessor )
		{
			this.service = service3 ?? throw new ArgumentNullException(nameof(service3));
			this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
		}

		public void DoSomething()
		{
			this.httpContextAccessor.HttpContext.Response.AddHeader( "X-HttpContextAccessor-Works", "1" );

			this.service.DoSomething();
		}
	}
}