using System;
using System.Net.Mail;

namespace Bewerbungs.Bot.Luis
{
    public class DataAssembler
    {
        //Die Send Data Methode versendet die vom Chatbot gesammelten Daten an eine gegebene Mail-Adresse.
        //Es wird die ID des Bewerbers übergeben, um die passenden Daten aus der Datenbank auszulesen
        public void sendData(int bewerberID)
        {
            //Es werden die grundlegenden Maildaten festgelegt
            //DataCollector Collector = new DataCollector();
            DatabaseConnector connector = new DatabaseConnector();
            String mailAddressSender = "bewerbungsbot@web.de";
            String mailAddressLise = "teamkoffein@outlook.de";
            //String[] Data = Collector.getData(bewerberID);
            String[] Data = connector.getData(bewerberID);
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(mailAddressSender); //Absender 
            mail.To.Add(mailAddressLise); //Empfänger 
            mail.Subject = "Es gibt einen neuen Bewerber über den Chatbot";
            mail.Body = convertInputToHTMLMailBody(Data);
            mail.IsBodyHtml = true;

            SmtpClient client = new SmtpClient("smtp.web.de", 587); //SMTP Server von Hotmail und Outlook. 
            try
            {
                client.Credentials = new System.Net.NetworkCredential(mailAddressSender, "LCD2017!");//Anmeldedaten für den SMTP Server 

                client.EnableSsl = true; //Die meisten Anbieter verlangen eine SSL-Verschlüsselung 

                client.Send(mail); //Senden 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Senden der E-Mail\n\n{0}", ex.Message);
            }
            Console.ReadKey();
        }

        //Die convertInputToHTMLMailBody Methode verändert die Datenbankeinträge zu einem HTML Code, sodass der Mailbody einem bestimmten Format folgt
        public String convertInputToHTMLMailBody(String[] Data)
        {
            int i = 0;
            String myEncodedString = "";
            while (i < Data.Length)
            {
                myEncodedString = myEncodedString + System.Web.HttpUtility.HtmlEncode(Data[i]);
                myEncodedString = myEncodedString + "<br>";
                i++;
            }
            return myEncodedString;
        }
    }
}