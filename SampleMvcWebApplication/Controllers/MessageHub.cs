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
		private readonly IWebConfiguration injectedConfig;

		public MessagesHub( IWebConfiguration injected )
		{
			System.Diagnostics.Debug.WriteLine( "MessagesHub created." );
			this.injectedConfig = injected ?? throw new ArgumentNullException(nameof(injected));
		}

		public override Task OnConnected()
		{
			return base.OnConnected();
		}

		public void NewChatMessage( String name, String text )
		{
			String newText = text + this.injectedConfig.RequireAppSetting("messagesHubSuffix");

			this.Clients.All.addChatMessageToPage( name, newText );
		}

		public void Started()
		{
			this.Clients.All.addChatMessageToPage( name: nameof(MessagesHub), text: this.Context.ConnectionId + " has started." );
		}
	}
}