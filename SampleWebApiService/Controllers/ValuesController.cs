using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using SampleWebApiService.SampleServices;

namespace SampleWebApiService.Controllers
{
	public class ValuesController : ApiController
	{
		private readonly ISampleSingletonService  sgl;
		private readonly ISampleScopedService1    sc1;
		private readonly ISampleScopedService2    sc2;
		private readonly ISampleTransientService1 st1;
		private readonly ISampleTransientService2 st2;

		public ValuesController(
			ISampleSingletonService  sgl,
			ISampleScopedService1    sc1,
			ISampleScopedService2    sc2,
			ISampleTransientService1 st1,
			ISampleTransientService2 st2
		)
		{
			this.sgl = sgl ?? throw new ArgumentNullException( nameof( sgl ) );
			this.sc1 = sc1 ?? throw new ArgumentNullException( nameof( sc1 ) );
			this.sc2 = sc2 ?? throw new ArgumentNullException( nameof( sc2 ) );
			this.st1 = st1 ?? throw new ArgumentNullException( nameof( st1 ) );
			this.st2 = st2 ?? throw new ArgumentNullException( nameof( st2 ) );
		}

		[HttpGet]
		[Route("")]
		public String Get()
		{
			const String FMT =
				nameof(ISampleSingletonService)  + " " + nameof(this.sgl) + ": Instance: {0}\r\n" +
				nameof(ISampleScopedService1)    + " " + nameof(this.sc1) + ": Instance: {1}\r\n" +
				nameof(ISampleScopedService2)    + " " + nameof(this.sc2) + ": Instance: {2}\r\n" +
				nameof(ISampleTransientService1) + " " + nameof(this.st1) + ": Instance: {3}\r\n" +
				nameof(ISampleTransientService2) + " " + nameof(this.st2) + ": Instance: {4}\r\n";

			return String.Format(
				provider: CultureInfo.InvariantCulture,
				format  : FMT,
				this.sgl.InstanceId,
				this.sc1.InstanceId,
				this.sc2.InstanceId,
				this.st1.InstanceId,
				this.st2.InstanceId
			);
		}
	}
}
