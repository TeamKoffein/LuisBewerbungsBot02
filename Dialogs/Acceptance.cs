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
    [Serializable]
    [LuisModel("9c8b155a-ab34-44f0-9da9-5d17c901cc8a", "fbd3e635d95341d28f6cdaa45891da16")]
    public class Acceptance : LuisDialog<object>
    {
        private bool accept = false;
        private string intent;
        private string messagecontext;

        public Acceptance( string intent, string messagecontext)
        {
            this.intent = intent;
            this.messagecontext = messagecontext;
        }

        override public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync($"Ist die Eingabe { messagecontext } mit Bezug auf { intent } korrekt?");
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Acceptance")]
        public async Task Accept(IDialogContext context, LuisResult result)
        {
            accept = true;
            context.Done(accept);
        }
        [LuisIntent("Negative")]
        public async Task Negative(IDialogContext context, LuisResult result)
        {
            context.Done(accept);
        }
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
        public async Task Other(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Bitte bestätigen oder verneinen");
            
        }


    }
}