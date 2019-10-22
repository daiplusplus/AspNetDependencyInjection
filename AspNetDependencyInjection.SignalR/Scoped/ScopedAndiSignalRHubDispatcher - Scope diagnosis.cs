#if NOT_NOW
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>The HubDispatcher manages DI scope over a Hub's lifetime. This HubDispatcher subclass must be registered ONLY with SignalR's DependencyResolver (and NOT in the consumer's DI system) and it MUST be registered as a transient. Weird bugs (without any error messages or output!) happen in SignalR when a HubDispatcher instance is reused.</summary>
	public class ScopedAndiSignalRHubDispatcher : HubDispatcher
    {
		private static readonly AsyncLocal<ScopeWrapper> _asyncLocalScopeRequest   = new AsyncLocal<ScopeWrapper>();
		private static readonly AsyncLocal<ScopeWrapper> _asyncLocalScopeOperation = new AsyncLocal<ScopeWrapper>();

		private readonly ApplicationDependencyInjection di;
		private readonly IServiceProvider               rootServiceProvider;

        public ScopedAndiSignalRHubDispatcher( ApplicationDependencyInjection di, IServiceProvider rootServiceProvider, HubConfiguration configuration ) 
            : base(configuration)
        {
            this.di                  = di                  ?? throw new ArgumentNullException( nameof(di) );
			this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException( nameof(rootServiceProvider) );
        }

		[Conditional( "DEBUG" )]
		private static void Log( String message, [CallerMemberName] String callerName = null )
		{
			ScopeWrapper requestScope = _asyncLocalScopeRequest  .Value;
			ScopeWrapper opScope      = _asyncLocalScopeOperation.Value;

			String requestScopeStr   = requestScope == null ? "null" : "set " + requestScope.Id;
			String operationScopeStr = opScope      == null ? "null" : "set " + opScope     .Id;

			String format = String.Format( CultureInfo.InvariantCulture, "{0} - {1} - Request scope: {2}. Operation scope: {3}.", callerName, message, requestScopeStr, operationScopeStr );

			Debug.WriteLine( format );
		}

		// Originally I thought `ProcessRequest` was invoked for every operation/request and its lifespan was that of the request (while it executes OnConnected, OnReceived, OnDisconnected, and OnReconnected) - but it turns out I was wrong.
		// It depends on the Transport - e.g. longPolling will run `OnReceived` inside `ProcessRequest`, but with WebSockets `OnReceived` is not (but `OnConnected` is).

		public override async Task ProcessRequest( HostContext context )
		{
			Log( "Entered" );

			if( _asyncLocalScopeRequest  .Value != null ) throw new InvalidOperationException( "Expected empty request scope." );
			if( _asyncLocalScopeOperation.Value != null ) throw new InvalidOperationException( "Expected empty operation scope." );

			using( ScopeWrapper sw = new ScopeWrapper( this.rootServiceProvider.CreateScope() ) )
			{
				_asyncLocalScopeRequest.Value = sw;

				Log( "Created request scope: " + sw.Id );

				try
				{
					await base.ProcessRequest( context ).ConfigureAwait(false);
				}
				finally
				{
					if( _asyncLocalScopeRequest.Value != sw ) throw new InvalidOperationException( "Request Scope has changed unexpectedly." );
					_asyncLocalScopeRequest.Value = null;
				}

				Log( "Disposing request scope: " + sw.Id );
			}

			Log( "Leaving" );
		}

		protected override async Task OnConnected( IRequest request, String connectionId )
        {
			Log( "Entered" );

			if( _asyncLocalScopeOperation.Value != null ) throw new InvalidOperationException( "Expected empty operation scope." );

			// `OnConnected` *can* called via ProcessRequest, so a Request Scope may already exist - but it also may not.

			IServiceScope opScope = _asyncLocalScopeRequest.Value?.Scope.ServiceProvider.CreateScope() ?? this.rootServiceProvider.CreateScope();
			using( ScopeWrapper sw = new ScopeWrapper( opScope ) )
			{
				_asyncLocalScopeOperation.Value = sw;

				Log( "Created operation scope: " + sw.Id );

				try
				{
					await base.OnConnected( request, connectionId ).ConfigureAwait(false);
				}
				finally
				{
					if( _asyncLocalScopeOperation.Value != sw ) throw new InvalidOperationException( "Operation Scope has changed unexpectedly." );
					_asyncLocalScopeOperation.Value = null;
				}

				Log( "Disposing operation scope: " + sw.Id );
			}

			Log( "Leaving" );
        }

        protected override async Task OnReceived(IRequest request, String connectionId, String data)
        {
			Log( "Entered" );

			if( _asyncLocalScopeOperation.Value != null ) throw new InvalidOperationException( "Expected empty operation scope." );

			IServiceScope opScope = _asyncLocalScopeRequest.Value?.Scope.ServiceProvider.CreateScope() ?? this.rootServiceProvider.CreateScope();
			using( ScopeWrapper sw = new ScopeWrapper( opScope ) )
			{
				_asyncLocalScopeOperation.Value = sw;

				Log( "Created operation scope: " + sw.Id );

				try
				{
					await base.OnReceived( request, connectionId, data ).ConfigureAwait(false);
				}
				finally
				{
					if( _asyncLocalScopeOperation.Value != sw ) throw new InvalidOperationException( "Operation Scope has changed unexpectedly." );
					_asyncLocalScopeOperation.Value = null;
				}

				Log( "Disposing operation scope: " + sw.Id );
			}

			Log( "Leaving" );
        }

        protected override async Task OnDisconnected(IRequest request, String connectionId, Boolean stopCalled)
        {
			Log( "Entered" );

			if( _asyncLocalScopeOperation.Value != null ) throw new InvalidOperationException( "Expected empty operation scope." );

			IServiceScope opScope = _asyncLocalScopeRequest.Value?.Scope.ServiceProvider.CreateScope() ?? this.rootServiceProvider.CreateScope();
			using( ScopeWrapper sw = new ScopeWrapper( opScope ) )
			{
				_asyncLocalScopeOperation.Value = sw;

				Log( "Created operation scope: " + sw.Id );

				try
				{
					await base.OnDisconnected( request, connectionId, stopCalled ).ConfigureAwait(false);
				}
				finally
				{
					if( _asyncLocalScopeOperation.Value != sw ) throw new InvalidOperationException( "Operation Scope has changed unexpectedly." );
					_asyncLocalScopeOperation.Value = null;
				}

				Log( "Disposing operation scope: " + sw.Id );
			}

			Log( "Leaving" );
        }

        protected override async Task OnReconnected(IRequest request, String connectionId)
        {
			Log( "Entered" );

			if( _asyncLocalScopeOperation.Value != null ) throw new InvalidOperationException( "Expected empty operation scope." );

			IServiceScope opScope = _asyncLocalScopeRequest.Value?.Scope.ServiceProvider.CreateScope() ?? this.rootServiceProvider.CreateScope();
			using( ScopeWrapper sw = new ScopeWrapper( opScope ) )
			{
				_asyncLocalScopeOperation.Value = sw;

				Log( "Created operation scope: " + sw.Id );

				try
				{
					await base.OnReconnected( request, connectionId ).ConfigureAwait(false);
				}
				finally
				{
					if( _asyncLocalScopeOperation.Value != sw ) throw new InvalidOperationException( "Operation Scope has changed unexpectedly." );
					_asyncLocalScopeOperation.Value = null;
				}

				Log( "Disposing operation scope: " + sw.Id );
			}

			Log( "Leaving" );
        }

		/// <summary>Exposes the current <see cref="AsyncLocal{T}"/> <see cref="IServiceScope"/>. Consuming applications generally do not need to use this property. It is exposed to allow consuming applications to manipulate the IServiceScope if absolutely necessary.</summary>
		public static Boolean TryGetScope( out IServiceScope scope )
		{
			scope = _asyncLocalScopeOperation.Value?.Scope ?? _asyncLocalScopeRequest.Value?.Scope;
			return scope != null;
		}

		/// <summary>Exposes the current <see cref="AsyncLocal{T}"/> <see cref="IServiceScope"/>. Consuming applications generally do not need to use this property. It is exposed to allow consuming applications to manipulate the IServiceScope if absolutely necessary.</summary>
		public static IServiceScope RequireScope()
		{
			IServiceScope scope = scope = _asyncLocalScopeOperation.Value?.Scope ?? _asyncLocalScopeRequest.Value?.Scope;
			if( scope == null ) throw new InvalidOperationException( "AsyncLocal Request scope nor Operation scope has not been set." );
			return scope;
		}
    }

	internal class OuterScopeWrapper : IDisposable
	{
		private static AsyncLocal<OuterScopeWrapper> _asyncLocal;

		private static          Int32         _idSeed;
		private        readonly IServiceScope scope;
		private                 Boolean       isDisposed;

		public OuterScopeWrapper( IServiceScope newScope )
		{
			this.Id      = Interlocked.Increment( ref _idSeed );
			this.Created = DateTimeOffset.UtcNow;
			this.scope   = newScope ?? throw new ArgumentNullException( nameof( newScope ) );
		}

		public DateTimeOffset Created { get; }
		public Int32          Id      { get; }

		public IServiceScope Scope
		{
			get
			{
				if( !this.isDisposed )
				{
					return this.scope;
				}
				else
				{
					throw new ObjectDisposedException( "This instance is disposed." );
				}
			}
		}

		public void Dispose()
		{
			this.scope.Dispose();
			this.isDisposed = true;
		}

		public void Attach(  )
		{

		}

		public static OuterScopeWrapper AsyncLocal
		{
			get
			{
				return _asyncLocal.Value;
			}
		}
	}

	internal class ScopeWrapper : IDisposable
	{
		private static          Int32         _idSeed;
		private        readonly IServiceScope scope;
		private                 Boolean       isDisposed;

		public ScopeWrapper( IServiceScope scope )
		{
			this.Id      = Interlocked.Increment( ref _idSeed );
			this.Created = DateTimeOffset.UtcNow;
			this.scope   = scope ?? throw new ArgumentNullException( nameof( scope ) );
		}

		public DateTimeOffset Created { get; }
		public Int32          Id      { get; }

		public IServiceScope Scope
		{
			get
			{
				if( !this.isDisposed )
				{
					return this.scope;
				}
				else
				{
					throw new ObjectDisposedException( "This instance is disposed." );
				}
			}
		}

		public void Dispose()
		{
			this.scope.Dispose();
			this.isDisposed = true;
		}
	}
}
#endif