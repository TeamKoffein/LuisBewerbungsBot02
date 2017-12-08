﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Bewerbungs.Bot.Luis;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.IO;

namespace Bewerbungs.Bot.Luis
{
    [Serializable]
    public class Upload : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Bitte schicke uns jetzt die die Datei.");
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            if (message.Attachments != null && message.Attachments.Any())
            {
                var attachment = message.Attachments.First();
                using (HttpClient httpClient = new HttpClient())
                {
                    // Skype attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.
                    if (message.ChannelId.Equals("skype", StringComparison.InvariantCultureIgnoreCase) && new Uri(attachment.ContentUrl).Host.EndsWith("skype.com"))
                    {
                        var token = await new MicrosoftAppCredentials().GetTokenAsync();
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                    string newFileName = attachment.Name;
                    string destinationContainer = "files";
                    string sourceUrl = attachment.ContentUrl;
                    var attachmentUrl = message.Attachments[0].ContentUrl;

                    var connector = new ConnectorClient(new Uri(message.ServiceUrl));

                    var attachmentData = connector.HttpClient.GetByteArrayAsync(attachmentUrl).Result;
                    CloudStorageAccount csa = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
                    CloudBlobClient blobClient = csa.CreateCloudBlobClient();
                    var blobContainer = blobClient.GetContainerReference(destinationContainer);
                    var newBlockBlob = blobContainer.GetBlockBlobReference(newFileName);
                    string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, newFileName);
                    File.WriteAllBytes(destPath, attachmentData);
                    newBlockBlob.UploadFromFile(destPath);
                    var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);

                    var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;

                    await context.PostAsync($"Attachment of {attachment.ContentType} type and size of {contentLenghtBytes} bytes received.");
                }
            }
            else
            {
                await context.PostAsync("Hi there! I'm a bot created to show you how I can receive message attachments, but no attachment was sent to me. Please, try again sending a new message including an attachment.");
            }

            context.Done(true);
        }
    }
}