using Microsoft.Bot.Builder.Dialogs;
using System;

namespace Bewerbungs.Bot.Luis
{
    /*
    FAQAnswers als gesamte Klassenstruktur beschreibt die Reaktion auf eine eingegangene Anfrage des Bewerbers über das Textfeld. Diese Eingabe wird mit vorgegebenen möglichen Eingaben verglichen.
    Daraus resultierend wird aus der Datenbank die passende Antwort abgerufen und über das Textfeld ausgegeben. 
    */
    [Serializable]
    public class FAQDialog
    {
        //Es wird eine Error Message festgelegt, die ausgegeben wird wenn keines der vorgegebenen Wörter auf die Abfrage des Bewerbers zutreffen
        String ErrorMessage = "Leider habe ich dich nicht verstanden. Bitte versuch es doch noch einmal! :)";
        //Die Methode answerFAQ gibt einen String zurück, der anschließend über die postAnswer ausgegeben wird. Hier erfolgt der Abgleich von vorgegeben Formulierungen mit der Aussage des Bewerbers.
        public string answerFAQ(string message, int stellenID)
        {
            DatabaseConnector databaseConnector1 = new DatabaseConnector();
            String faqDBQuery = "SELECT Aussage FROM FAQ WHERE FAQID =@ID";
            String stellenDBQuery = "SELECT Profil FROM Stellen WHERE StellenID =@ID";

            //Abfrage Urlaubstage
            if (message.Equals("Holiday"))
            {
                return databaseConnector1.getDBEntry(1, faqDBQuery);
            }
            else if (message.Equals("WorkingHours")) //Abfrage Arbeitszeiten
            {
                return databaseConnector1.getDBEntry(2, faqDBQuery);
            }
            else if (message.Equals("Salary")) //Abfrage Gehalt
            {
                return databaseConnector1.getDBEntry(3, faqDBQuery);
            }
            else if (message.Equals("FlexTime"))//Abfrage nach Gleitzeit
            {
                return databaseConnector1.getDBEntry(4, faqDBQuery);
            }
            else if (message.Equals("HolidayDistribution"))//Abfrage nach Verteilung der Urlaubstage
            {
                return databaseConnector1.getDBEntry(5, faqDBQuery);
            }
            else if (message.Equals("Location"))//Abfrage nach Standort
            {
                return databaseConnector1.getDBEntry(6, faqDBQuery);
            }
            else if (message.Equals("Eliza")) //Eliza Easter Egg
            {
                return databaseConnector1.getDBEntry(15, faqDBQuery);
            }
            else if (message.Equals("Greetings"))//Begrüßung abfangen
            {
                return "Hallo User";
            }
            else if (message.Equals("HomeOffice"))//Home Office Abfrage
            {
                return databaseConnector1.getDBEntry(7, faqDBQuery);
            }
            else if (message.Equals("PublicTransport"))//ÖPNV
            {
                return databaseConnector1.getDBEntry(8, faqDBQuery);
            }
            else if (message.Equals("Parking")) //Parkplatz/auto
            {
                return databaseConnector1.getDBEntry(9, faqDBQuery);
            }
            else if (message.Equals("Benefits")) //Benefits
            {
                return databaseConnector1.getDBEntry(10, faqDBQuery);
            }
            else if (message.Equals("Client")) //Abfrage nach Projekten/Kunden
            {
                return databaseConnector1.getDBEntry(11, faqDBQuery);
            }
            else if (message.Equals("Ethics")) //Leitkonzept
            {
                return databaseConnector1.getDBEntry(12, faqDBQuery);
            }
            else if (message.Equals("StaffTraining")) //Fortbildungsmöglichkeiten
            {
                return databaseConnector1.getDBEntry(13, faqDBQuery);
            }
            else if (message.Equals("Promotion")) //Aufstiegschancen
            {
                return databaseConnector1.getDBEntry(14, faqDBQuery);
            }
            else if (message.Equals("Requirements"))//Anforderungen für die Stelle
            {
                if (stellenID == -1) //Es wird geprüft, ob der Bewerber bereits eine Stelle ausgewählt hat. Falls dies nicht der Fall ist, so wird eine Fehlermeldung zurückgegeben
                {
                    return "Es wurde noch keine Stelle ausgewählt.";
                }
                return databaseConnector1.getDBEntry(1, stellenDBQuery); //Wenn der Bewerber eine Stelle ausgewählt hat, so werden die Anforderungen an die Stelle aus der Datenbank ausgelesen und ausgegeben
            }
            else
            {
                return ErrorMessage; //Die zuvor festgelegte ErrorMessage wird zurückgegeben
            }
        }
        //Die postAnswer Methode gibt den in der answerFAQ Methode festgelegten String zurück
        public async void postAnswer(IDialogContext context, String message, int stellenID)
        {
            String answer = answerFAQ(message, stellenID);
            if (String.IsNullOrEmpty(answer))
            { }
            else
            {
                await context.PostAsync(answer);
            }
        }
    }
}