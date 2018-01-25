using ProactiveBot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Configuration;

namespace Bewerbungs.Bot.Luis
{
    //Diese Klasse dient als Sammelstelle für alle Datenbankanbindungen
    public class DatabaseConnector
    {
        //Methode um die richtige Anrede aus der entsprechenden Spalte der FAQ zu filtern
        public String getDBEntry(int ID, String commandText)
        {
            return getDBEntry(ID, commandText, 0);
        }

        //Abruf der Fachfragen aus DB
        public String[,] getQuizDBEntry()
        {
            String[,] quiz = new String[12,6];
            using (DataConnection context = new DataConnection())
            {
                int i = 0;
                Fachfragen question = new Fachfragen { };
                var list = context.Fachfragens.ToList();
                foreach (var bl in list)
                {
                    quiz[i,0] = bl.Frage;
                    quiz[i,1] = bl.AntwortEins;
                    quiz[i,2] = bl.AntwortZwei;
                    quiz[i,3] = bl.AntwortDrei;
                    quiz[i,4] = bl.RichtigeAntwort;
                    quiz[i,5] = bl.Punkte;
                    i++;
                }
            }
            return quiz;
        }

        //Setzen des Scores für den Bewerber
        public void setScore(int appID, int score)
        {
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.FirstOrDefault(r => r.BewerberID == appID);
                applicant.Score = score;
                context.SaveChanges();
            };
        }

        //Setzen des Levels des Bewerbers um ihn bei Inaktivität anzuschreiben
        public void setLevel(int appID, string level)
        {
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.FirstOrDefault(r => r.BewerberID == appID);
                applicant.Level = level;
                context.SaveChanges();
            };
        }

        //Abrufen des Levels des Bewerbers um ihn bei Inaktivität anzuschreiben
        public string getLevel(int appID)
        {
            string level = "";
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.FirstOrDefault(r => r.BewerberID == appID);
                level = applicant.Level;
            };
            return level;
        }

        //Setzen von Active des Bewerbers um ihn bei Inaktivität anzuschreiben
        public void setActive(int appID, int active)
        {
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.FirstOrDefault(r => r.BewerberID == appID);
                applicant.Active = active;
                context.SaveChanges();
            };
        }

        //Abrufen von Active des Bewerbers um ihn bei Inaktivität anzuschreiben
        public int getActive(int appID)
        {
            int active = 0;
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.FirstOrDefault(r => r.BewerberID == appID);
                active = applicant.Active ?? 0;
            };
            return active;
        }

        //Setzen der letzten zeitlichen Eingabe des Bewerbers um ihn bei Inaktivität anzuschreiben
        public void setTime(int appID)
        {
            DateTime time = new DateTime();
            time = DateTime.Now;
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.FirstOrDefault(r => r.BewerberID == appID);
                applicant.Time = time;
                context.SaveChanges();
            };
        }

        //Abruf der letzten zeitlichen Eingabe des Bewerbers um ihn bei Inaktivität anzuschreiben
        public DateTime getTime(int appID)
        {
            DateTime time = new DateTime();
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.FirstOrDefault(r => r.BewerberID == appID);
                DateTime? dateOrNull = applicant.Time;
                if(dateOrNull != null)
                {
                    time = dateOrNull.Value;
                }
            };
            return time;
        }

        //Anlegen eines neuen Bewerbers unter Angabe aller Standardwerte und IDs
        public int insertNewApp(string conversationID, string userID, string channel)
        {
            DateTime time = new DateTime();
            DateTime deftime = new DateTime(3018, 1, 1);
            time = DateTime.Today;
            int appID = 0;
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { ConversationID = conversationID };
                context.BewerberdatenLuis.Add(applicant);
                applicant.UserID = userID;
                applicant.ChannelId = channel;
                applicant.Score = 0;
                applicant.Newsletter = "false";
                applicant.JobInterview = "false";
                applicant.ApplicationReviewed = 0;
                applicant.Active = 0;
                applicant.TimeStamp = time;
                applicant.InterviewDate = deftime;
                if (channel.Equals("emulator") || channel.Equals("webchat")) {
                    applicant.Level = "4";
                }
                else
                {
                    applicant.Level = "0";
                }
                context.SaveChanges();
                appID = applicant.BewerberID;
            };
            return appID;
        }

        //Überprüfung ob Feld beschrieben ist
        public bool checkNull(string column, int appID)
        {
            bool isNull = true;
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.FirstOrDefault(r => r.BewerberID == appID);
                int i = applicant.BewerberID;
                var result = context.BewerberdatenLuis.Where(r => r.BewerberID == appID).Select(column);
                string test = result == null ? null : result.ToString();
                if (!String.IsNullOrEmpty(test))
                {
                    isNull = false;
                }
            };
            return isNull;
        }


        //Abfrage ob Bewerbung vom Recruiter schon angesehen wurde
        public bool checkReview(int appID)
        {
            bool review = false;
            using (DataConnection context = new DataConnection()) {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.FirstOrDefault(r => r.BewerberID == appID);
                int rev = applicant.ApplicationReviewed ?? 0;
                if (rev != 0)
                    review = true;      
            };
            return review;
        }

        //Diese Methode übergibt einen einzelnen Eintrag aus der DB
        //benötigt werden die @ID der gesuchten Zeile, der @key als Spaltenangabe (0) für den reader und der @commandText als SQL-Befehl 
        public String getDBEntry(int ID, String commandText, int key)
        {
            String DBEntry = "";


            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "chatbotlcd2017db.database.windows.net";
            builder.UserID = "TeamKoffein";
            builder.Password = "LCD2017!";
            builder.InitialCatalog = "ChatBotLCD";


            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            //using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["DataConnection"].ToString()))
            {
                using (SqlCommand command = new SqlCommand())
                {

                    //sqlCommand(Object conn, Object command, String commandText, ID, "@ID", SqlDbType.Int)
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandText = commandText;
                    command.Parameters.Add("@ID", SqlDbType.Int).Value = ID;

                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Read();
                    DBEntry = reader.GetString(key);
                    reader.Close();
                }
            }
            return DBEntry;
        }


        //Rückgabe der hinterlegten Adresse
        public string getAdress(int appID)
        {
            string dbEntry = "";
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.FirstOrDefault(r => r.BewerberID == appID);
                dbEntry = applicant.Adress ?? "";
            };
            return dbEntry;
        }

        //Rüchgabe der hinterlegten PLZ
        public string getPostalCode(int appID)
        {
            string dbEntry = "";
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.FirstOrDefault(r => r.BewerberID == appID);
                dbEntry = applicant.PostalCode ?? "";
            };
            return dbEntry;
        }


        //Rückgabe der hinterlegten Stadt
        public string getPlace(int appID)
        {
            string dbEntry = "";
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.FirstOrDefault(r => r.BewerberID == appID);
                dbEntry = applicant.Place ?? "";
            };
            return dbEntry;
        }

        //Update-Funktion für die Angaben des Bewerbers
        public void updateDB(string column, int appID, string dbEntry)
        {
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.FirstOrDefault(r => r.BewerberID == appID);
                switch (column)
                {
                    case "Name":
                        applicant.Name = dbEntry;
                        break;
                    case "Adress":
                        applicant.Adress = dbEntry;
                        break;
                    case "PostalCode":
                        applicant.PostalCode = dbEntry;
                        break;
                    case "Place":
                        applicant.Place = dbEntry;
                        break;
                    case "PhoneNumber":
                        applicant.PhoneNumber = dbEntry;
                        break;
                    case "Email":
                        applicant.Email = dbEntry;
                        break;
                    case "Birthday":
                        applicant.Birthday = dbEntry;
                        break;
                    case "Career":
                        applicant.Career = dbEntry;
                        break;
                    case "EducationalBackground":
                        applicant.EducationalBackground = dbEntry;
                        break;
                    case "ProgrammingLanguage":
                        applicant.ProgrammingLanguage = dbEntry;
                        break;
                    case "SocialEngagement":
                        applicant.SocialEngagement = dbEntry;
                        break;
                    case "Language":
                        applicant.Language = dbEntry;
                        break;
                    case "PrivateProjects":
                        applicant.PrivateProjects = dbEntry;
                        break;
                    case "StartDate":
                        applicant.StartDate = dbEntry;
                        break;
                    case "Job":
                        applicant.Job = Convert.ToInt32(dbEntry);
                        break;
                    case "ChannelID":
                        applicant.ChannelId = dbEntry;
                        break;
                    case "ConversationID":
                        applicant.ConversationID = dbEntry;
                        break;
                }
                context.SaveChanges();
            };
        }

        //Update für die Akzeptanz des Newsletters
        public void updateNewsletter(int appID)
        {
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.FirstOrDefault(r => r.BewerberID == appID);
                applicant.Newsletter = "true";
            };
        }
        

        //Zusammenfassung aller hinterlegten Daten des Bewerbers
        public string[] getData (int appID)
        {
            string[] data = new string[] {"", "Anzahl Bewerber auf diese Stelle: ", "Job: ", "Name: ", "Adress: ", "PostalCode: ", "Place: ", "PhoneNumber: ", "Email: ",
                "Birthday: ", "Career: ", "EducationalBackground: ", "ProgrammingLanguage: ", "SocialEngagement: ", "Language: ", "PrivateProjects: ", "StartDate: ", "Score: " };
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.FirstOrDefault(r => r.BewerberID == appID);
                int jobID = applicant.Job ?? 0;
                int count = context.BewerberdatenLuis.Where(c => c.Job == applicant.Job).Count();
                data[1] = data[1] + count.ToString();
                data[2] = data[2] + applicant.Job ?? "";
                data[3] = data[3] + applicant.Name ?? "";
                data[4] = data[4] + applicant.Adress ?? "";
                data[5] = data[5] + applicant.PostalCode ?? "";
                data[6] = data[6] + applicant.Place ?? "";
                data[7] = data[7] + applicant.PhoneNumber ?? "";
                data[8] = data[8] + applicant.Email ?? "";
                data[9] = data[9] + applicant.Birthday ?? "";
                data[10] = data[10] + applicant.Career ?? "";
                data[11] = data[11] + applicant.EducationalBackground ?? "";
                data[12] = data[12] + applicant.ProgrammingLanguage ?? "";
                data[13] = data[13] + applicant.SocialEngagement ?? "";
                data[14] = data[14] + applicant.Language ?? "";
                data[15] = data[15] + applicant.PrivateProjects ?? "";
                data[16] = data[16] + applicant.StartDate ?? "";
                data[17] = data[17] + applicant.Score ?? "";

                using (DataConnection ctx = new DataConnection()) {
                    Stellen job = new Stellen { };
                    job = ctx.Stellens.FirstOrDefault(j => j.StellenID == jobID);
                    data[0] = "Beworbene Stelle: " + job.Stellenname ?? "";
                };
            };
            return data;
        }

        //Rückgabe aller hinterlegten, offenen Stellen
        public String[] getStellenDBEntry()
        {
            String[] jobs;
            int count;
            using (DataConnection context = new DataConnection())
            {
                Stellen stellen = new Stellen { };
                count = context.Stellens.Count();
                jobs = new String[count];
                var list = context.Stellens.ToList();
                int i = 0;
                foreach (var bl in list)
                {
                    jobs[i] = bl.Stellenname;
                    i++;
                }
            };
            return jobs;
        }

        // Diese Methode übergibt alle hinterlegten FAQ-Fragen unter Angabe der @anrede
        //Diese Fragen werden gestellt um Daten über den Bewerber zu sammeln
        public String[] getFAQQuestions(int salutaion)
        {
            String[] questions;
            using (DataConnection context = new DataConnection())
            {
                FAQFragen faq = new FAQFragen { };
                int count = context.FAQFragens.Count();
                questions = new String[count + 1];
                var list = context.FAQFragens.ToList();
                int i = 1;
                if (salutaion == 1) {
                    foreach (var bl in list)
                    {
                        questions[i] = bl.FAQFrageDU;
                        i++;
                    }
                }
                else
                {
                    foreach (var bl in list)
                    {
                        questions[i] = bl.FAQFrageSie;
                        i++;
                    }
                }
            };
            return questions;
        }


  
        //Rückgabe des Anfangsdatums für den Job
        public string getJobDate(int jobID)
        {
            string date = "";
            using (DataConnection context = new DataConnection())
            {
                Stellen stellen = new Stellen { };
                stellen = context.Stellens.FirstOrDefault(s => s.StellenID == jobID);
                date = stellen.Einstellungsdatum;
            };
            return date;
        }

        //Überprüfung ob der Angegebene Náme schon existiert
        public Int32 getCountName(string appName)
        {
            int count;
            using (DataConnection context = new DataConnection())
            {
                count = context.BewerberdatenLuis.Where(n => n.Name == appName).Count();
            }
            return count;
        }


        //Rückgabe der BewerberID bei einer Wiederanmeldung eines bekannten Bewerbers
        public int getApplicantIDMail (string name, string email)
        {
            int appID;
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                applicant = context.BewerberdatenLuis.Where(n => n.Name == name).FirstOrDefault(e =>e.Email == email);
                appID = applicant.BewerberID;
            };
            return appID;
        }


        //Rückgabe einer Liste für schon beantwortete Fragen
        public List<bool> getMissing(int appID)
        {
            List<bool> Question = new List<bool>();
            bool field;
            Question.Add(true);
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { };
                var list = context.BewerberdatenLuis.Where(i => i.BewerberID == appID).Select(i => new { i.Job, i.Name, i.Adress, i.PostalCode, i.Place,
                    i.PhoneNumber, i.Email, i.Birthday, i.Career, i.EducationalBackground, i.ProgrammingLanguage, i.SocialEngagement,
                    i.Language, i.PrivateProjects, i.StartDate }).ToList();
                foreach (var bl in list)
                {
                    field = bl.Job != null;
                    Question.Add(field);
                    field = bl.Name != null;
                    Question.Add(field);
                    field = bl.Adress != null;
                    Question.Add(field);
                    field = bl.PostalCode != null;
                    Question.Add(field);
                    field = bl.Place != null;
                    Question.Add(field);
                    field = bl.PhoneNumber != null;
                    Question.Add(field);
                    field = bl.Email != null;
                    Question.Add(field);
                    field = bl.Birthday != null;
                    Question.Add(field);
                    field = bl.Career != null;
                    Question.Add(field);
                    field = bl.EducationalBackground != null;
                    Question.Add(field);
                    field = bl.ProgrammingLanguage != null;
                    Question.Add(field);
                    field = bl.SocialEngagement != null;
                    Question.Add(field);
                    field = bl.Language != null;
                    Question.Add(field);
                    field = bl.PrivateProjects != null;
                    Question.Add(field);
                    field = bl.StartDate != null;
                    Question.Add(field);
                }
            };
            return Question;
        }

        public void transferData(int appID)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "chatbotlcd2017db.database.windows.net";
            builder.UserID = "TeamKoffein";
            builder.Password = "LCD2017!";
            builder.InitialCatalog = "ChatBotLCD";
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "IF @@ROWCOUNT = 0" +
                        " INSERT INTO GespeicherteBewerber (BewerberID, Job, Name, Adress, PostalCode, Place, PhoneNumber, Email, Birthday, Career, EducationalBackground, ProgrammingLanguage, SocialEngagement, Language, PrivateProjects)" +
                        " SELECT BewerberID, Job, Name, Adress, PostalCode, Place, PhoneNumber, Email, Birthday, Career, EducationalBackground, ProgrammingLanguage, SocialEngagement, Language, PrivateProjects" +
                        " FROM BewerberdatenLuis" +
                        " WHERE BewerberID = @appID" +
                        " ELSE" +
                        " UPDATE GespeicherteBewerber" +
                        " SET BewerberID = t2.BewerberID, Job = t2.Job, Name = t2.Name, Adress = t2.Adress, PostalCode = t2.PostalCode, Place = t2.Place, PhoneNumber = t2.PhoneNumber, Email = t2.Email, Birthday = t2.Birthday, Career = t2.Career, EducationalBackground = t2.EducationalBackground, ProgrammingLanguage = t2.ProgrammingLanguage, SocialEngagement = t2.SocialEngagement, Language = t2.Language, PrivateProjects = t2.PrivateProjects" +
                        " FROM BewerberdatenLuis t2" +
                        " WHERE t2.BewerberID = @appID";
                    command.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
                    conn.Open();
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }
    }
}

