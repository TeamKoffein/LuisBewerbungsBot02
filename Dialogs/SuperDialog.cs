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

namespace Bewerbungs.Bot.Luis
{
    [Serializable]
    [LuisModel("9c8b155a-ab34-44f0-9da9-5d17c901cc8a", "fbd3e635d95341d28f6cdaa45891da16")]
    public class SuperDialog : LuisDialog<object>
    {
        List<string> FAQDatabase = new List<string>() { "","Holiday", "WorkingHours", "Salary", "FlexTime", "HolidayDistribution",
            "Location", "HomeOffice", "PublicTransport", "Parking", "Benefits", "Client", "Ethics", "StaffTraining", "Promotion", "Eliza",
            "Requirements"};
        List<string> AnswerDatabase = new List<string>() {"", "Job", "Name", "Adress", "PostalCode", "Place", "PhoneNumber", "Email", "Birthday",
             "Career", "EducationalBackground", "ProgrammingLanguage", "SocialEngagement", "Language", "PrivateProjects",
            "StartDate"};
        List<bool> Question = new List<bool>() {true, false, false, false, false, false, false, false, false, false, false,
            false, false, false, false};
        bool safeDataConfirmation;
        int jobID = -1;
        int du = -1;
        int applicantID = -1;

        override public async Task StartAsync(IDialogContext Chat)
        {
            //Initialisierung der Grundvariablen
            safeDataConfirmation = false;

            //Willkommenstext und Datenschutzerklaerung beim Starten des Bots
            await Chat.PostAsync("Herzlich Willkommen bei unserem Bewerbungsbot! Wir freuen uns, dass du dich für eine unserer Stellen interessierst. Damit du dich erfolgreich bewerben kannst, musst du folgende Datenschutzerklaerung lesen und akzeptieren, sonst koennen wir leider mit der Bewerbung nicht fortfahren: Im Rahmen dieses Gespraechs mit dem Bot werden personenbezogene Daten über dich erhoben, jedoch nur zu Zwecken der Bewerbung erhoben, gespeichert, verarbeitet und genutzt, die in Zusammenhang mit deinem Interesse an einer Stelle bei uns steht. Es erfolgt keine Weitergabe an Dritte. Du kannst deine Einwilligung jederzeit mit Wirkung für die Zukunft widerrufen und wir löschen dann deine Daten umgehend. Bitte schreibe uns in diesem Falll unter Angabe deines vollständigen Namens eine Email. Bitte bestätige, dass du die Erklaerung gelesen hast und sie akzeptierst, indem du folgendes abschreibst: 'Ja, ich bestaetige.'");

            await Chat.PostAsync("Bewerbungs.Bot.Luis/Xing.html");
            /*
            //Xing Test (Card Aufruf)
            XingDialog xingtest = new XingDialog(Chat);
            xingtest.createCardUrl();*/

            Chat.Wait(this.MessageReceived);
        }

        [LuisIntent("Farewell")]
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Name")]
        public async Task Name(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var myKey = AnswerDatabase.IndexOf("Name");
            Question[index: myKey] = true;
            DatabaseConnector databaseConnector = new DatabaseConnector();
            var message = await activity;
            applicantID = databaseConnector.insertDatabaseEntry("Name", message.Text);
            await context.PostAsync(result.TopScoringIntent.Intent.ToString());
            context.Wait(this.MessageReceived);
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
        public async Task Answer(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var myKey = AnswerDatabase.IndexOf(result.TopScoringIntent.Intent.ToString());
            Question[myKey] = true;
            DatabaseConnector databaseConnector = new DatabaseConnector();
            var message = await activity;
            databaseConnector.updateDatabase(result.TopScoringIntent.Intent.ToString(), applicantID, message.Text);
            if (result.TopScoringIntent.Intent.ToString().Equals("Job"))
            {
                String[] jobList = databaseConnector.getStellenDBEntry();
                jobID = Array.IndexOf(jobList, message.Text);
            }
            await context.PostAsync(result.TopScoringIntent.Intent.ToString());
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


        [LuisIntent("Greetings")]
        public async Task Greetings(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hallo User!");
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Acceptance")]
        public async Task Acceptance(IDialogContext context, LuisResult result)
        {
            await FindAcceptance(context, true);
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Negative")]
        public async Task Negative(IDialogContext context, LuisResult result)
        {
            await FindAcceptance(context, false);
            context.Wait(this.MessageReceived);
        }

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
                if (result)
                {
                    du = 1;
                }
                else
                {
                    du = 2;
                }
                await context.PostAsync("Dann fangen wir mal an!");
            }
        }
    }
}