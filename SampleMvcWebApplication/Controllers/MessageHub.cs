using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AspNetDependencyInjection;
using Microsoft.AspNet.SignalR;

namespace SampleMvcWebApplication.Controllers
{
	public interface IMessagesHubClient
	{
		void addChatMessageToPage( String name, String text );
	}

//	public interface IMessagesHubServer
//	{
//		void SendMessage( String name, String text );
//	}

	public class MessagesHub : Hub<IMessagesHubClient>//, IMessagesHubServer
	{
		private readonly IUserIdProvider userIdProvider;
		private readonly IWebConfiguration injectedConfig;

		public MessagesHub( IUserIdProvider userIdProvider, IWebConfiguration injected )
		{
			this.userIdProvider = userIdProvider ?? throw new ArgumentNullException( nameof( userIdProvider ) );
			this.injectedConfig = injected       ?? throw new ArgumentNullException(nameof(injected));

			System.Diagnostics.Debug.WriteLine( "MessagesHub created." );
		}

		public override Task OnConnected()
		{
			return base.OnConnected();
		}

		public void NewChatMessage( String name, String text )
		{
			String newName = name + "(" + this.userIdProvider.GetUserId( this.Context.Request ) + ")";
			String newText = text + this.injectedConfig.RequireAppSetting("messagesHubSuffix");

			this.Clients.All.addChatMessageToPage( newName, newText );
		}

		public void Started()
		{
			this.Clients.All.addChatMessageToPage( name: nameof(MessagesHub), text: this.Context.ConnectionId + " has started." );
		}
	}
}

namespace SampleMvcWebApplication
{
	public class SampleUserIdProvider : IUserIdProvider
	{
		const String cookieName = "ASP.NET_SessionId";

		public String GetUserId( IRequest request )
		{
			if( request.Cookies.TryGetValue( cookieName, out Cookie value ) )
			{
				return value.Value;
			}
			else
			{
				return "No Session cookie";
			}
		}
	}
}