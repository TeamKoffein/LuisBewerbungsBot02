using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.ConnectorEx;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using Microsoft.WindowsAzure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using System.Configuration;
using System.Net.Http.Headers;
using System.IO;
using AdaptiveCards;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Bewerbungs.Bot.Luis
{

    /*Die Klasse Superdialog dient als Rahmen für die Kommunikation mit dem Bewerber.
     * Im Superdialog beginnt das Gespräch mit dem Bewerber und alle Dialog-Queues werden innerhalb des Gesprächs aufgerufen.
     * Die Liste @FAQDatabase beinhaltet alle Intents für die FAQ-Abfragen und @AnswerDatabase alle Intents für Antwortmöglichkeiten.
     * Liste @Questions markiert alle noch nicht beantworteten Fragen des Bewerbers
     */
    [Serializable]
    [LuisModel("9c8b155a-ab34-44f0-9da9-5d17c901cc8a", "19ec3bb52da54d3b855d0fd331c195b8", domain: "eastus2.api.cognitive.microsoft.com")]
    public class SuperDialog : LuisDialog<object>
    {
        string Text;
        string LuisTopIntention;
        List<string> FAQDatabase = new List<string>() { "","Holiday", "WorkingHours", "Salary", "FlexTime", "HolidayDistribution",
            "Location", "HomeOffice", "PublicTransport", "Parking", "Benefits", "Client", "Ethics", "StaffTraining", "Promotion", "Eliza",
            "Requirements"};
        List<string> AnswerDatabase = new List<string>() {"", "Job", "Name", "Adress", "PostalCode", "Place", "PhoneNumber", "Email", "Birthday",
             "Career", "EducationalBackground", "ProgrammingLanguage", "SocialEngagement", "Language", "PrivateProjects",
            "StartDate"};
        List<bool> Question = new List<bool>() {true, false, false, false, false, false, false, false, false, false, false,
            false, false, false, false, false};
        string[] askingPersonal;
        string[] askingFormal;
        string[] currentData;
        bool safeDataConfirmation;
        bool safeNewsConfirmation;
        bool nameUpdateable;
        bool currentUpload;
        int jobID = -1;

        int sendDataConfirmation = -2;
        int saveDataLongterm = -2;
        int correctData = -2;

        //knowledge dient zur Erkennung der Gesprächsfortsetzung
        // -1= Wert nicht gesetzt; 0= neuer Bewerber; 1= gespräch wird fortgesetzt
        int knowledge = -1;

        //1= Du , 2 = Sie
        int du = -1;
        int applicantID = -1;

        public class JsonFileBing
        {
            public string Origin { get; set; }
            public string Destination { get; set; }
            public ConversationReference RelatesTo { get; set; }
        }

        public static async Task AddMessageToQueueAsync(string message, string queueName)
        {
            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(SettingsUtils.GetAppSettings("AzureWebJobsStorage"));

            // Create the queue client.
            var queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue.
            var queue = queueClient.GetQueueReference(queueName);

            // Create the queue if it doesn't already exist.
            await queue.CreateIfNotExistsAsync();

            // Create a message and add it to the queue.
            var queuemessage = new CloudQueueMessage(message);
            await queue.AddMessageAsync(queuemessage);
        }

        //StartAsync startet den Gesprächbeginn
        override public async Task StartAsync(IDialogContext Chat)
        {
            DatabaseConnector databaseConnector = new DatabaseConnector();
            //Initialisierung der Grundvariablen
            safeDataConfirmation = false;
            safeNewsConfirmation = false;


            //Speicherung der Fragen für Du und Sie
            askingPersonal = databaseConnector.getFAQQuestions(1);
            askingFormal = databaseConnector.getFAQQuestions(2);

            //Willkommenstext und Datenschutzerklaerung beim Starten des Bots
            await Chat.PostAsync("Herzlich Willkommen bei unserem Bewerbungsbot! Wir freuen uns, dass du dich für eine unserer Stellen interessierst.");
            await Chat.PostAsync("Damit du dich erfolgreich bewerben kannst, musst du die Datenschutzerklaerung lesen und akzeptieren, sonst koennen wir leider mit der Bewerbung nicht fortfahren.");
            await Chat.PostAsync("Bitte bestätige danach hier im Bot, dass du die Erklaerung unter http://luisbewerbungsbot02.azurewebsites.net/PrivacyPolicy.html gelesen hast und sie akzeptierst.");

            Chat.Wait(this.MessageReceived);
        }

        [LuisIntent("Farewell")]
        [LuisIntent("")]
        [LuisIntent("None")]
        //Abfang von Luis nicht einordbaren Bewerberaussagen
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Name")]
        //Die erste Frage, die dem Bewerber gestellt werden soll, da durch die Eingabe die des Namens eine neue BewerberID vergeben wird, die notwendig ist um alle folgenden 
        // Antworten zu speichern.
        //Weitergabe an AfterName
        public async Task Name(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            Text = message.Text;
            if (knowledge == 0)
            {
                await context.Forward(new Acceptance(result.TopScoringIntent.Intent.ToString(), message.Text), AfterName, message, CancellationToken.None);
            }
            else
            {
                DatabaseConnector databaseConnector = new DatabaseConnector();
                int count = databaseConnector.getCountName(message.Text);
                if (count == 0)
                {
                    knowledge = 0;
                    await context.PostAsync("Name nicht gefunden. Neuer Name angelegt.");
                    await context.Forward(new Acceptance(result.TopScoringIntent.Intent.ToString(), message.Text), AfterName, message, CancellationToken.None);
                }
                else
                {
                    await context.PostAsync("Name gefunden. " + message.Text);

                    context.Call(new CheckEmail(message.Text), CheckInformation);
                }
            }
        }

        private async Task CheckInformation(IDialogContext context, IAwaitable<object> result)
        {
            DatabaseConnector databaseConnector = new DatabaseConnector();
            applicantID = Convert.ToInt32(await result);
            if (applicantID > 0)
            {
                Question = databaseConnector.getMissing(applicantID);
                int index = Question.FindIndex(x => x == false);

                if (index == -1)
                {
                    await context.PostAsync("Du hast schon alle Angaben gemacht!");
                    currentData = databaseConnector.getData(applicantID);
                    string data = "";
                    for (int i = 0; i < currentData.Length; i++)
                    {
                        data = data + "" + "\n\n" + currentData[i];
                    }
                    await context.PostAsync("Diese Angaben sind hinterlegt: " + Environment.NewLine + data);

                    correctData = -1;
                    await context.PostAsync("Sind diese Angaben korrekt?");
                }
                else
                {
                    currentData = databaseConnector.getData(applicantID);
                    string data = "";
                    for (int i = 0; i < currentData.Length; i++)
                    {
                        data = data + "" + "\n\n" + currentData[i];
                    }
                    await context.PostAsync("Diese Angaben sind hinterlegt: " + Environment.NewLine + data);

                    if (du == 1)
                    {
                        if (index == 1)
                        {
                            //Frage nach beworbene Stelle mit Du
                            context.Call(new AskingJob(askingFormal[index]), AfterStellen);
                        }
                        else
                        {
                            //Frage nach beworbene Stelle mit Sie
                            await context.PostAsync(askingPersonal[index]);
                            context.Wait(this.MessageReceived);
                        }
                    }
                    else
                    {
                        if (index == 1)
                        {
                            context.Call(new AskingJob(askingFormal[index]), AfterStellen);
                        }
                        else
                        {
                            await context.PostAsync(askingFormal[index]);
                            context.Wait(this.MessageReceived);
                        }
                    }
                }

            }
            else
            {
                knowledge = 0;
                await context.PostAsync("Neuer Bewerber. Name noch einmal angeben.");
                context.Wait(this.MessageReceived);
            }
        }

        /* AfterName speichert den Namen des Bewerber ab und stellt die Frage nach der Stelle
         * 
         */
        private async Task AfterName(IDialogContext context, IAwaitable<object> result)
        {
            DatabaseConnector databaseConnector = new DatabaseConnector();
            int accept = Convert.ToInt32(await result);
            if (accept == 1)
            {
                if (nameUpdateable)
                {
                    databaseConnector.updateDatabase("Name", applicantID, Text);
                }
                //Namenspeicherung
                else
                {
                    var myKey = AnswerDatabase.IndexOf("Name");
                    Question[index: myKey] = true;
                    applicantID = databaseConnector.insertDatabaseEntry("Name", Text);
                }
            }
            //Neue Methode hinzugefügt
            await FindNextAnswer(context, accept);
        }

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

        //Filtern nach dem erwarteten Intent, Weitergabe an AfterAnswer
        public async Task Answer(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            Text = message.Text;
            LuisTopIntention = result.TopScoringIntent.Intent.ToString();
            await context.Forward(new Acceptance(result.TopScoringIntent.Intent.ToString(), message.Text), AfterAnswer, message, CancellationToken.None);
        }

        /*Speicherung der letzten Antwort des Bewerbers, nachdem diese bestätigt wurde
         * Sofern noch Fragen unbeantwortet sind, werden diese dem Bewerber gestellt.
         * Wenn keine Fragen offen sind, wird dies dem Bewerber mitgeteilt und die Daten werden an den Recruiter übermittelt.
         */
        public async Task AfterAnswer(IDialogContext context, IAwaitable<object> result)
        {
            DatabaseConnector databaseConnector = new DatabaseConnector();
            int accept = Convert.ToInt32(await result);

            if (currentUpload)
            {
                if (accept == 1)
                {
                    int counter = 2;
                    while (AnswerDatabase[counter] != "StartDate")
                    {
                        Question[counter] = true;
                        counter++;
                    }
                }
            }
            else
            {

                if (accept == 1)
                {
                    var myKey = AnswerDatabase.IndexOf(LuisTopIntention);
                    Question[index: myKey] = true;
                    databaseConnector.updateDatabase(LuisTopIntention, applicantID, Text);
                    databaseConnector.updateDatabase("ChannelID", applicantID, context.Activity.ChannelId);
                    databaseConnector.updateDatabase("ConversationID", applicantID, context.Activity.Conversation.Id);

                }
            }
            //Neue Methode hinzugefügt
            await FindNextAnswer(context, accept);
        }

        /*Wenn der Bewerber angegeben hat auf welche Stelle er sich bewerben möchte, wird dies hinterlegt und die dementsprechende Fachfrage zur der Position gestellt
         */
        public async Task AfterStellen(IDialogContext context, IAwaitable<object> result)
        {
            DatabaseConnector databaseConnector = new DatabaseConnector();
            int accept = Convert.ToInt32(await result);
            Question[index: 1] = true;
            databaseConnector.updateDatabase("Job", applicantID, Convert.ToString(accept));

            string date = databaseConnector.getJobDate(accept);
            await context.PostAsync("Zu diesem Termin stellen wir ein: " + date);
            //Neue Methode hinzugefügt
            await FindNextAnswer(context, 1);
        }

        public async Task AfterNewsletter(IDialogContext context, IAwaitable<object> result)
        {
            int accept = Convert.ToInt32(await result);
            if (accept == 0)
            {
                await context.PostAsync("Schade! Aber wir akzeptieren das mit gebrochenem Herzen. :(");
            }
            else
            {
                safeNewsConfirmation = true;
                await context.PostAsync("Wir freuen uns und informieren dich gerne darüber, was bei uns so alles abgeht!");
                DatabaseConnector databaseConnector = new DatabaseConnector();
                databaseConnector.updateNewsletter(applicantID);
            }
            context.Wait(this.MessageReceived);
        }

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

        //Wenn der Intent nach einer FAQ an Lise erkannt wird, wird der entsprechende Antworteintrag abhängig von der gewählten Anrede ausgelesen.
        public async Task FAQ(IDialogContext context, LuisResult result)
        {
            if (safeDataConfirmation)
            {
                DatabaseConnector databaseConnector = new DatabaseConnector();
                int key = FAQDatabase.IndexOf(result.TopScoringIntent.Intent.ToString());
                if (key < 16)
                {
                    if (key == 6)
                    {
                       // if (Question[4] && Question[5] && Question[6])
                        //{
                            string homeAdress = databaseConnector.getBingAdress(applicantID);
                        if (!String.IsNullOrEmpty(homeAdress))
                        {
                            await context.PostAsync("Adresse empty");
                        }
                        else
                        {
                            var bingTrigger = new JsonFileBing
                            {
                                RelatesTo = context.Activity.ToConversationReference(),
                                Origin = homeAdress,
                                Destination = "Am Butzweilerhofallee 2, Köln"
                            };
                            await AddMessageToQueueAsync(JsonConvert.SerializeObject(bingTrigger), "bingtrigger");
                        }
                      //  }
                     //   else
                      //  {
                      //      await context.PostAsync("Wenn du mir deine Adresse, Postleitzahl und den Ort angibst, dann sag ich Dir wie lange du zu uns brauchst.");
                      //  }

                    }
                    if (du != -1)
                    {    
                        await context.PostAsync(databaseConnector.getDBEntry(key, "SELECT * FROM FAQ WHERE FAQID =@ID", du));
                    }
                    else
                    {
                        await context.PostAsync("Bitte verrate mir vorher, ob wir beim 'Du' bleiben sollen");
                    }
                }
                else
                {
                    if (jobID > -1)
                    {
                        await context.PostAsync(databaseConnector.getDBEntry(jobID, "SELECT Profil FROM Stellen WHERE StellenID =@ID"));
                    }
                }
            }
            else
            {
                await context.PostAsync("Zuerst musst du die Datenschutzerklärung bestätigen.");
            }
            await context.PostAsync(result.TopScoringIntent.Intent.ToString());
            context.Wait(this.MessageReceived);
        }

        //Begrüßen des Bewerbers
        [LuisIntent("Greetings")]
        public async Task Greetings(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hallo User!");
            context.Wait(this.MessageReceived);
        }

        //Akzeptanz durch den Bewerber
        [LuisIntent("Acceptance")]
        public async Task Acceptance(IDialogContext context, LuisResult result)
        {
            await FindAcceptance(context, true);
            context.Wait(this.MessageReceived);
        }

        //Xing-Anbindung 
        [LuisIntent("Xing")]
        public async Task Xing(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("http://luisbewerbungsbot02.azurewebsites.net/Xing.aspx");
            context.Wait(this.MessageReceived);
        }

        //Aufruf der Upload-Funktion
        [LuisIntent("Upload")]
        public async Task Upload(IDialogContext context, LuisResult result)
        {
            currentUpload = true;
            context.Call(new Upload(applicantID), AfterAnswer);
        }


        //Verneinung durch den Bewerber
        [LuisIntent("Negative")]
        public async Task Negative(IDialogContext context, LuisResult result)
        {
            await FindAcceptance(context, false);
            context.Wait(this.MessageReceived);
        }

        //Methode zum finden der Nächsten Frage
        public async Task FindNextAnswer(IDialogContext context, int accept)
        {
            DatabaseConnector databaseConnector = new DatabaseConnector();
            int index = Question.FindIndex(x => x == false);
            if (Question[2] == false)
            {
                index = 2;
            }
            if (index == -1)
            {
                currentData = databaseConnector.getData(applicantID);
                string data = "";
                for (int i = 0; i < currentData.Length; i++)
                {
                    data = data + "" + "\n\n" + currentData[i];
                }
                await context.PostAsync("Dies sind deine Angaben: " + Environment.NewLine + data);

                correctData = -1;
                await context.PostAsync("Sind diese Angaben korrekt?");
            }
            else
            {
                if (accept == 0)
                {
                    index = 2;
                }
                if (du == 1)
                {
                    if (index == 1)
                    {
                        //Frage nach beworbene Stelle mit Du
                        context.Call(new AskingJob(askingFormal[index]), AfterStellen);
                    }
                    else
                    {
                        //Frage nach beworbene Stelle mit Sie
                        await context.PostAsync(askingPersonal[index]);
                        context.Wait(this.MessageReceived);
                    }
                }
                else
                {
                    if (index == 1)
                    {
                        context.Call(new AskingJob(askingFormal[index]), AfterStellen);
                    }
                    else
                    {
                        await context.PostAsync(askingFormal[index]);
                        context.Wait(this.MessageReceived);
                    }
                }
            }
        }
        //Abfrage der Anrede nach Bestätigung der Datenschutzerklärung
        public async Task FindAcceptance(IDialogContext context, bool result)
        {
            if (!safeDataConfirmation)
            {
                if (result)
                {
                    safeDataConfirmation = true;
                    await context.PostAsync("Danke für das Akzeptieren der Datenschutzerklärung. Kennst du mich schon?");
                }
                else
                {
                    await context.PostAsync("Wenn du unsere Datenschutzerklärung nicht bestätigst kannst du nicht mit dem Bot schreiben!");
                }
            }
            else if (knowledge == -1)
            {
                if (!result)
                {
                    knowledge = 0;
                    await context.PostAsync("Darf ich Sie duzen?");
                }
                else
                {
                    knowledge = 1;
                    await context.PostAsync("Darf ich Sie duzen?");
                }
            }
            else if (du == -1)
            {
                await context.PostAsync("Dann fangen wir mal an!");
                if (result)
                {
                    du = 1;
                    await context.PostAsync(askingPersonal[2]);

                }
                else
                {
                    du = 2;
                    await context.PostAsync(askingFormal[2]);
                }
            }
            else if (correctData == -1)
            {
                if (result)
                {
                    correctData = 1;
                    sendDataConfirmation = -1;
                    await context.PostAsync("Darf ich die Daten an den Recruiter übermitteln?");
                }
                else
                {
                    nameUpdateable = true;
                    await context.PostAsync("Welche Angabe ist falsch?");
                }
            }
            else if (sendDataConfirmation == -1)
            {
                if (result)
                {
                    saveDataLongterm = -1;
                    sendDataConfirmation = 1;
                    await context.PostAsync("Dann versende ich die Daten an den Recruiter.");
                    DataAssembler assemble = new DataAssembler();
                    assemble.sendData(applicantID);
                    await context.PostAsync("Wir sind hier mit unseren Fragen fertig. Deine Daten werden an den Recruiter übermittelt, aber du kannst mir gerne weiterhin Fragen stellen.");
                }
                else
                {
                    saveDataLongterm = -1;
                    sendDataConfirmation = 0;
                    await context.PostAsync("Deine Daten werden nicht übermittelt. Möchtest du deine Daten trotzdem dauerhaft speichern?");
                }
            }
            else if (saveDataLongterm == -1)
            {
                if (result)
                {
                    saveDataLongterm = 1;
                    DatabaseConnector databaseConnector = new DatabaseConnector();
                    databaseConnector.transferData(applicantID);
                    await context.PostAsync("Daten dauerhaft gespeichert.");

                    //Abfrage fuer Datenschutz bzgl. Newsletter
                    context.Call(new Acceptance("Wie versprochen erheben wir deine Daten nur für die Bewerbungszwecke. Möchtest du, dass wir dich auch ueber Neuigkeiten informieren?"), AfterNewsletter);

                }
                else
                {
                    saveDataLongterm = 0;
                    await context.PostAsync("Daten nicht gespeichert.");

                    //Abfrage fuer Datenschutz bzgl. Newsletter
                    context.Call(new Acceptance("Wie versprochen erheben wir deine Daten nur für die Bewerbungszwecke. Möchtest du, dass wir dich auch ueber Neuigkeiten informieren?"), AfterNewsletter);

                }
            }
        }
    }
}