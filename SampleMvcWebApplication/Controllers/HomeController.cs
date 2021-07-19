using System;
using System.Net;
using System.Web.Mvc;

using Microsoft.AspNet.SignalR;

using SampleMvcWebApplication.SampleServices;

namespace SampleMvcWebApplication.Controllers
{
	public class HomeController : Controller
	{
		public const String Name = "Home";

		private readonly ISampleSingletonService  sgl;
		private readonly ISampleScopedService1    sc1;
		private readonly ISampleScopedService2    sc2;
		private readonly ISampleTransientService1 st1;
		private readonly ISampleTransientService2 st2;

		public HomeController(
			ISampleSingletonService  sgl,
			ISampleScopedService1    sc1,
			ISampleScopedService2    sc2,
			ISampleTransientService1 st1,
			ISampleTransientService2 st2
		)
		{
			this.sgl = sgl ?? throw new ArgumentNullException( nameof( sgl ) );
			this.sc1 = sc1 ?? throw new ArgumentNullException( nameof( sc1 ) );
			this.sc2 = sc2 ?? throw new ArgumentNullException( nameof( sc2 ) );
			this.st1 = st1 ?? throw new ArgumentNullException( nameof( st1 ) );
			this.st2 = st2 ?? throw new ArgumentNullException( nameof( st2 ) );
		}

		public ActionResult Index()
		{
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
