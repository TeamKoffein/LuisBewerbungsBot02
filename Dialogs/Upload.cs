using System;
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
    //Diese Klasse behandelt den Dateien-upload des Chatbots
    [Serializable]
    public class Upload : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Bitte schicke uns jetzt die die Datei.");
            context.Wait(this.MessageReceivedAsync);
        }
        //Diese MessageRecivedAsync Methode ist einzig zum Speichern eines Attachment da. Bei einer Texteingabe wird der Vorgang übersprungen der Speicherung 
        //übersprunge und es wird am Ende zum SuperDialog zurück überwiesen.
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
                    //File Name wird für Lokale Speicherung erstellt mit dem verweis auf einen Ordner im Projekt
                    string newFileName = "tmpData/" + attachment.Name;
                    //File Name zum speichern in der Blob Storage
                    string azureName = attachment.Name;
                    //Container wird in der Storage angegeben
                    string destinationContainer = "files";
                    //URL vom dem Attachment wird gespeichert
                    string sourceUrl = attachment.ContentUrl;
                    //Speicherung des Attachment in ein byte[]
                    var attachmentUrl = message.Attachments[0].ContentUrl;
                    var connector = new ConnectorClient(new Uri(message.ServiceUrl));
                    var attachmentData = connector.HttpClient.GetByteArrayAsync(attachmentUrl).Result;

                    //Verbindung zur Azure CloudStorage
                    CloudStorageAccount csa = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
                    CloudBlobClient blobClient = csa.CreateCloudBlobClient();
                    var blobContainer = blobClient.GetContainerReference(destinationContainer);
                    var newBlockBlob = blobContainer.GetBlockBlobReference(azureName);
                    //File Name zum Projekt erstellt, unabhängig vom Speicherungsort des Projekts
                    //string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, newFileName);
                    //File-Erstellung aus dem byte[]
                    //File.WriteAllBytes(destPath, attachmentData);
                    //Hochladen des Files
                    //await newBlockBlob.UploadFromFileAsync(destPath);
                    //Löschen des lokalen Files
                    // File.Delete(destPath);
                    using (var ms = new MemoryStream(attachmentData, false))
                    {
                        newBlockBlob.UploadFromStream(ms);
                    }
                    var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);

                    var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;

                    await context.PostAsync($"Attachment of {attachment.ContentType} type and size of {contentLenghtBytes} bytes received.");
                }
            }
            else
            {
                await context.PostAsync("Es wurde kein Datei erkannt!");
            }

            context.Done(value: 0);
        }
    }
}