using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleWebApplication
{
	public class SingletonService
	{
		private static Int32 _idSeed;

		public SingletonService()
		{
			this.Id = System.Threading.Interlocked.Increment( ref _idSeed );
		}

		public Int32 Id { get; }
	}
}