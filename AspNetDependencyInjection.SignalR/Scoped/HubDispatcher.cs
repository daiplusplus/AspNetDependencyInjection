using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>The HubDispatcher manages DI scope over a Hub's lifetime.</summary>
	public class DependencyInjectionSignalRHubDispatcher : HubDispatcher
    {
		private static readonly AsyncLocal<IServiceScope> _asyncLocalScope = new AsyncLocal<IServiceScope>();

		private readonly ApplicationDependencyInjection di;
		private readonly IServiceProvider               rootServiceProvider;

        public DependencyInjectionSignalRHubDispatcher( ApplicationDependencyInjection di, IServiceProvider rootServiceProvider, HubConfiguration configuration ) 
            : base(configuration)
        {
            this.di                  = di                  ?? throw new ArgumentNullException( nameof(di) );
			this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException( nameof(rootServiceProvider) );
        }

        protected override async Task OnConnected( IRequest request, String connectionId )
        {
			using( IServiceScope scope = this.rootServiceProvider.CreateScope() )
			{
				SetScope( scope );

				try
				{
					await base.OnConnected( request, connectionId ).ConfigureAwait(false);
				}
				finally
				{
					ReleaseScope( scope );
				}
			}
        }

        protected override async Task OnReceived(IRequest request, String connectionId, String data)
        {
			using( IServiceScope scope = this.rootServiceProvider.CreateScope() )
			{
				SetScope( scope );

				try
				{
					await base.OnReceived( request, connectionId, data ).ConfigureAwait(false);
				}
				finally
				{
					ReleaseScope( scope );
				}
			}
        }

        protected override async Task OnDisconnected(IRequest request, String connectionId, Boolean stopCalled)
        {
			using( IServiceScope scope = this.rootServiceProvider.CreateScope() )
			{
				SetScope( scope );

				try
				{
					await base.OnDisconnected( request, connectionId, stopCalled ).ConfigureAwait(false);
				}
				finally
				{
					ReleaseScope( scope );
				}
			}
        }

        protected override async Task OnReconnected(IRequest request, String connectionId)
        {
			using( IServiceScope scope = this.rootServiceProvider.CreateScope() )
			{
				SetScope( scope );

				try
				{
					await base.OnReconnected( request, connectionId ).ConfigureAwait(false);
				}
				finally
				{
					ReleaseScope( scope );
				}
			}
        }

		private sealed class AsyncLocalScope : IDisposable
		{
			private readonly IServiceScope scope;

			public static AsyncLocalScope Create( IServiceProvider rootServiceProvider )
			{
				if( rootServiceProvider == null ) throw new ArgumentNullException(nameof(rootServiceProvider));

				IServiceScope scope = rootServiceProvider.CreateScope();
				return new AsyncLocalScope( scope );
			}

			private AsyncLocalScope( IServiceScope scope )
			{
				this.scope = scope ?? throw new ArgumentNullException( nameof( scope ) );

				SetScope( scope );
			}

			void IDisposable.Dispose()
			{
				ReleaseScope( this.scope );
			}
		}

		private static void SetScope( IServiceScope scope )
		{
			if( scope == null ) throw new ArgumentNullException(nameof(scope));

			if( _asyncLocalScope.Value != null )
			{
				throw new InvalidOperationException( "Scope has already been set for this Async context." );
			}
			else
			{
				_asyncLocalScope.Value = scope;
			}
		}

		private static void ReleaseScope( IServiceScope scope )
		{
			if( scope == null ) throw new ArgumentNullException(nameof(scope));

			if( Object.ReferenceEquals( _asyncLocalScope.Value, scope ) )
			{
				_asyncLocalScope.Value = null;
			}
			else if( _asyncLocalScope.Value == null )
			{
				throw new InvalidOperationException( "Current Async context has a null Scope object." );
			}
			else
			{
				throw new InvalidOperationException( "Current Async context has an unexpected Scope object set." );
			}
		}

		public static IServiceScope GetScope()
		{
			if( _asyncLocalScope.Value == null )
			{
				throw new InvalidOperationException( "Current Async context has a null Scope object." );
			}
			else
			{
				return _asyncLocalScope.Value;
			}
		}
    }
}
