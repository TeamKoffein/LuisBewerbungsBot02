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
            message.Attachments = new List<Attachment>();
            var button = new List<CardAction>
                {
                    new CardAction(
                        ActionTypes.ImBack,
                        "Ja",
                        "Ja",
                        "Ja")
                };
            button.Add(
                new CardAction(
                        ActionTypes.ImBack,
                        "Nein",
                        "Nein",
                        "Nein"));

            var card = new ThumbnailCard
            {
                Text = text,
                Buttons = button
            };

            message.Attachments.Add(card.ToAttachment());
            return message;
        }
    }
}