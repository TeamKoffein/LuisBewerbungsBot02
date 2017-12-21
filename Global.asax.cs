using Autofac;
using Bewerbungs.Bot.Luis;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace SimpleEchoBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            /*Conversation.UpdateContainer(builder =>
            {
                builder.RegisterType<DebugActivityLogger>().AsImplementedInterfaces().InstancePerDependency();
            });*/
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
