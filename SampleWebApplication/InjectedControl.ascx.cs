using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Practices.Unity;

namespace SampleWebApplication
{
    public partial class InjectedControl : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        #region Dependencies

        [Dependency]
        public Service1 InjectedService1 { get; set; }

        [Dependency]
        public Service2 InjectedService2 { get; set; }
 
        #endregion
    }
}