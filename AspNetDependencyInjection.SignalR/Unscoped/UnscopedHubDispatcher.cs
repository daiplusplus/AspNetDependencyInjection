using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>The HubDispatcher manages DI scope over a Hub's lifetime.</summary>
	public class UnscopedHubDispatcher : HubDispatcher
    {
		private static readonly AsyncLocal<IServiceScope> _asyncLocalScope = new AsyncLocal<IServiceScope>();

		private readonly UnscopedAspNetDiSignalRDependencyResolver dr;

        public UnscopedHubDispatcher( UnscopedAspNetDiSignalRDependencyResolver dr, HubConfiguration configuration ) 
            : base( configuration )
        {
            this.dr = dr ?? throw new ArgumentNullException(nameof(dr));
        }

		protected override Task OnConnected( IRequest request, String connectionId )
		{
			Debug.WriteLine( nameof(UnscopedHubDispatcher) + "." + nameof(OnConnected) );

			return base.OnConnected( request, connectionId );
		}

		protected override Task OnDisconnected( IRequest request, String connectionId, Boolean stopCalled )
		{
			Debug.WriteLine( nameof(UnscopedHubDispatcher) + "." + nameof(OnDisconnected) );

			return base.OnDisconnected( request, connectionId, stopCalled );
		}

		protected override Task OnReceived( IRequest request, String connectionId, String data )
		{
			Debug.WriteLine( nameof(UnscopedHubDispatcher) + "." + nameof(OnReceived) );

			return base.OnReceived( request, connectionId, data );
		}

		protected override Task OnReconnected( IRequest request, String connectionId )
		{
			Debug.WriteLine( nameof(UnscopedHubDispatcher) + "." + nameof(OnReconnected) );

			return base.OnReconnected( request, connectionId );
		}

		protected override IList<String> OnRejoiningGroups( IRequest request, IList<String> groups, String connectionId )
		{
			Debug.WriteLine( nameof(UnscopedHubDispatcher) + "." + nameof(OnRejoiningGroups) );

			return base.OnRejoiningGroups( request, groups, connectionId );
		}

		protected override Boolean AuthorizeRequest( IRequest request )
		{
			Debug.WriteLine( nameof(UnscopedHubDispatcher) + "." + nameof(AuthorizeRequest) );

			return base.AuthorizeRequest( request );
		}

		protected override IList<String> GetSignals( String userId, String connectionId )
		{
			Debug.WriteLine( nameof(UnscopedHubDispatcher) + "." + nameof(GetSignals) );

			return base.GetSignals( userId, connectionId );
		}

		public override void Initialize( IDependencyResolver resolver )
		{
			Debug.WriteLine( nameof(UnscopedHubDispatcher) + "." + nameof(Initialize) );

			base.Initialize( resolver );
		}

		public override Task ProcessRequest( HostContext context )
		{
			Debug.WriteLine( nameof(UnscopedHubDispatcher) + "." + nameof(ProcessRequest) );

			return base.ProcessRequest( context );
		}

		protected override Boolean TryGetConnectionId( HostContext context, String connectionToken, out String connectionId, out String message, out Int32 statusCode )
		{
			Debug.WriteLine( nameof(UnscopedHubDispatcher) + "." + nameof(TryGetConnectionId) );

			return base.TryGetConnectionId( context, connectionToken, out connectionId, out message, out statusCode );
		}
	}
}
