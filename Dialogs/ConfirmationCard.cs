using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bewerbungs.Bot.Luis
{
    //Erstellung der Acceptance Card, die es dem nutzer ermöglicht auf die Fragen einfach mit Hilfe von Buttons "Ja" und "Nein" zu antworten
    public class ConfirmationCard
    {
        //Erstellung der Nachricht, die die Karte verschickt
        public IMessageActivity AttachedData(IDialogContext context, string text)
        {
            var reply = context.MakeMessage();
            IMessageActivity message = (IMessageActivity)reply;
            message.Text = "";
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
                Text = text,
                Buttons = button,
            };

            message.Attachments.Add(card.ToAttachment());
            return message;
        }
    }
}