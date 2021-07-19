using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace SampleWebApiService.SampleServices
{
	public interface ISampleSingletonService
	{
		Int32 InstanceId { get; }
	}

	public class DefaultSingletonService : ISampleSingletonService
	{
		private static Int32 _idSeed = 0;

		public DefaultSingletonService()
		{
			this.InstanceId = Interlocked.Increment( ref _idSeed );
		}

		public Int32 InstanceId { get; }
	}
}
