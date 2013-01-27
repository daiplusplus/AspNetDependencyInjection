using System.Web;

using Microsoft.Practices.Unity;

[assembly: WebActivator.PostApplicationStartMethod(typeof(SampleWebApplication.App_Start.UnityWebFormsStart), "PostStart")]
namespace SampleWebApplication.App_Start
{
    internal static class UnityWebFormsStart
    {
        /// <summary>
        ///     Initializes the unity container when the application starts up.
        /// </summary>
        internal static void PostStart()
        {
            IUnityContainer container = new UnityContainer();

            // Add any dependencies needed here
            container
                .RegisterType<Service1, Service1>()
                .RegisterType<Service2, Service2>();

            HttpContext.Current.Application.Add("UnityContainer", container);
        }
    }
}