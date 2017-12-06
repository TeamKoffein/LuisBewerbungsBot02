﻿using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bewerbungs.Bot.Luis
{
    [Serializable]
    public class AskingDialog
    {
        IDialogContext chat;
        public AskingDialog(IDialogContext chatInput)
        {
            chat = chatInput;

        }
        public async void SendMessage(int pos, string[] questions)
        {
            IMessageActivity message = Activity.CreateMessageActivity();
            message.Text = questions[pos];
            message.TextFormat = "plain";
            message.Locale = "en-Us";
            await chat.PostAsync(message);

        }
    }
}