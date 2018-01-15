using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Bewerbungs.Bot.Luis;

namespace Bewerbungs.Bot.Luis
{
    [Serializable]
    public class CheckEmail : IDialog<object>
    {
        string appName;

        public CheckEmail(string appName)
        {
            this.appName = appName;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Gebe deine hinterlegte Emailadresse an.");
            context.Wait(this.MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            string appMail = message.Text;
            DatabaseConnector databaseConnector = new DatabaseConnector();
            int appID = 0;
            try
            {
                appID = databaseConnector.getApplicantIDMail(this.appName, appMail);
            }
            catch (Exception e)
            {
                await context.PostAsync("falsche Angaben " + e.Message);
            }
            context.Done(appID);

        }
    }
}