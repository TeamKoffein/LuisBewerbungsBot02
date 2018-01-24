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
        private string[,] quiz;
        private int bewerberScore;
        private int lenght;
        List<int> questions = new List<int>();
        private int appID;

        public AskingQuestions(int appID)
        {
            this.appID = appID;
        }
        //Ausgabe aller Jobs
        public async Task StartAsync(IDialogContext context)
        {
            var rnd = new Random();
            questions = Enumerable.Range(0,12).OrderBy(x => rnd.Next()).Take(10).ToList();
            DatabaseConnector databaseConnector = new DatabaseConnector();
            //Hole Fragen und Antworten aus DB
            quiz = databaseConnector.getQuizDBEntry(); 
            //Setze Score einmalig zum Start auf 0 Punkte
            bewerberScore = 0;
            lenght = 10;
            await context.PostAsync("Es werden insgesamt 10 Fragen gestellt, von denen 5 Fragen beantwortet werden müssen. Die Fragen, die beantwortet wurden, werden aus dem Katalog entfernt.");
            var reply = context.MakeMessage();
            await sendCarousel(context, reply);

            context.Wait(this.MessageReceivedAsync);
        }
        

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

        private async Task sendCarousel(IDialogContext context, IMessageActivity reply)
        {
            IMessageActivity message = (IMessageActivity)reply;
            message.AttachmentLayout = AttachmentLayoutTypes.List;
            message.Text = "";
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            message.Attachments = CardsAttachment();
            await context.PostAsync(message);
        }

    
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            string answer = message.Text;
            var reply = context.MakeMessage();
            //questionNumber enthaelt Nummer der Frage
            int[] posAndScoreForThisAnswer = findPosAndScoreForThisAnswer(answer);

            if(posAndScoreForThisAnswer[0] == -1 && posAndScoreForThisAnswer[0] == -1)
            {
                await sendCarousel(context, reply);
                context.Wait(this.MessageReceivedAsync);
            }
            lenght = lenght - 1;
            if(lenght == 5)
            {
                bewerberScore = bewerberScore + posAndScoreForThisAnswer[1];
                DatabaseConnector databaseConnector = new DatabaseConnector();
                databaseConnector.setScore(appID, bewerberScore);
                await context.PostAsync("Der erreichte Score ist: "+ bewerberScore.ToString());
                context.Done(bewerberScore);
            }
            else
            {
                questions.RemoveAt(posAndScoreForThisAnswer[0]);
                bewerberScore = bewerberScore + posAndScoreForThisAnswer[1];
                await sendCarousel(context, reply);
                context.Wait(this.MessageReceivedAsync);
            }
        }

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