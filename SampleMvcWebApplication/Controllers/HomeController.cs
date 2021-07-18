using System;
using System.Net;
using System.Web.Mvc;

using Microsoft.AspNet.SignalR;

namespace SampleMvcWebApplication.Controllers
{
	public class HomeController : Controller
	{
		public const String Name = "Home";

		public ActionResult Index()
		{
			String str = this.Resolver.GetService<String>();

			IUserIdProvider userIdProvider = this.Resolver.GetService<IUserIdProvider>();

			return this.View();
		}

		public ActionResult About()
		{
			this.ViewBag.Message = "Your application description page.";

			return this.View();
		}

		public ActionResult Contact()
		{
			this.ViewBag.Message = "Your contact page.";

			return this.View();
		}

		[HttpPost]
		public ActionResult SendMessage302( MvcSendMessageDto model )
		{
			IHubContext<IMessagesHubClient> hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagesHub,IMessagesHubClient>();
			hubContext.Clients.All.addChatMessageToPage( model.Name, model.Text );

			return new RedirectResult( url: "/", permanent: false );
		}

		[HttpPost]
		public ActionResult SendMessage204( MvcSendMessageDto model )
		{
			IHubContext<IMessagesHubClient> hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagesHub,IMessagesHubClient>();
			hubContext.Clients.All.addChatMessageToPage( model.Name, model.Text );

			return new HttpStatusCodeResult( HttpStatusCode.NoContent );
		}
	}

	public class MvcSendMessageDto
	{
		public String Name { get; set; }

		public String Text { get; set; }
	}

}
