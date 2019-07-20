using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleWebApplication
{
	public class Service4
	{
		private readonly IExampleRequestLifelongService service;

		public Service4( IExampleRequestLifelongService service3 )
		{
			this.service = service3 ?? throw new ArgumentNullException(nameof(service3));
		}

		public void DoSomething()
		{
			this.service.DoSomething();
		}
	}
}