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
    //Diese Klasse listet alle aktuell hinterlegten Stellenausschreibungen aus der DB auf 
    [Serializable]
    public class AskingJob : IDialog<object>
    {
        string title;
        private string[] jobs;
        public AskingJob(string title)
        {
            this.title = title;
        }

        //Ausgabe aller Jobs
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(title);
            DatabaseConnector databaseConnector = new DatabaseConnector();
            jobs = databaseConnector.getStellenDBEntry();

            for (int i=0; i< jobs.Length; i++)
            {
                await context.PostAsync(Convert.ToString(i+1) + ": " + jobs[i]);
            }
            await context.PostAsync("Gebe die Stellennummer an, auf die du dich bewerben möchtest. Bitte geben sie eine Zahl an");

            context.Wait(this.MessageReceivedAsync);
        }

        //erwartet als Antwort des Bewerbers einen integer-Wert im Bereich der angegebenen Stellen
        //Bei Erfolg wird der Dialog beendet, sonst wird Frage neu gestellt
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            int jobID;
            bool isNumeric = int.TryParse(message.Text, out jobID);
            if (isNumeric)
            {
                jobID = Convert.ToInt32(message.Text);

                if ((jobID > 0) && (jobID < jobs.Length + 1))
                {
                    //Beendet den Dialog
                    context.Done(jobID);
                }
            }
            //Ansonsten erneute Abfrage
            else
            {

                    await context.PostAsync("Bitte eine Zahl angeben");
                for (int i = 0; i < jobs.Length; i++)
                {
                    await context.PostAsync(Convert.ToString(i + 1) + ": " + jobs[i]);
                }

                context.Wait(this.MessageReceivedAsync);
            }
        }
    }
}