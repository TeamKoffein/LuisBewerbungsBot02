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
        bool safeDataConfirmation;
        int jobID = -1;

        //1= Du , 2 = Sie
        int du = -1; 
        int applicantID = -1;

        //StartAsync startet den Gesprächbeginn
        override public async Task StartAsync(IDialogContext Chat)
        {
            DatabaseConnector databaseConnector = new DatabaseConnector();
            //Initialisierung der Grundvariablen
            safeDataConfirmation = false;


            //Speicherung der Fragen für Du und Sie
            askingPersonal = databaseConnector.getFAQQuestions(1);
            askingFormal = databaseConnector.getFAQQuestions(2);

            //Willkommenstext und Datenschutzerklaerung beim Starten des Bots
            await Chat.PostAsync("Herzlich Willkommen bei unserem Bewerbungsbot! Wir freuen uns, dass du dich für eine unserer Stellen interessierst. Damit du dich erfolgreich bewerben kannst, musst du folgende Datenschutzerklaerung lesen und akzeptieren, sonst koennen wir leider mit der Bewerbung nicht fortfahren: Im Rahmen dieses Gespraechs mit dem Bot werden personenbezogene Daten über dich erhoben, jedoch nur zu Zwecken der Bewerbung erhoben, gespeichert, verarbeitet und genutzt, die in Zusammenhang mit deinem Interesse an einer Stelle bei uns steht. Es erfolgt keine Weitergabe an Dritte. Du kannst deine Einwilligung jederzeit mit Wirkung für die Zukunft widerrufen und wir löschen dann deine Daten umgehend. Bitte schreibe uns in diesem Falll unter Angabe deines vollständigen Namens eine Email. Bitte bestätige, dass du die Erklaerung gelesen hast und sie akzeptierst, indem du folgendes abschreibst: 'Ja, ich bestaetige.'");

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
            await context.Forward(new Acceptance(result.TopScoringIntent.Intent.ToString(), message.Text), AfterName, message, CancellationToken.None);
        }

        /* AfterName speichert den Namen des Bewerber ab und stellt die Frage nach der Stelle
         * 
         */
        private async Task AfterName(IDialogContext context, IAwaitable<object> result)
        {
            DatabaseConnector databaseConnector = new DatabaseConnector();
            int accept =Convert.ToInt32(await result);
            if (accept == 1)
            {
                //Namenspeicherung
                var myKey = AnswerDatabase.IndexOf("Name");
                Question[index: myKey] = true;
                applicantID = databaseConnector.insertDatabaseEntry("Name", Text);
            }
            //Nächste, nicht-beantwortete Frage
            int index = Question.FindIndex(x => x == false);
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
                    await context.Forward(new AskingJob(askingFormal[index]), AfterStellen, context, CancellationToken.None);
                }
                else
                {
                    await context.PostAsync(askingFormal[index]);
                    context.Wait(this.MessageReceived);
                }
            }
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
            if (accept == 1)
            {
                var myKey = AnswerDatabase.IndexOf(LuisTopIntention);
                Question[index: myKey] = true;
                databaseConnector.updateDatabase(LuisTopIntention, applicantID, Text);
               
            }
            int index = Question.FindIndex(x => x == false);

            //Fragendialogabschluss wird erkannt, wenn die Liste @questions den Wert -1 zurückgibt.
            if (index == -1)
            {
                DataAssembler assemble = new DataAssembler();
                assemble.sendData(applicantID);
                await context.PostAsync("Wir sind hier mit unseren Fragen fertig. Deine Daten werden an den Recruiter übermittelt, aber du kannst mir gerne weiterhin Fragen stellen.");

                await Task.Delay(10000);
                await context.PostAsync("Der Recruiter hat deine Daten eingesehen und wird dich demnächst zu einem Gespräch einladen");
            }
            else
            {
                if (du == 1)
                {
                    if (index == 1)
                    {
                        context.Call(child: new AskingJob(askingFormal[index]), resume: AfterAnswer);
                    }
                    await context.PostAsync(askingPersonal[index]);
                }
                else
                {
                    if (index == 1)
                    {
                        context.Call(child: new AskingJob(askingFormal[index]), resume: AfterAnswer);    
                    }
                    await context.PostAsync(askingFormal[index]);
                }
            }
            context.Wait(this.MessageReceived);
        }

        /*Wenn der Bewerber angegeben hat auf welche Stelle er sich bewerben möchte, wird dies hinterlegt und die dementsprechende Fachfrage zur der Psoition gestellt
         */ 
        public async Task AfterStellen(IDialogContext context, IAwaitable<object> result)
        {
            DatabaseConnector databaseConnector = new DatabaseConnector();
            int accept = Convert.ToInt32(await result);
            Question[index: 1] = true;
            databaseConnector.updateDatabase("Job", applicantID, Convert.ToString(accept));

            string technicalQuestion = databaseConnector.getTechQuestion(accept);
            await context.PostAsync(technicalQuestion);

            int index = Question.FindIndex(x => x == false);
            AskingDialog send = new AskingDialog(context);
            if (du == 1)
            {
                await context.PostAsync(askingPersonal[index]);
            }
            else
            {
                await context.PostAsync(askingFormal[index]);
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
                    if (jobID < -1)
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

            //System.Web.UI.WebControls.HyperLink linkXing = new System.Web.UI.WebControls.HyperLink();
            //linkXing.ID = "linkXing";
            //linkXing.Text = "Ja, ich moechte mich in Xing anmelden!";
            //linkXing.Visible = true;
            //linkXing.NavigateUrl = "Xing.aspx";

            await context.PostAsync("Fickdich");
            context.Wait(this.MessageReceived);
        }

        //Aufruf der Upload-Funktion
        [LuisIntent("Upload")]
        public async Task Upload(IDialogContext context, LuisResult result)
        {
            context.Call(new Upload(), AfterStellen);
        }
        public async Task AfterUpload(IDialogContext context, LuisResult result)
        {
            int index = Question.FindIndex(x => x == false);
            if (Question[2] == false)
            {
                index = 2;
            }
            if (du == 1)
            {
                if (index == 1)
                {
                    //await context.Forward(new AskingJob(askingFormal[index]), AfterStellen, context, CancellationToken.None);
                    context.Call(new AskingJob(askingFormal[index]), AfterStellen);
                }
                else
                {
                    await context.PostAsync(askingPersonal[index]);
                    context.Wait(this.MessageReceived);
                }
            }
        }

        //Verneinung durch den Bewerber
        [LuisIntent("Negative")]
        public async Task Negative(IDialogContext context, LuisResult result)
        {
            await FindAcceptance(context, false);
            context.Wait(this.MessageReceived);
        }

        //Abfrage der Anrede nach Bestätigung der Datenschutzerklärung
        public async Task FindAcceptance(IDialogContext context, bool result)
        {
            if (!safeDataConfirmation)
            {
                if (result)
                {
                    safeDataConfirmation = true;
                    await context.PostAsync("Danke für das Akzeptieren der Datenschutzerklärung. Sollen wir das 'Du' einführen?");
                }
                else
                {
                    await context.PostAsync("Wenn du unsere Datenschutzerklärung nicht bestätigst kannst du nicht mit dem Bot schreiben!");
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
        }

    }
}