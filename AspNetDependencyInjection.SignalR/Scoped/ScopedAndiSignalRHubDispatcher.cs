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
		private static readonly AsyncLocal<IServiceScope> _asyncLocalScope = new AsyncLocal<IServiceScope>();

		private readonly ApplicationDependencyInjection di;
		private readonly IServiceProvider               rootServiceProvider;

		/// <summary>Constructor.</summary>
        public ScopedAndiSignalRHubDispatcher( ApplicationDependencyInjection di, IServiceProvider rootServiceProvider, HubConfiguration configuration ) 
            : base( configuration )
        {
            this.di                  = di                  ?? throw new ArgumentNullException( nameof(di) );
			this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException( nameof(rootServiceProvider) );
        }

		// Originally I thought `ProcessRequest` was invoked for every operation/request and its lifespan was that of the request (while it executes OnConnected, OnReceived, OnDisconnected, and OnReconnected) - but it turns out I was wrong.
		// It depends on the Transport - e.g. longPolling will run `OnReceived` inside `ProcessRequest`, but with WebSockets `OnReceived` is not (but `OnConnected` is).

		/// <summary>Internal SignalR event. A new <see cref="IServiceScope"/> is always created. If another scope is already active then an <see cref="InvalidOperationException"/> is thrown (this should never happen).</summary>
		public override async Task ProcessRequest( HostContext context )
		{
			if( _asyncLocalScope.Value != null ) throw new InvalidOperationException( "Expected empty request scope." );

			using( IServiceScope scope = this.rootServiceProvider.CreateScope() )
			{
				_asyncLocalScope.Value = scope;

				try
				{
					await base.ProcessRequest( context ).ConfigureAwait(false);
				}
				finally
				{
					if( _asyncLocalScope.Value != scope ) throw new InvalidOperationException( "Request Scope has changed unexpectedly." );
					_asyncLocalScope.Value = null;
				}
			}
		}

		/// <summary>Internal SignalR event. A new <see cref="IServiceScope"/> is created if not is not already active.</summary>
		protected override async Task OnConnected( IRequest request, String connectionId )
        {
			if( _asyncLocalScope.Value == null )
			{
				using( IServiceScope scope = this.rootServiceProvider.CreateScope() )
				{
					_asyncLocalScope.Value = scope;

					try
					{
						await base.OnConnected( request, connectionId ).ConfigureAwait(false);
					}
					finally
					{
						if( _asyncLocalScope.Value != scope ) throw new InvalidOperationException( "Request Scope has changed unexpectedly." );
						_asyncLocalScope.Value = null;
					}
				}
			}
			else
			{
				await base.OnConnected( request, connectionId ).ConfigureAwait(false);
			}
        }

		/// <summary>Internal SignalR event. A new <see cref="IServiceScope"/> is created if not is not already active.</summary>
        protected override async Task OnReceived(IRequest request, String connectionId, String data)
        {
			if( _asyncLocalScope.Value == null )
			{
				using( IServiceScope scope = this.rootServiceProvider.CreateScope() )
				{
					_asyncLocalScope.Value = scope;

					try
					{
						await base.OnReceived( request, connectionId, data ).ConfigureAwait(false);
					}
					finally
					{
						if( _asyncLocalScope.Value != scope ) throw new InvalidOperationException( "Request Scope has changed unexpectedly." );
						_asyncLocalScope.Value = null;
					}
				}
			}
			else
			{
				await base.OnReceived( request, connectionId, data ).ConfigureAwait(false);
			}
        }

		/// <summary>Internal SignalR event. A new <see cref="IServiceScope"/> is created if not is not already active.</summary>
        protected override async Task OnDisconnected(IRequest request, String connectionId, Boolean stopCalled)
        {
			if( _asyncLocalScope.Value == null )
			{
				using( IServiceScope scope = this.rootServiceProvider.CreateScope() )
				{
					_asyncLocalScope.Value = scope;

					try
					{
						await base.OnDisconnected( request, connectionId, stopCalled ).ConfigureAwait(false);
					}
					finally
					{
						if( _asyncLocalScope.Value != scope ) throw new InvalidOperationException( "Request Scope has changed unexpectedly." );
						_asyncLocalScope.Value = null;
					}
				}
			}
			else
			{
				await base.OnDisconnected( request, connectionId, stopCalled ).ConfigureAwait(false);
			}
        }

		/// <summary>Internal SignalR event. A new <see cref="IServiceScope"/> is created if not is not already active.</summary>
        protected override async Task OnReconnected(IRequest request, String connectionId)
        {
			if( _asyncLocalScope.Value == null )
			{
				using( IServiceScope scope = this.rootServiceProvider.CreateScope() )
				{
					_asyncLocalScope.Value = scope;

					try
					{
						await base.OnReconnected( request, connectionId ).ConfigureAwait(false);
					}
					finally
					{
						if( _asyncLocalScope.Value != scope ) throw new InvalidOperationException( "Request Scope has changed unexpectedly." );
						_asyncLocalScope.Value = null;
					}
				}
			}
			else
			{
				await base.OnReconnected( request, connectionId ).ConfigureAwait(false);
			}
        }

		/// <summary>Exposes the current <see cref="AsyncLocal{T}"/> <see cref="IServiceScope"/>. Consuming applications generally do not need to use this property. It is exposed to allow consuming applications to manipulate the IServiceScope if absolutely necessary.</summary>
		public static Boolean TryGetScope( out IServiceScope scope )
		{
			scope = _asyncLocalScope.Value;
			return scope != null;
		}

		/// <summary>Exposes the current <see cref="AsyncLocal{T}"/> <see cref="IServiceScope"/>. Consuming applications generally do not need to use this property. It is exposed to allow consuming applications to manipulate the IServiceScope if absolutely necessary.</summary>
		public static IServiceScope RequireScope()
		{
			IServiceScope scope = scope = _asyncLocalScope.Value;
			if( scope == null ) throw new InvalidOperationException( "AsyncLocal Request scope nor Operation scope has not been set." );
			return scope;
		}
    }
}
