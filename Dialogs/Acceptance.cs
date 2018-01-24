using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace Bewerbungs.Bot.Luis
{
    //Diese Klasse gibt dem Bewerber die Rückfrage ob seine letzte Antwort in Bezug auf den Intent korrekt war und gibt im die Möglichkeit diese Angabe zu bestätigen 
    // oder erneut zu beantworten.
    [Serializable]
    [LuisModel("9c8b155a-ab34-44f0-9da9-5d17c901cc8a", "19ec3bb52da54d3b855d0fd331c195b8", domain: "eastus2.api.cognitive.microsoft.com")]
    public class Acceptance : LuisDialog<object>
    {
        private int accept = 0;
        private string intent;
        private string messagecontext;
        private string textMessage;
        private bool firstMessage = true;


        //Konstruktor für den Acceptance Dialog
        public Acceptance(string textMessage)
        {
            this.textMessage = textMessage;
        }

        //Konstruktor für den Acceptance Dialog; Es wird das Intent mit übergeben
        public Acceptance( string intent, string messagecontext)
        {
            this.intent = intent;
            this.messagecontext = messagecontext;
        }

        //Erstellung der Karte, die die Akzeptanzabfrage verschickt
        public IMessageActivity AttachedData(IDialogContext context)
        {
            var reply = context.MakeMessage();
            IMessageActivity message = (IMessageActivity)reply;
            message.Attachments = new List<Attachment>();
            var button = new List<CardAction>
                {
                    new CardAction(
                        type: ActionTypes.ImBack,
                        value: "Ja",
                        title: "Ja",
                        displayText: "Ja")
                };
            button.Add(
                new CardAction(
                        type: ActionTypes.ImBack,
                        value: "Nein",
                        title: "Nein",
                        displayText: "Nein"));

            var card = new ThumbnailCard
            {
                Text = $"Ist die Eingabe { messagecontext } mit Bezug auf { intent } korrekt?",
                Buttons = button
            };

            message.Attachments.Add(card.ToAttachment());
            return message;
        }

        //Rückfrage, ob die Eingabe korrekt ist
        override public async Task StartAsync(IDialogContext context)
        {
            if (textMessage == null) { 
                await context.PostAsync(AttachedData(context));
            }
            else
            {
                await context.PostAsync(textMessage);
            }
            context.Wait(this.MessageReceived);
        }

        //Akzeptanz der Angabe und Rückgabe an den Dialog
        [LuisIntent("Acceptance")]
        public async Task Accept(IDialogContext context, LuisResult result)
        {
            accept = 1;
            context.Done(accept);
        }

        //keine Akzeptanz, Rückgabe und erneutes Stellen der Frage
        [LuisIntent("Negative")]
        public async Task Negative(IDialogContext context, LuisResult result)
        {
            accept = 0;
            context.Done(accept);
        }

        //Abfangen der anderen Intents, um Falsche Zuordnungen zu vermeiden
        [LuisIntent("Farewell")]
        [LuisIntent("")]
        [LuisIntent("None")]
        [LuisIntent("Name")]
        [LuisIntent("Job")]
        [LuisIntent("Adress")]
        [LuisIntent("Birthday")]
        [LuisIntent("Career")]
        [LuisIntent("EducationalBackground")]
        [LuisIntent("Email")]
        [LuisIntent("Language")]
        [LuisIntent("PhoneNumber")]
        [LuisIntent("Place")]
        [LuisIntent("PostalCode")]
        [LuisIntent("PrivateProjects")]
        [LuisIntent("ProgrammingLanguage")]
        [LuisIntent("SocialEngagement")]
        [LuisIntent("StartDate")]
        [LuisIntent("Benefits")]
        [LuisIntent("Client")]
        [LuisIntent("Eliza")]
        [LuisIntent("Ethics")]
        [LuisIntent("FlexTime")]
        [LuisIntent("Holiday")]
        [LuisIntent("HolidayDistribution")]
        [LuisIntent("HomeOffice")]
        [LuisIntent("Location")]
        [LuisIntent("Parking")]
        [LuisIntent("Promotion")]
        [LuisIntent("PublicTransport")]
        [LuisIntent("Requirements")]
        [LuisIntent("Salary")]
        [LuisIntent("StaffTraining")]
        [LuisIntent("WorkingHours")]
        [LuisIntent("Greetings")]
        [LuisIntent("Xing")]
        [LuisIntent("Upload")]        
        public async Task Other(IDialogContext context, LuisResult result)
        {
            if (!firstMessage)
            {
                await context.PostAsync("Bitte bestätigen oder verneinen");
                await context.PostAsync(AttachedData(context));
                
            }
            else
            {
                firstMessage = false;
            }

        }


    }
}