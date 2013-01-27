using System.Web;

using Microsoft.Practices.Unity;

[assembly: WebActivator.PostApplicationStartMethod(typeof($rootnamespace$.App_Start.UnityWebFormsStart), "PostStart")]
namespace $rootnamespace$.App_Start
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

            HttpContext.Current.Application.Add("UnityContainer", container);
        }
    }
}