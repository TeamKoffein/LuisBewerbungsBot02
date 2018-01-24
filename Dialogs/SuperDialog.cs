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
using System.Web.Configuration;
using Microsoft.Bot.Builder.Location;

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
            "Technology", "overtime", "Office", "average Age", "Requirements"};
        List<string> AnswerDatabase = new List<string>() {"", "Job", "Name", "Adress", "PostalCode", "Place", "PhoneNumber", "Email", "Birthday",
             "Career", "EducationalBackground", "ProgrammingLanguage", "SocialEngagement", "Language", "PrivateProjects",
            "StartDate"};
        List<bool> Question = new List<bool>() {true, false, false, false, false, false, false, false, false, false, false,
            false, false, false, false, false};
        List<bool> QuestionsYesNo = new List<bool>() {true, true, true, true, true, true, true, true, true, true, true,
            false, false, false, false, true};
        //Abspeicherung der Entities zu ihren Intens
        Dictionary<string, List<string>> EntityTranslation = new Dictionary<string, System.Collections.Generic.List<string>>();
        string[] askingPersonal;//Du-Fragen
        string[] askingFormal;//Sie-Fragen
        string[] currentData;//Speicherung der bisherigen Eingaben des Nutzers
        bool saveDataConfirmation; //bool für die Speicherung der Daten
        bool currentUpload; //abprüfen, ob aktuell ein Upload verlangt wird
        bool dataReviewed;//Überprüfung der Daten abgeschlossen
        int jobID = -1; //Festlegung auf welche Stelle der Bewerber die Bewerbung abgibt. -1 als Kontrollwert, dass diese noch nicht festgelegt wurd
        int sendDataConfirmation = -2; 
        /*
         * Daten an Recruiter senden
         * -2 nicht überprüft, nicht relevant
         * -1 nicht überprüft, relevant
         * 0 nicht akzeptiert
         * 1 akzeptiert
         */
        int saveDataLongterm = -2;
        /*
         * Speicherung der Daten für zukünftige Bewerbungen
         * Zahlenwerte siehe sendataConfirmation
         */
        int correctData = -2;
        /*
         * Daten wurden korrigiert
         * Zahlenwerte siehe sendDataConfirmation
         */
        int saveNewsConfirmation = -2;
        /*
         * Anmeldung für den Newsletter
         * Zahlenwerte siehe senDataConfirmation
         */
        string applicantName; //Bewerbername
        string strasse; //Zwischenspeicher für die Adresse
        string city;//Zwischenspeicher für die Stadt
        string postalcode;//Zwischenspeicher für die PLZ

        //knowledge dient zur Erkennung der Gesprächsfortsetzung
        // -1= Wert nicht gesetzt; 0= neuer Bewerber; 1= gespräch wird fortgesetzt
        int knowledge = -2;

        //1= Du , 2 = Sie
        int du = -1;
        int applicantID = -1;

        //Bot bekommt die erste Nachricht als Kontaktaufnahme
        bool firstMessage = false;

        
        //Geo Koordinaten vom Facebook Messenger erhalten und an Bing weiter versendet
        public async Task ReceivedLocation(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            var place = message.Entities?.Where(t => t.Type == "Place").Select(t => t.GetAs<Place>()).FirstOrDefault();

            if (place != null && place.Geo != null && place.Geo.latitude != null && place.Geo.longitude != null)
            {
                double latitude = (double)place.Geo.latitude;
                double longitude = (double)place.Geo.longitude;
                string geocode = latitude.ToString() + " " + longitude.ToString();
                var bingTrigger = new JsonFileBing
                {
                    RelatesTo = context.Activity.ToConversationReference(),
                    Origin = geocode,
                    Destination = "Am Butzweilerhofallee 2, Köln"
                };
                await AddMessageToQueueAsync(JsonConvert.SerializeObject(bingTrigger), "bingtrigger");
                context.Wait(this.MessageReceived);
            }
            else
            {
                await context.PostAsync("ungültige Ortsangabe");
                await this.MessageReceived(context, argument);
            }
        }

        //StartAsync steht für den Gesprächbeginn
        override public async Task StartAsync(IDialogContext Chat)
        {
            ConfirmationCard confirm = new ConfirmationCard();
            DatabaseConnector databaseConnector = new DatabaseConnector();
            //Initialisierung der Grundvariablen
            saveDataConfirmation = false;

            //Speicherung der Fragen für Du und Sie
            askingPersonal = databaseConnector.getFAQQuestions(1);
            askingFormal = databaseConnector.getFAQQuestions(2);

            var relatesTo = new JsonFileRelatesTo
            {
                relatesTo = Chat.Activity.ToConversationReference(),
                ConversationID = Chat.Activity.Conversation.Id
            };
            await AddFileToBlobbAsynch("relatesto", Chat.Activity.Conversation.Id + ".json", JsonConvert.SerializeObject(relatesTo));

            //Willkommenstext und Datenschutzerklaerung beim Starten des Bots
            string willkommensText = "Herzlich Willkommen bei unserem Bewerbungsbot! Wir freuen uns, dass du dich für eine unserer Stellen interessierst.";
            string datenschutzText = "Wir würden gerne die erhaltenen Daten speichern. Bitte bestätige uns die Datenschutzerklärung, die über folgenden Link aufgerufen werden kann:";
            string datenschutzLink = "http://luisbewerbungsbot.azurewebsites.net/PrivacyPolicy.html";
            string messageText = willkommensText + "\n\n" + datenschutzText + "\n\n" + datenschutzLink;
            await Chat.PostAsync(messageText);
            string text = ("Eine kurze Bestätigung reicht uns voll und ganz!");
            await Chat.PostAsync(confirm.AttachedData(Chat, text));
            Chat.Wait(this.MessageReceived);
        }

        [LuisIntent("")]
        [LuisIntent("None")]
        //Abfang von Luis nicht einordbaren Bewerberaussagen
        public async Task None(IDialogContext context, LuisResult result)
        {
            string text = result.Query;
            if (!firstMessage)
            {
                firstMessage = true;
            }
            else if (!saveDataConfirmation)
            {
                await context.PostAsync("Bitte Bestätigen sie die Datenschutzerklärung");
            }
            else if (result.Query == "help" || result.Query == "Help")
            {
                string message = "Tippen Sie 'upload' um Daten an den Bot zu senden, 'xing' um ihr Xing-Profil anzugeben oder stellen Sie mir eine Frage";
                await context.PostAsync(message);
            }
            else
            {
                string message = $"Ich habe '{result.Query}' leider nicht verstanden. Versuch doch bitte es für mich noch einmal anders zu formulieren. Falls diese Nachricht mehr als einmal erscheint, dann schreib bitte eine Mail an teamkoffein@outlook.de mit deiner Anfrage. Wir werden zeitnah antworten!";
                await context.PostAsync(message);
                context.Wait(this.MessageReceived);
            }
            
        }

        //Verabschiedung
        [LuisIntent("Farewell")]
        public async Task Farewell(IDialogContext context, LuisResult result)
        {
            if (result.TopScoringIntent.Score.Value >= 0.5)
            {
                String message = "Bis zum nächsten Mal!";
                await context.PostAsync(message);
                context.Wait(this.MessageReceived);
            }
            else
            {
                await None(context, result);
            }
        }

        [LuisIntent("Name")]
        //Die erste Frage, die dem Bewerber gestellt werden soll, da durch die Eingabe die des Namens eine neue BewerberID vergeben wird, die notwendig ist um alle folgenden 
        // Antworten zu speichern.
        //Weitergabe an AfterName
        public async Task Name(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            if (!firstMessage)
            {
                firstMessage = true;
            }
            else if (!saveDataConfirmation)
            {
                await context.PostAsync("Bitte Bestätigen sie die Datenschutzerklärung");
            }
            else
            {
                if (result.TopScoringIntent.Score.Value >= 0.5)
                {
                    var message = await activity;
                    Text = message.Text;
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
                        applicantName = message.Text;
                        knowledge = -1;
                        string kenntnis = "";
                        if (du == 0)
                        {
                            kenntnis = "Kennen sie mich schon?";
                        }
                        else
                        {
                            kenntnis = "Kennst du mich schon?";
                        }
                        ConfirmationCard confirm = new ConfirmationCard();
                        await context.PostAsync(confirm.AttachedData(context, kenntnis));
                        context.Wait(this.MessageReceived);
                    }
                }
                else
                {
                    await None(context, result);
                }
            }
        }

        //Dialog, der die Angaben des Nutzers lädt und das Gespräch an der richtigen Stellen fortführt
        private async Task CheckInformation(IDialogContext context, IAwaitable<object> result)
        {
            ConfirmationCard confirm = new ConfirmationCard();
            DatabaseConnector databaseConnector = new DatabaseConnector();
            applicantID = Convert.ToInt32(await result);
            if (applicantID > 0)
            {
                Question = databaseConnector.getMissing(applicantID);
                int index = Question.FindIndex(x => x == false);

                //Falls alle Daten gemacht wurden wird dies abgefangen
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
                    string text = ("Sind diese Angaben korrekt?");
                    await context.PostAsync(confirm.AttachedData(context, text));
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
                //Namenspeicherung
                var myKey = AnswerDatabase.IndexOf("Name");
                Question[index: myKey] = true;
                databaseConnector.updateDB("Name", applicantID, Text);
            }
            //Neue Methode hinzugefügt
            await FindNextAnswer(context, true);
        }

        //Wenn der Bewerber erneut nach den ausgeschriebenen Stellen da, so kann er erneut die Stelle ausählen, auf die er sich bewerben möchte bzw. kann diese ändern
        [LuisIntent("Job")]
        public async Task JobIntent(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            if (!firstMessage)
            {
                firstMessage = true;
            }
            else if (!saveDataConfirmation)
            {
                await context.PostAsync("Bitte Bestätigen sie die Datenschutzerklärung");
            }
            else
            {
                Question[1] = false;
                context.Call(new AskingJob(askingFormal[1]), AfterStellen);               
            }
        }

        [LuisIntent("Adress")]
        [LuisIntent("Birthday")]
        [LuisIntent("Career")]
        [LuisIntent("EducationalBackground")]      
        [LuisIntent("Language")]
        [LuisIntent("Place")]
        [LuisIntent("PostalCode")]
        [LuisIntent("PrivateProjects")]
        [LuisIntent("ProgrammingLanguage")]
        [LuisIntent("SocialEngagement")]
        [LuisIntent("StartDate")]

        //Filtern nach dem erwarteten Intent, Weitergabe an AfterAnswer
        public async Task Answer(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            if (!firstMessage)
            {
                firstMessage = true;
            }
            else if (!saveDataConfirmation)
            {
                await context.PostAsync("Bitte Bestätigen sie die Datenschutzerklärung");
            }
            else
            {
                if (result.TopScoringIntent.Score.Value >= 0.5)
                {
                    var message = await activity;
                    LuisTopIntention = result.TopScoringIntent.Intent.ToString();
                    if (!LuisTopIntention.Equals("PhoneNumber") && !LuisTopIntention.Equals("Email"))
                    {
                        Text = message.Text; //hier einstetzen von GiveEntities(result);
                    }
                    await context.Forward(new Acceptance(result.TopScoringIntent.Intent.ToString(), message.Text), AfterAnswer, message, CancellationToken.None);
                }
                else
                {
                    await None(context, result);
                }
            }
        }

        //Phone Number wird gesondert abgefangen und es wird überprüft, ob die eingegebene Nachricht eine gültige Telefonnummer enthält
        [LuisIntent("PhoneNumber")]
        public async Task Phone(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            string phone = phoneNumber(result.Query);
            if (!String.IsNullOrEmpty(phone))
            {
                Text = phone;
                await Answer(context, activity, result);
            }
            else
            {
                await context.PostAsync("Das ist keine gültige Telefonnummer");
                context.Wait(MessageReceived);
            }
        }

        //Überprüfung, ob die Nachricht aus Phone eine Telefonnummer enthält. Bei leerem Rückgabestring ist die Rufnummer nicht korrekt
        private static string phoneNumber(string message)
        {
            //Filterung, dass nur noch Zahlen enthalten sind. Aussortierung der Buchstaben und Sonderzeichen
            char[] phone = message.ToCharArray();
            message = "";
            for (int i = 0; i < phone.Length; i++)
            {
                if (phone[i].Equals('0') || phone[i].Equals('1') || phone[i].Equals('2') || phone[i].Equals('3') || phone[i].Equals('4') || phone[i].Equals('5') || phone[i].Equals('6') || phone[i].Equals('7') || phone[i].Equals('8') || phone[i].Equals('9'))
                {
                    message = message + phone[i];
                }
            }

            //Entfernen der Landesvorwahl für Deutschland
            if (message.StartsWith("0049"))
            {
                message = message.TrimStart('0', '4');
                message = message.TrimStart('9');
            }
            else if (message.StartsWith("+49"))
            {
                message = message.TrimStart('+', '4');
                message = message.TrimStart('9');
            }
            else if (message.StartsWith("0"))
            {
                message = message.TrimStart('0');
            }
            else //Falls keine deutsche Vorwahl, so wird die Telefonnummer nicht erkannt
            {
                return "";
            }//die korrekte Telefonnummer wird zurückgegeben
            return isValidPhoneNumber(message);
        }

        //Zusammenbauen zu einer richtigen Telefonnummer inkl. Landesvorwahl
        private static String isValidPhoneNumber(string phoneNumber)
        {
            if (phoneNumber.Length > 5 && phoneNumber.Length < 12)
            {
                return "0049" + phoneNumber;
            }
            else
            {
                return "";
            }
        }

        //Aussortierung der E-Mail Eingabe
        [LuisIntent("Email")]
        public async Task Mail(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            string mail = isValidMail(result.Query);
            if (!String.IsNullOrEmpty(mail))
            {
                Text = mail;
                await Answer(context, activity, result);
            }
            else
            {
                await context.PostAsync("Das ist keine gültige E-Mail Adresse");
                context.Wait(MessageReceived);
            }
        }

        //Überprüfung, ob in der eingegeben Nachricht eine Mailadresse enthalten ist
        private String isValidMail(string message)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(message);
                return addr.Address;
            }
            catch
            {
                return "";
            }
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
                    int indexYesNo = QuestionsYesNo.FindIndex(x => x == false);
                    Question[index: myKey] = true;
                    if (myKey == indexYesNo)
                    {
                        QuestionsYesNo[index: indexYesNo] = true;
                    }
                    databaseConnector.updateDB(LuisTopIntention, applicantID, Text);
                    databaseConnector.updateDB("ChannelID", applicantID, context.Activity.ChannelId);
                    databaseConnector.updateDB("ConversationID", applicantID, context.Activity.Conversation.Id);

                }
            }
            //Neue Methode hinzugefügt
            await FindNextAnswer(context, true);
        }

        /*Wenn der Bewerber angegeben hat auf welche Stelle er sich bewerben möchte, wird dies hinterlegt und die dementsprechende Fachfrage zur der Position gestellt
         */
        public async Task AfterStellen(IDialogContext context, IAwaitable<object> result)
        {
            DatabaseConnector databaseConnector = new DatabaseConnector();
            int accept = Convert.ToInt32(await result);
            Question[index: 1] = true;
            databaseConnector.updateDB("Job", applicantID, Convert.ToString(accept));

            string date = databaseConnector.getJobDate(accept);
            await context.PostAsync("Zu diesem Termin stellen wir ein: " + date);
            //Neue Methode hinzugefügt
            context.Call(new AskingQuestions(applicantID), AfterQuestions);
        }
        
        //Nachdem der Bewerber das Quiz beantwortet hat
         public async Task AfterQuestions(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync("Danke, dass du die Quiz-Fragen beantwortet hast!");
            await FindNextAnswer(context, true);
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
        [LuisIntent("average Age")]
        [LuisIntent("Technology")]
        [LuisIntent("Office")]
        [LuisIntent("overtime")]

        //Wenn der Intent nach einer FAQ an Lise erkannt wird, wird der entsprechende Antworteintrag abhängig von der gewählten Anrede ausgelesen.
        public async Task FAQ(IDialogContext context, IAwaitable<IMessageActivity> activity ,LuisResult result)
        {
            IMessageActivity mess = (IMessageActivity)activity;
            if (!firstMessage)
            {
                firstMessage = true;
            }
            else if (!saveDataConfirmation)
            {
                await context.PostAsync("Bitte Bestätigen sie die Datenschutzerklärung");
            }
            else
            {
                if (result.TopScoringIntent.Score.Value >= 0.5)
                {
                    if (saveDataConfirmation)
                    {
                        DatabaseConnector databaseConnector = new DatabaseConnector();
                        int key = FAQDatabase.IndexOf(result.TopScoringIntent.Intent.ToString());
                        if (key < 20)
                        {
                            if (key == 6)
                            {
                                await context.PostAsync(databaseConnector.getDBEntry(key, "SELECT * FROM FAQ WHERE FAQID =@ID", du));
                                if (mess.ChannelId.Equals("facebook"))
                                {
                                    var reply = context.MakeMessage();
                                    reply.ChannelData = new FacebookMessage
                                    (
                                        text: "Wo bist du?",
                                        quickReplies: new List<FacebookQuickReply>
                                        {
                                        new FacebookQuickReply(
                                            contentType: FacebookQuickReply.ContentTypes.Location,
                                            title: default(string),
                                            payload: default(string)
                                    )
                                        }
                                    );
                                    await context.PostAsync(reply);
                                    context.Wait(this.ReceivedLocation);
                                }
                                else
                                {
                                    if(Question[6] == true)
                                    {
                                        string adresse =  strasse + " " + city + " " + postalcode;
                                        var bingTrigger = new JsonFileBing
                                        {
                                            RelatesTo = context.Activity.ToConversationReference(),
                                            Origin = adresse,
                                            Destination = "Am Butzweilerhofallee 2, Köln"
                                        };
                                        await AddMessageToQueueAsync(JsonConvert.SerializeObject(bingTrigger), "bingtrigger");
                                    }
                                }
                            }
                            if (du != -1)
                            {
                                
                            }
                            else
                            {
                                await context.PostAsync("Bitte verrate mir vorher, ob wir beim 'Du' bleiben sollen");
                                context.Wait(this.MessageReceived);
                            }
                        }
                        else
                        {
                            if (jobID > -1)
                            {
                                await context.PostAsync(databaseConnector.getDBEntry(jobID, "SELECT Profil FROM Stellen WHERE StellenID =@ID"));
                            }
                            context.Wait(this.MessageReceived);
                        }
                    }
                    else
                    {
                        await context.PostAsync("Zuerst musst du die Datenschutzerklärung bestätigen.");
                        context.Wait(this.MessageReceived);
                    }
                }
                else
                {
                    await None(context, result);
                }
            }
        }

        //Begrüßen des Bewerbers
        [LuisIntent("Greetings")]
        public async Task Greetings(IDialogContext context, LuisResult result)
        {
            if (!firstMessage)
            {
                firstMessage = true;
            }
            else if (!saveDataConfirmation)
            {
                await context.PostAsync("Bitte Bestätigen sie die Datenschutzerklärung");
            }
            else
            {
                if (result.TopScoringIntent.Score.Value >= 0.5)
                {
                    await context.PostAsync("Hallo User!");
                    context.Wait(this.MessageReceived);
                }
                else
                {
                    await None(context, result);
                }
            }
        }

        //Akzeptanz durch den Bewerber
        [LuisIntent("Acceptance")]
        public async Task Acceptance(IDialogContext context, LuisResult result)
        {
            if (!firstMessage)
            {
                firstMessage = true;
            }
            else
            {
                if (result.TopScoringIntent.Score.Value >= 0.5)
                {
                    //Abspeicherung der Letzten Nachricht, damit eine Abspeicherung in der Datenbank möglich ist
                    Text = "Ja";
                    await FindAcceptance(context, true);
                }
                else
                {
                    await None(context, result);
                }
            }
        }

        //Xing-Anbindung 
        [LuisIntent("Xing")]
        public async Task Xing(IDialogContext context, LuisResult result)
        {
            if (!firstMessage)
            {
                firstMessage = true;
            }
            else if (!saveDataConfirmation)
            {
                await context.PostAsync("Bitte Bestätigen sie die Datenschutzerklärung");
            }
            else
            {
                if (result.TopScoringIntent.Score.Value >= 0.5)
                {
                    await context.PostAsync("http://luisbewerbungsbot.azurewebsites.net/Xing.aspx");
                    context.Wait(this.MessageReceived);
                }
                else
                {
                    await None(context, result);
                }
            }
        }

        //Aufruf der Upload-Funktion
        [LuisIntent("Upload")]
        public async Task Upload(IDialogContext context, LuisResult result)
        {
            if (!firstMessage)
            {
                firstMessage = true;
            }
            else if (!saveDataConfirmation)
            {
                await context.PostAsync("Bitte Bestätigen sie die Datenschutzerklärung");
            }
            else
            {
                if (result.TopScoringIntent.Score.Value >= 0.5)
                {
                    currentUpload = true;
                    context.Call(new Upload(applicantID), AfterAnswer);
                }
                else
                {
                    await None(context, result);
                }
            }
        }


        //Verneinung durch den Bewerber
        [LuisIntent("Negative")]
        public async Task Negative(IDialogContext context, LuisResult result)
        {
            if (!firstMessage)
            {
                firstMessage = true;
            }
            else
            {
                if (result.TopScoringIntent.Score.Value >= 0.5)
                {
                    //Abspeicherung der Letzten Nachricht, damit eine Abspeicherung in der Datenbank möglich ist

                    Text = "Nein";
                    await FindAcceptance(context, false);
                    
                }
                else
                {
                    await None(context, result);
                }
            }
        }

        //Methode zum finden der Nächsten Frage. Diese Methode wurde ausgelagert, da sie sich sonst gedoppelt hat.
        public async Task FindNextAnswer(IDialogContext context, bool needWait)
        {
            ConfirmationCard confirm = new ConfirmationCard();
            DatabaseConnector databaseConnector = new DatabaseConnector();
            databaseConnector.setTime(applicantID);
            int index = Question.FindIndex(x => x == false);
            if (index == -1)
            {
                if (!dataReviewed)
                {
                    dataReviewed = true;
                    currentData = databaseConnector.getData(applicantID);
                    string data = "";
                    for (int i = 0; i < currentData.Length; i++)
                    {
                        data = data + "" + "\n\n" + currentData[i];
                    }
                    await context.PostAsync("Dies sind deine Angaben: " + Environment.NewLine + data);
                    correctData = -1;
                    string text = ("Sind diese Angaben korrekt?");
                    await context.PostAsync(confirm.AttachedData(context, text));
                }

            }

            else
            {
                if (du == 1)
                {
                    if(Question[2] == false)
                    {
                        index = 2;
                    }
                    if (index == 1)
                    {
                        //Frage nach beworbene Stelle mit Du
                        context.Call(new AskingJob(askingFormal[index]), AfterStellen);
                    }
                    else if (2 < index && index <6)
                    {
                        var apiKey = WebConfigurationManager.AppSettings["BingMapsApiKey"];
                        var prompt = "Wie lautet deine Adresse? Bitte gebe hierzu die Straße, Postleitzahl und Stadt an.";
                        var locationDialog = new LocationDialog(apiKey, context.Activity.ChannelId, prompt,  LocationOptions.SkipFavorites | LocationOptions.UseNativeControl, LocationRequiredFields.StreetAddress);
                        context.Call(locationDialog, async (contextIn, result) =>
                        {
                            Place place = await result;
                            if (place != null)
                            {
                                var address = place.GetPostalAddress();
                                strasse = address.StreetAddress;
                                postalcode = address.PostalCode;
                                city = address.Locality;
                                string name = address != null ?
                                    $"{address.StreetAddress},{address.PostalCode} {address.Locality}, {address.Region}, {address.Country}" :
                                    "the pinned location";
                                await contextIn.PostAsync($"Die  Adresse {name} ist abgespeichert.");
                                Question[3] = true;
                                Question[4] = true;
                                Question[5] = true;
                                int indexa = Question.FindIndex(x => x == false);
                                if (Question[2] == false)
                                {
                                    indexa = 2;
                                }
                                await contextIn.PostAsync(askingPersonal[indexa]);
                            }
                            else
                            {
                                await contextIn.PostAsync("Ok, abgebrochen.");
                            }
                        });
                  
                    }
                    else
                    {
                        if (index == 7)
                        {
                            databaseConnector.updateDB("Adress", applicantID, strasse);
                            databaseConnector.updateDB("Place", applicantID, city);
                            databaseConnector.updateDB("PostalCode", applicantID, postalcode);
                        }
                        //Frage nach beworbene Stelle mit Sie
                        await context.PostAsync(askingPersonal[index]);
                        if (needWait == true)
                        {
                            context.Wait(this.MessageReceived);
                        }

                    }
                }
                else
                {
                    if (Question[2] == false)
                    {
                        index = 2;
                    }
                    if (index == 1)
                    {
                        context.Call(new AskingJob(askingFormal[index]), AfterStellen);
                    }
                    else if (2 < index && index < 6)
                    {
                        var apiKey = WebConfigurationManager.AppSettings["BingMapsApiKey"];
                        var prompt = "Wie lautet deine Adresse? Bitte gebe hierzu die Straße, Postleitzahl und Stadt an.";
                        var locationDialog = new LocationDialog(apiKey, context.Activity.ChannelId, prompt,   LocationOptions.SkipFavorites | LocationOptions.UseNativeControl, LocationRequiredFields.StreetAddress);
                        context.Call(locationDialog, async (contextIn, result) =>
                        {
                            Place place = await result;
                            if (place != null)
                            {
                                var address = place.GetPostalAddress();
                                strasse = address.StreetAddress;
                                postalcode = address.PostalCode;
                                city = address.Locality;
                                string name = address != null ?
                                    $"{address.StreetAddress},{address.PostalCode} {address.Locality}, {address.Region}, {address.Country}" :
                                    "the pinned location";
                                await contextIn.PostAsync($"Die  Adresse {name} ist abgespeichert.");
                                Question[3] = true;
                                Question[4] = true;
                                Question[5] = true;
                                int indexa = Question.FindIndex(x => x == false);
                                if (Question[2] == false)
                                {
                                    indexa = 2;
                                }
                                await contextIn.PostAsync(askingPersonal[indexa]);
                            }
                            else
                            {
                                await contextIn.PostAsync("Ok, abgebrochen.");
                            }
                        });
                        
                    }
                    else
                    {
                        if (index == 6)
                        {
                            databaseConnector.updateDB("Adress", applicantID, strasse);
                            databaseConnector.updateDB("Place", applicantID, city);
                            databaseConnector.updateDB("PostalCode", applicantID, postalcode);
                        }
                        await context.PostAsync(askingFormal[index]);
                        if (needWait == true)
                        {
                            context.Wait(this.MessageReceived);
                        }
                    }
                }
            }
        }
        //Abfrage der Anrede nach Bestätigung der Datenschutzerklärung, sowie Abfrage bei allen anderen Szenarien
        /*
         * In diesem Dialog werden, abhängig von dem Gesprächsfortschritt, dem Bewerber Bestätigungsfragen gestellt.
         * Es wird gefragt nach der Anrede, ob der Bewerber den Bot kennt und sein Gespräch fortsetzen will, die Daten korrekt hinterlegt sind,
         * ob die Daten übermittelt und langfristig gespeichert werden dürfen sowie die Frage ob der Bewerber einen Newslettererhalten will.
         */
        public async Task FindAcceptance(IDialogContext context, bool result)
        {
            ConfirmationCard confirm = new ConfirmationCard();
            DatabaseConnector databaseConnector = new DatabaseConnector();
            int index = Question.FindIndex(x => x == false);
            int indexYesNo = QuestionsYesNo.FindIndex(x => x == false);
            if (!saveDataConfirmation)
            {
                //Bestätigung der Datenschutzerklärung und Frage nach Anrede
                if (result)
                {
                    saveDataConfirmation = true;
                    string conversationID = context.Activity.Conversation.Id;
                    string userID = context.Activity.From.Id;
                    string id = context.Activity.Recipient.Id;
                    string channel = context.Activity.ChannelId;
                    applicantID = databaseConnector.insertNewApp(conversationID, userID, channel);
                    string text = "Danke für das Akzeptieren der Datenschutzerklärung. Darf ich sie duzen?";
                    await context.PostAsync(confirm.AttachedData(context, text));
                }
                else
                {
                    await context.PostAsync("Wenn du unsere Datenschutzerklärung nicht bestätigst kannst du nicht mit dem Bot schreiben!");
                }
            }
            //Antwort ob der Bot dem Bewerber bekannt ist
            else if (knowledge == -1)
            {
                //Fall unbekannt. Neuer Bewerber wird angelegt
                if (!result)
                {
                    knowledge = 0;
                    var myKey = AnswerDatabase.IndexOf("Name");
                    Question[index: myKey] = true;
                    databaseConnector.updateDB("Name", applicantID, applicantName);
                    await FindNextAnswer(context, false);                    
                }
                //Fall bekannt. Frage nach Email-Adresse
                else
                {
                    knowledge = 1;
                    context.Call(new CheckEmail(applicantName), CheckInformation);
                }
            }
            else if (du == -1)
            {
                await context.PostAsync("Dann fangen wir mal an!");
                //Anrede Fall Du
                if (result)
                {
                    du = 1;
                    await context.PostAsync(askingPersonal[2]);

                }
                //Anrede Fall Sie
                else
                {
                    du = 2;
                    await context.PostAsync(askingFormal[2]);
                }
            }
            //Frage nach Korrektheit
            else if (correctData == -1)
            {
                if (result)
                {
                    correctData = 1;
                    sendDataConfirmation = -1;
                    string text = ("Darf ich die Daten an den Recruiter übermitteln?");
                    await context.PostAsync(confirm.AttachedData(context, text));
                }
                else
                {
                    await context.PostAsync("Welche Angabe ist falsch?");
                }
            }
            //Frage nach Datenübermittlung
            else if (sendDataConfirmation == -1)
            {
                if (result)
                {
                    saveDataLongterm = -1;
                    sendDataConfirmation = 1;
                    await context.PostAsync("Dann versende ich die Daten an den Recruiter.");
                    DataAssembler assemble = new DataAssembler();
                    assemble.sendData(applicantID);
                    string text = ("Möchtest du deine Daten dauerhaft speichern?");
                    await context.PostAsync(confirm.AttachedData(context, text));
                }
                else
                {
                    saveDataLongterm = -1;
                    sendDataConfirmation = 0;
                    string text = ("Deine Daten werden nicht übermittelt. Möchtest du deine Daten trotzdem dauerhaft speichern?");
                    await context.PostAsync(confirm.AttachedData(context, text));
                }
            }
            //Frage nach langfristiger Speicherung
            else if (saveDataLongterm == -1)
            {
                saveNewsConfirmation = -1;
                if (result)
                {
                    saveDataLongterm = 1;
                    databaseConnector.transferData(applicantID);
                    string text = ("Die Daten werden dauerhaft gespeichert. Sollen wir die angegebene Email-Adresse für unseren Newsletter eintragen?");
                    await context.PostAsync(confirm.AttachedData(context, text));
                }
                else
                {
                    saveDataLongterm = 0;
                    string text = ("Die Daten werden nicht gespeichert. Sollen wir die angegebene Email-Adresse für unseren Newsletter eintragen?");
                    await context.PostAsync(confirm.AttachedData(context, text));
                }
            }
            //Frage nach Newsletterabonnierung
            else if (saveNewsConfirmation == -1)
            {
                if (result)
                {
                    saveNewsConfirmation = 1;
                    databaseConnector.updateNewsletter(applicantID);
                    await context.PostAsync("Vielen Dank für die Akzeptierung unseres Newsletters.");
                    await context.PostAsync("Wir sind hier mit unseren Fragen fertig. Deine Daten werden an den Recruiter übermittelt, aber du kannst mir gerne weiterhin Fragen stellen.");
                }
                else
                {
                    saveNewsConfirmation = 0;
                    await context.PostAsync("Es werden keine Newsletter an die angegebene Email-Adresse verschickt.");
                    await context.PostAsync("Wir sind hier mit unseren Fragen fertig. Deine Daten werden an den Recruiter übermittelt, aber du kannst mir gerne weiterhin Fragen stellen.");
                }
            }
            //Neu Hinzugefügte Abfrage, welche bei einer Seperaten Liste checkt, ob man diese Frage mit Ja oder nein beantworten kann.
            else if (index != -1 && index == indexYesNo)
            {
                Question[index] = true;
                QuestionsYesNo[index] = true;
                await FindNextAnswer(context, false);
            }
            

            //Hinzufügen eines nicht abgehandelten falls, dass eine Frage gestellt wird.
            else
            {
                await FindNextAnswer(context, false);
            }

        }
        //Methode zum Suchen und wiedergabe des/der Value/s des/der Entity/ies des angegeben Top Scoring Intent
        public string GiveEntities(LuisResult result)
        {
            string returnResult = null;
            string topIntent = result.TopScoringIntent.Intent.ToString();
            var topEntities = EntityTranslation[topIntent];
            var listEntities = result.Entities;
            foreach (var te in topEntities)
            {
                foreach (var le in listEntities)
                {
                    if (te.Equals(le.Type))
                    {
                        if (string.IsNullOrEmpty(value: returnResult))
                        {
                            returnResult = le.Entity;
                        }
                        else
                        {
                            returnResult = returnResult + " " + le.Entity;
                        }

                    }
                }
            }
            return returnResult;
        }
        public static async Task AddMessageToQueueAsync(string message, string queueName)
        {
            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

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

        public static async Task AddFileToBlobbAsynch(string containerName, string path, string jsonFile)
        {
            //string path = activity.Conversation.Id + ".txt";
            string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            CloudStorageAccount csa = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = csa.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(path);
            using (StreamWriter writer = new StreamWriter(blob.OpenWrite()))
            {
                writer.Write(value: jsonFile);
            }
        }
    }

    public class JsonFileBing
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public ConversationReference RelatesTo { get; set; }
    }

    public class JsonFileRelatesTo
    {
        public string ConversationID { get; set; }
        public ConversationReference relatesTo { get; set; }
    }
}