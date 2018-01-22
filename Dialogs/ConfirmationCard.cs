using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bewerbungs.Bot.Luis
{
    public class ConfirmationCard
    {
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
                //Images = new List<CardImage>() { new CardImage(url: "https://upload.wikimedia.org/wikipedia/commons/c/ca/1x1.png") }
                
            };

            message.Attachments.Add(card.ToAttachment());
            return message;
        }
    }
}