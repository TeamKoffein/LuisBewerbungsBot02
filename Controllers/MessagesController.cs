using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Bewerbungs.Bot.Luis
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Die Post Methode fängt sämtliche ankommenden Nachrichten ab und verteilt diese an die jeweiligen passenden Stellen.
        /// Es ist dabei egal, ob die eingehende Nachricht von einem Messenger Dienst oder von einer externen API kommt.
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            // es wird geprüft, ob die ankommende Nachricht eine "Message" ist. Messages sind die Nachrichten, die von Chat-Diensten, wie z.B. Skype kommen.
            //Anschließend wird diese Nachricht an den Bot gegeben, um diese zu verarbeiten.
            if (activity.GetActivityType() == ActivityTypes.Message)
            {
                activity.Locale = "de-DE";
                await Conversation.SendAsync(activity, () => new SuperDialog());
            }//Ankommende "Event" Nachrichten, die von den externen Azure Functions an den Bot gesendet werden. 
            else if (activity.Type == ActivityTypes.Event)
            {
                IEventActivity triggerEvent = activity;
                var message = JsonConvert.DeserializeObject<Message>(((JObject)triggerEvent.Value).GetValue("Message").ToString());
                var messageactivity = (Activity)message.RelatesTo.GetPostToBotMessage();

                var client = new ConnectorClient(new Uri(messageactivity.ServiceUrl));
                var triggerReply = messageactivity.CreateReply();
                triggerReply.Text = $"{message.Text}";
                await client.Conversations.ReplyToActivityAsync(triggerReply);
            }
            else
            {//Verarbeitung von "Systemnachrichten"
                HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        //Systemnachrichten sind Eingaben, die nicht explizit vom User verschickt wurden, auf die der Bot aber reagieren kann.
        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
                IConversationUpdateActivity iConversationUpdated = message as IConversationUpdateActivity;
                if (iConversationUpdated != null)
                {
                    ConnectorClient connector = new ConnectorClient(new System.Uri(message.ServiceUrl));

                    foreach (var member in iConversationUpdated.MembersAdded ?? System.Array.Empty<ChannelAccount>())
                    {
                        // Der User bekommt bei der Verbindung die Möglichkeit angezeigt dem Bot zu schreiben.
                        if (member.Id == iConversationUpdated.Recipient.Id)
                        {
                            var reply = ((Activity)iConversationUpdated).CreateReply();

                            reply.SuggestedActions = new SuggestedActions() {
                                Actions = new List<CardAction>() {
                                    new CardAction()
                                    {
                                        Title = "Hallo Bot", Type = ActionTypes.ImBack, Value = "Hallo"
                                    }
                                }
                            };
                            connector.Conversations.ReplyToActivity(reply);
                        }
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }

    //Festlegung der Message Klasse, über die die gesamte Kommunikation des Bots läuft. Es wird dafür eine grundlegende Struktur festgelegt an die sich die Kommunikation halten muss
    public class Message
    {
        public ConversationReference RelatesTo { get; set; }
        public String Text { get; set; }
    }
}