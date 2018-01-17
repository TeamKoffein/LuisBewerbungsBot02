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
            /*
            string jobstring = "";

            for (int i = 0; i < jobs.Length; i++)
            {
                jobstring = jobstring + " " + "\n\n" + Convert.ToString(i + 1) + ": " + jobs[i];
            }
            await context.PostAsync(jobstring);
            await context.PostAsync("Gebe die Stellennummer an, auf die du dich bewerben möchtest. Bitte geben sie eine Zahl an");
            */
            
            var reply = context.MakeMessage();
            IMessageActivity message = (IMessageActivity)reply;
            message.AttachmentLayout = AttachmentLayoutTypes.List;
            message.Text = "";
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            
          /*  List<CardAction> Buttons = new List<CardAction>();
            for (int i = 1; i <= jobs.Length; i++)
            {
                Buttons.Add(new CardAction(
                            text: jobs[i],
                            value: i.ToString(),
                            type: ActionTypes.ImBack,
                            displayText: jobs[i],
                            title: jobs[i]));
            }*/

            //message.Attachments.Add(GetThumbnailCard("title", "text", Buttons)); 
            message.Attachments = CardsAttachment();
            await context.PostAsync(message);
            context.Wait(this.MessageReceivedAsync);
        }

       /* public IMessageActivity AttachedData(IDialogContext context)
        {
            var reply = context.MakeMessage();
            IMessageActivity message = (IMessageActivity)reply;
            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            message.Attachments = CardsAttachment();          
            return message;
        }*/

        public IList<Attachment> CardsAttachment()
        {
            List<Attachment> attach = new List<Attachment>();
            int lenght = jobs.Length;
            int i = 1;
            while(i<=lenght)
            {              
                int counter = 0;
                List<CardAction> Buttons = new List<CardAction>();
                while (i <=lenght && counter < 3)
                {
                    Buttons.Add(new CardAction(
                         text: jobs[i],
                            value: i.ToString(),
                            type: ActionTypes.ImBack,
                            displayText: jobs[i],
                            title: jobs[i]
                ));
                    i++;
                    counter++;
                }               
                attach.Add(GetThumbnailCard("Stellenausschreibungen", "Bitte wähle eine Stelle aus", Buttons));
            }
            return attach;


        }

        private Attachment GetThumbnailCard(string title, string text, List<CardAction> cardAction)
        {
            var heroCard = new ThumbnailCard
            {
                
                Title = title,
                Subtitle = "Stellenauswahl",
                Text = text,
                Images = new List<CardImage>() { new CardImage(url: "https://t2.ftcdn.net/jpg/00/60/83/19/500_F_60831978_c24ahi9gJDOsefFT6lvt3VOVjwgeXxZz.jpg") },
                Buttons = cardAction
            };

            return heroCard.ToAttachment();
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
                /*
                string jobstring = "";

                for (int i = 0; i < jobs.Length; i++)
                {
                    jobstring = jobstring + " " + "\n\n" + Convert.ToString(i + 1) + ": " + jobs[i];
                }
                await context.PostAsync("Bitte eine Zahl angeben");
                await context.PostAsync(jobstring);
                */
                //await context.PostAsync(AttachedData(context));
                context.Wait(this.MessageReceivedAsync);
            }
        }


    }
}