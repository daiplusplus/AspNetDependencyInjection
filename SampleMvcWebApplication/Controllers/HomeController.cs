using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;

namespace SampleMvcWebApplication.Controllers
{
	public class HomeController : Controller
	{
		public const String Name = "Home";

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}

		[HttpPost]
		public ActionResult SendMessage( MvcSendMessageDto model )
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