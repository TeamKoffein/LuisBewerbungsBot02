using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
//using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Bewerbungs.Bot.Luis;

namespace Bewerbungs.Bot.Luis
{
    //Die E-Mail Adresse für die Validierung der bekannten Nutzer
    [Serializable]
    public class CheckEmail : IDialog<object>
    {
        string appName;

        //Konstruktor
        public CheckEmail(string appName)
        {
            this.appName = appName;
        }

        //Einforderung der Eingabe für die E-Mail Adressen Validierung
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Gebe deine hinterlegte Emailadresse an.");
            context.Wait(this.MessageReceivedAsync);
        }

        //Check, ob die vom Nutzer angegebene Mail Adresse hinterlegt wurde
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