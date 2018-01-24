using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
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
            var reply = context.MakeMessage();
            IMessageActivity message = (IMessageActivity)reply;
            message.AttachmentLayout = AttachmentLayoutTypes.List;
            message.Text = "";
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel; 
            message.Attachments = CardsAttachment();
            await context.PostAsync(message);
            context.Wait(this.MessageReceivedAsync);
        }

        //Erstellung der Karte, die die offenen Stellen verschickt
        public IList<Attachment> CardsAttachment()
        {
            List<Attachment> attach = new List<Attachment>();
            int lenght = jobs.Length;
            int i = 0;
            while(i<lenght)
            {              
                int counter = 0;
                List<CardAction> Buttons = new List<CardAction>();
                while (i <lenght && counter < 3)
                {
                    int j = i + 1;
                    Buttons.Add(new CardAction(
                         text: jobs[i],
                            value: j.ToString(),
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

        //Erstellung der eigentlichen Karte, die in CardsAttachment beschrieben wird
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
            int i = jobs.Length + 1;
            bool isNumeric = int.TryParse(message.Text, out jobID);
            if (isNumeric)
            {
                jobID = Convert.ToInt32(message.Text);
                if ((jobID > 0) && (jobID < jobs.Length + 1))
                {
                    //Beendet den Dialog
                    context.Done(jobID);
                }
                else
                    await context.PostAsync("Bitte eine Zahl zwischen " + 1 + " und " + i + " angeben.");
            }
            //Ansonsten erneute Abfrage
            else
            {
                await context.PostAsync("Bitte eine Zahl zwischen " + 1 + " und " + i + " angeben.");
                context.Wait(this.MessageReceivedAsync);
            }
        }


    }
}