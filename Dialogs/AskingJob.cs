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
    public class AskingJob : IDialog<object>
    {
        string title;
        private string[] jobs;
        public AskingJob(string title)
        {
            this.title = title;
        }

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
                    /* Completes the dialog, removes it from the dialog stack, and returns the result to the parent/calling
                        dialog. */
                    context.Done(jobID);
                }
            }
            /* Else, try again by re-prompting the user. */
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