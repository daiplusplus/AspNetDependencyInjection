using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SampleWebApplication
{
	public interface IExampleRequestLifelongService : IDisposable
	{
		void DoSomething();

		Int32 ConstructorCallCount { get; }

		Int32 InstanceNumber { get; }

		Int32 InstanceDoSomethingCallCount { get; }

		Int32 SharedDoSomethingCallCount { get; }
	}

	public sealed class ExampleRequestLifelongService : IExampleRequestLifelongService
	{
		private static Int32 _ctorCallCount;
		private static Int32 _sharedCallCount;
		private static Int32 _disposedCount;

		private readonly Int32 instanceNumber;
		private          Int32 callCount;

		private          Boolean isDisposed;

		public Int32 ConstructorCallCount => _ctorCallCount;

		public Int32 InstanceDoSomethingCallCount => this.callCount;

		public Int32 SharedDoSomethingCallCount => _sharedCallCount;

		public Int32 InstanceNumber => this.instanceNumber;

		public Int32 DisposedCount => _disposedCount;

		public ExampleRequestLifelongService()
		{
			this.instanceNumber = Interlocked.Increment( ref _ctorCallCount );

			Task verifyTask = Task.Run( this.VerifyIsDisposedAsync );
		}

		public void DoSomething()
		{
			this.callCount++;

			Interlocked.Increment( ref _sharedCallCount );
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations" )]
		public void Dispose()
		{
			if( this.isDisposed )
			{
				// This exception is only thrown for demonstration purposes. Never throw an exception from a Dispose method in production code!
				throw new InvalidOperationException( "This instance is already disposed." );
			}
			else
			{
				this.isDisposed = true;

				Interlocked.Increment( ref _disposedCount );
			}
		}

		private async Task VerifyIsDisposedAsync()
		{
			await Task.Delay( millisecondsDelay: 10 * 1000 ); // Wait 10 seconds after the instance has been constructed (assuming no HTTP request takes 10 seconds to run).

			if( !this.isDisposed )
			{
				throw new InvalidOperationException( "This instance was not disposed after its parent HTTP request had (presumably) completed." );
			}
		}

		public override String ToString()
		{
			const String fmt = "Static: ( Ctor calls: {0}, DoSomething calls: {1}, Disposed count: {2} ), Instance: ( Instance number: {3}, DoSomething calls: {4} )";
			return String.Format( CultureInfo.CurrentCulture, fmt, this.ConstructorCallCount, this.SharedDoSomethingCallCount, this.DisposedCount, this.InstanceNumber, this.InstanceDoSomethingCallCount );
		}
	}
}