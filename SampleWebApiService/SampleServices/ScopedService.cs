using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace SampleWebApiService.SampleServices
{
	public interface ISampleScopedService1
	{
		Int32 InstanceId { get; }
	}

	public class DefaultScopedService1 : ISampleScopedService1
	{
		private static Int32 _idSeed = 0;

		public DefaultScopedService1()
		{
			this.InstanceId = Interlocked.Increment( ref _idSeed );
		}

		public Int32 InstanceId { get; }
	}

	//

	public interface ISampleScopedService2
	{
		Int32 InstanceId { get; }
	}

	public class DefaultScopedService2 : ISampleScopedService2
	{
		private static Int32 _idSeed = 0;

		public DefaultScopedService2( ISampleSingletonService singletonService, ISampleTransientService1 transient1, ISampleTransientService2 transient2 )
		{
			this.InstanceId       = Interlocked.Increment( ref _idSeed );

			this.SingletonService = singletonService ?? throw new ArgumentNullException( nameof( singletonService ) );
			this.Transient1       = transient1 ?? throw new ArgumentNullException( nameof( transient1 ) );
			this.Transient2       = transient2 ?? throw new ArgumentNullException( nameof( transient2 ) );
		}

		public Int32 InstanceId { get; }

		public ISampleSingletonService  SingletonService { get; }
		public ISampleTransientService1 Transient1       { get; }
		public ISampleTransientService2 Transient2       { get; }
	}
}
