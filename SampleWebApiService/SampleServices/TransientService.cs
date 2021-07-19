using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace SampleWebApiService.SampleServices
{
	public interface ISampleTransientService1
	{
		Int32 InstanceId { get; }
	}

	public class DefaultTransientService1 : ISampleTransientService1
	{
		private static Int32 _idSeed = 0;

		public DefaultTransientService1()
		{
			this.InstanceId = Interlocked.Increment( ref _idSeed );
		}

		public Int32 InstanceId { get; }
	}

	//

	public interface ISampleTransientService2
	{
		Int32 InstanceId { get; }
	}

	public class DefaultTransientService2 : ISampleTransientService2
	{
		private static Int32 _idSeed = 0;

		public DefaultTransientService2( ISampleSingletonService singleton )
		{
			this.InstanceId = Interlocked.Increment( ref _idSeed );

			this.SingletonDependency = singleton;
		}

		public Int32 InstanceId { get; }

		public ISampleSingletonService SingletonDependency { get; }
	}
}
