using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Bewerbungs.Bot.Luis;

namespace Bewerbungs.Bot.Luis
{
    //Diese Klasse listet alle aktuell hinterlegten Fachfragen aus der DB auf 
    [Serializable]
    public class AskingQuestions : IDialog<object>
    {
        string title;
        private Boolean[] answeredQuestions;
        private string[][] quiz;
        int bewerberScore;

        //Array mit der festen Zuordnungen der Antworten (Wert im Array) auf die Fragen (Index des Arrays = Fragennummer)
        //Zeilen = Erste Antwortoption, zweite Antwortoption, dritte Antwortoptionen
        //Spalten = Erste Frage, Zweite Frage, usw.
        string[][] solutionArray = new string[][]
        {
                new string[] {"0", "3", "6", "9", "12", "15", "18", "21", "24", "27", "30", "33"},
                new string[] {"1", "4", "7", "10", "13", "16", "19", "22", "25", "28", "31", "34"},
                new string[] {"2", "5", "8", "11", "14", "17", "20", "23", "26", "29", "32", "35"}
        };


        public AskingQuestions(string title)
        {
            this.title = title;
        }

        //Ausgabe aller Jobs
        public async Task StartAsync(IDialogContext context)
        {
            DatabaseConnector databaseConnector = new DatabaseConnector();
            //Hole Fragen und Antworten aus DB





            //quiz = databaseConnector.getQuizDBEntry(); //###








            //Setzte alles im Boolean Array auf false (alle Fragen unbeantwortet)
            for (int i = 0; i < answeredQuestions.Length; i++) { answeredQuestions[i] = false; }
            //Setze Score einmalig zum Start auf 0 Punkte
            bewerberScore = 0;

            var reply = context.MakeMessage();
            await sendCarousel(context, reply);

            context.Wait(this.MessageReceivedAsync);
        }

        private async Task sendCarousel(IDialogContext context, IMessageActivity reply)
        {
            IMessageActivity message = (IMessageActivity)reply;
            message.AttachmentLayout = AttachmentLayoutTypes.List;
            message.Text = "";
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            message.Attachments = CardsAttachment();
            await context.PostAsync(message);
        }

        public IList<Attachment> CardsAttachment()
        {
            List<Attachment> attach = new List<Attachment>();
            //Array mit den Fragen und moeglichen Antworten
            int cardLength = quiz[0].Length;
            //Anzahl der Karten
            int cardCounter = 0; 
            //Anzahl der Antworten
            int answerCounter = 0;

            //Erstellt Karte fuer jede Fachfrage, wegen Skype auf 10 begrenzt
            while (cardCounter < cardLength && cardCounter < 10) 
            {
                //Anzahl der Antworten pro Karte
                int optionCounter = 1;

                List<CardAction> Buttons = new List<CardAction>();
                
                //Fuegt Antwortungsmoeglichkeiten jeder Fachfrage pro Karte hinzu
                while (optionCounter<4) 
                {
                    Buttons.Add(new CardAction(
                         text: quiz[cardCounter][optionCounter],
                            value: answerCounter,
                            type: ActionTypes.ImBack,
                            displayText: quiz[cardCounter][optionCounter],
                            title: quiz[cardCounter][optionCounter]
                    ));
                    answeredQuestions[answerCounter] = true;
                    answerCounter++;
                    optionCounter++;
                }
                //Zaehle Anzahl der Karte hoch, da Karte neu erstellt 
                cardCounter++;
                attach.Add(GetThumbnailCard("Fachfragen", quiz[cardCounter][0], Buttons));
            }
            return attach;
        }

        private Attachment GetThumbnailCard(string title, string text, List<CardAction> cardAction)
        {
            var heroCard = new ThumbnailCard
            {   
                Title = title,
                Subtitle = "Bitte waehle die richtige Antwort aus.",
                Text = text,
                Images = new List<CardImage>() { new CardImage(url: "https://t2.ftcdn.net/jpg/00/60/83/19/500_F_60831978_c24ahi9gJDOsefFT6lvt3VOVjwgeXxZz.jpg") },
                Buttons = cardAction
            };
            return heroCard.ToAttachment();
        }

        //Methode: Finde die zu der Nummer der Antwort zugehoerige Nummer der Antwortoption (0, 1 oder 2) oder Fragennummer (0-11)
        private int FindSolutionToAnswer(String number, int typ)
        {
            //typ=0: Gib Fragennummer zurueck, typ=1: Gib Anwortoption zurueck

            //Nummer der Antwort, die der Nutzer ausgewaehlt hat
            string answerNumber = Convert.ToString(number);

            //Return Wert = Ausgewaehlte Antwort, die 0, 1 oder 2 betragen kann, um mit der in der DB hinterlegten Loesung zu vergleichen
            int solutionNumber = -1;

            //j gibt Fragennummer (Index des Arrays) zurueck
            //i gibt Antwortoptionen 0, 1 oder 2 zurueck (mit DB Wert zu vergleichen)
            for (int i = 0; i < solutionArray.Length; i++)
            {
                for (int j = 0; j < solutionArray.Length; j++)
                {
                    //Wenn zugehoerige Fragennummer erfragt wird 
                    if ((answerNumber == solutionArray[i][j]) && (typ==0))
                    {
                        return solutionNumber = j;
                    }
                    //Wenn Antwortoption erfragt wird
                    else
                    {
                        return solutionNumber = i;
                    }
                }
            }
            return solutionNumber;
        }
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = context.MakeMessage();
            Boolean endLoop = false;
            String answerNumber =result.ToString();
            //questionNumber enthaelt Nummer der Frage
            int questionNumber = FindSolutionToAnswer(answerNumber, 0);
            //solutionNumber enthaelt vom Nutzer gewaehlte Antwortoption 
            String solutionNumber = Convert.ToString(FindSolutionToAnswer(answerNumber, 1));
            //Score des Bewerbers
            

            //Vergleiche vom Nutzer gewaehlte Antwortoption mit der in der Datenbank hinterlegten, richtigen Antwortoption
            if (solutionNumber == quiz[questionNumber][4])
            {
                //Addiere aktuellen Punktewert des Bewerbers mit der in der DB hinterlegten Anzahl der Punkte
                bewerberScore = bewerberScore + Convert.ToInt32(quiz[questionNumber][5]);

                //#AN DB ZU SENDEN
            }

            while (endLoop)
            for (int i = 0; i < answeredQuestions.Length; i++)
            {
                if (answeredQuestions[i] == false)
                {
                   await sendCarousel(context, message);
                    endLoop = true;
                }
                else
                {
                    context.Done(true);
                }
            }
        }


    }
}