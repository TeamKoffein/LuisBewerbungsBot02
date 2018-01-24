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
    //Diese Klasse stellt 10 Fragen aus dem Pool, welcher bei der DB hinterlegt wurden ist
    [Serializable]
    public class AskingQuestions : IDialog<object>
    {
        /**2 Diminsionales Array 
         *[,0] Frage
         *[,1-3] Antwortmöglichkeiten
         *[,4] enthält Zahlen im Raum 1-3 zum Angeben, welche Antwortmöglichkeit richtig ist 
         *[,5] Punkte bei richtiger Beantwortung
         */
        private string[,] quiz;
        //Punkteanzahl des Bewerber
        private int bewerberScore;
        //Länge des Karussells zum Darstellen der Fragen
        private int lenght;
        //Liste mit den Zufäligen Fragen, welche gestellt werden sollen
        List<int> questions = new List<int>();
        private int appID;

        //Konstrakter, welcher die Applicantion ID übergeben bekommen muss
        public AskingQuestions(int appID)
        {
            this.appID = appID;
        }
        //Erste Methode, welche aufgerufen wird. Postet zuerst eine Anleitung und das erste Karussell zur Ausgabe aller Fragen
        public async Task StartAsync(IDialogContext context)
        {
            //Setze Score einmalig zum Start auf 0 Punkte und lege die anfängliche Länge des Karussells Fest
            bewerberScore = 0;
            lenght = 10;
            //Object von Random erschaffen um aus dem Pool der Fragen aus der Var lenght zu ziehen und in die Liste questions abzuspeichern
            var rnd = new Random();
            questions = Enumerable.Range(0,12).OrderBy(x => rnd.Next()).Take(lenght).ToList();
            DatabaseConnector databaseConnector = new DatabaseConnector();
            //Hole Fragen und Antworten aus DB
            quiz = databaseConnector.getQuizDBEntry(); 
            
            await context.PostAsync("Es werden insgesamt 10 Fragen gestellt, von denen 5 Fragen beantwortet werden müssen. Die Fragen, die beantwortet wurden, werden aus dem Katalog entfernt.");
            var reply = context.MakeMessage();
            //Aufruf zum Erschaffen und Senden des Karussells
            await sendCarousel(context, reply);

            context.Wait(this.MessageReceivedAsync);
        }
        
        // Methode zum Erstellen der Attachments bzw. Buttons der einzelnen Cards
        public IList<Attachment> CardsAttachment()
        {
            List<Attachment> attach = new List<Attachment>();
            for(int outLoop = 0; outLoop < lenght; outLoop++)
            {
                List<CardAction> Buttons = new List<CardAction>();
                for(int inLoop = 1; inLoop < 4; inLoop++)
                {
                    string value = quiz[questions[outLoop], 0] + inLoop.ToString();
                    Buttons.Add(new CardAction(
                        text: quiz[questions[outLoop], inLoop],
                        value: value,
                        type: ActionTypes.ImBack,
                        displayText: quiz[questions[outLoop], inLoop],
                        title: quiz[questions[outLoop], inLoop]
                        )
                    );
                }
                string titel = "Frage" + (outLoop+1).ToString();
                attach.Add(GetThumbnailCard(titel, quiz[questions[outLoop], 0] , Buttons));
            }
            
                
            
            return attach;
        }
        //Methode zum erschaffen der Cards
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
        //Methode zum Zusammensetzten der Cards zum Karussel und Erstellen und Senden der Nachricht
        private async Task sendCarousel(IDialogContext context, IMessageActivity reply)
        {
            IMessageActivity message = (IMessageActivity)reply;
            message.AttachmentLayout = AttachmentLayoutTypes.List;
            message.Text = "";
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            message.Attachments = CardsAttachment();
            await context.PostAsync(message);
        }

        //Methode zum Behandeln der einkommenden Nachrichten bzw. Antworten
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            string answer = message.Text;
            var reply = context.MakeMessage();
            //posAndScoreForThisAnswer enthaelt bei Position 0 die Position der Frage und bei Position 1 die Punkte Anzahl. 
            //Falls die Nachricht keinen Bezug zu den Fragen hat, wird das Ursprüngliche Karussell ausgegeben.
            int[] posAndScoreForThisAnswer = findPosAndScoreForThisAnswer(answer);

            if(posAndScoreForThisAnswer[0] == -1 && posAndScoreForThisAnswer[0] == -1)
            {
                await context.PostAsync("Diese Aussage war keine Antwortmöglichkeit. Bitte wähle einen Button als Antwort aus. Fragen an den Bot kannst du nach diesen Fragen Stellen.");
                await sendCarousel(context, reply);
                context.Wait(this.MessageReceivedAsync);
            }
            else
            {
                lenght = lenght - 1;
                //Wenn 5 Fragen erreicht werden wird der Score in der DB abgespeichert und der Dialog beendet.
                if(lenght == 5)
                {
                    bewerberScore = bewerberScore + posAndScoreForThisAnswer[1];
                    DatabaseConnector databaseConnector = new DatabaseConnector();
                    databaseConnector.setScore(appID, bewerberScore);
                    await context.PostAsync("Der erreichte Score ist: "+ bewerberScore.ToString());
                    context.Done(bewerberScore);
                }
                //Falls der Obere Fall nicht eingetroffen ist, wird das Karussell ohne die beantworte Frage dargestellt
                else
                {
                    questions.RemoveAt(posAndScoreForThisAnswer[0]);
                    bewerberScore = bewerberScore + posAndScoreForThisAnswer[1];
                    await sendCarousel(context, reply);
                    context.Wait(this.MessageReceivedAsync);
                }
            }
        }
        //Suche nach der Position und der Wertung und gibt diese in einem Int Array wieder. Bei einer Flascheingabe wird [-1,-1] gepostet.
        private int[] findPosAndScoreForThisAnswer(string answer)
        {
            int[] returnArray = new int[2] {-1, -1};
            string lastChar = answer.Substring(answer.Length - 1, 1);
            int pos;
            bool isNumeric = int.TryParse(lastChar, out pos);
            if (!isNumeric)
            {
                return new int[2] { -1, -1 };
            }

            for (int outLoop = 0; outLoop < lenght; outLoop++)
            {
                List<CardAction> Buttons = new List<CardAction>();
                for (int inLoop = 1; inLoop < 4; inLoop++)
                {
                    string value = quiz[questions[outLoop], 0] + inLoop.ToString();
                    if (value.Equals(answer))
                    {
                        returnArray[0] = outLoop;
                        returnArray[1] = 0;
                        if (inLoop == Convert.ToInt32(quiz[questions[outLoop], 4]))
                        {
                            returnArray[1] = Convert.ToInt32(quiz[questions[outLoop], 5]);
                            return returnArray;
                        }
                        else
                        {
                            return returnArray;
                        }
                    }
                }
            }
            return new int[2] { -1, -1 };
        }
    }
}