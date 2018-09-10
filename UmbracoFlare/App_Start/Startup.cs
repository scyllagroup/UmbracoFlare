using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Web;

namespace UmbracoFlare.App_Start
{
    public class Startup : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // UmbracoFlare no longer supports Tls 1.0 and 1.1 -> upgrade to 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
    }
}
