using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
//using System;
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
        int applicantID;
        List<string> lebenslaufnamen= new List<string>(){"lebenslauf","cv"};
        public Upload(int applicantID)
        {
            this.applicantID = applicantID;
        }
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
                    //File Name zum speichern in der Blob Storage
                    string azureName = applicantID + "/" + attachment.Name;
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
                    //byte[] wird mit einem MemoryStream in die Blob Storage geschrieben.
                    using (var ms = new MemoryStream(attachmentData, false))
                    {
                        newBlockBlob.UploadFromStream(ms);
                    }
                    var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);

                    var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;

                    await context.PostAsync($"Attachment of {attachment.ContentType} type and size of {contentLenghtBytes} bytes received.");
                    
                }

                //Es wird zunächst anhand des Dateinamens überprüft, ob es sich um einen Lebenslauf handelt.
                string lowerName = (attachment.Name).ToLower();
                foreach (string schleifenLebenslauf in lebenslaufnamen)
                {
                    if (lowerName.Contains(schleifenLebenslauf))
                    {
                        context.Done(value: 1);
                    }
                }
                //Konnte dies anhand des Dateinamens nicht festgestellt werden, wird der User explizit danach gefragt, ob es sich
                //um einen Lebenslauf handelt.
                context.Call(new Acceptance("Handelt es sich bei der hochgeladenen Datei um einen Lebenslauf?"), AfterUpload);
            }
            else
            {
                await context.PostAsync("Es wurde keine Datei erkannt!");
                context.Done(value: 2);
            }

            
        }

        //Wenn der Upload abgeschlossen wurde wird dem BasisDialog ein Boolean zurückgegeben, damit der eigentliche Prozess weiter läuft
        private async Task AfterUpload(IDialogContext context, IAwaitable<object> result)
        {
            int accept = Convert.ToInt32(await result);
            context.Done(value: accept);
        }
    }
}