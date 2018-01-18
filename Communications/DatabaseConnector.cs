using ProactiveBot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic;

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

        public int insertNewApp(string conversationID)
        {
            int appID = 0;
            using (DataConnection context = new DataConnection())
            {
                BewerberdatenLui applicant = new BewerberdatenLui { ConversationID = conversationID };
                context.BewerberdatenLuis.Add(applicant);
                context.SaveChanges();
                appID = applicant.BewerberID;
            };

            return appID;
        }
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

        public String getBingAdress(int ID)
        {
            String DBEntry = "";
            int i = 0;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "chatbotlcd2017db.database.windows.net";
            builder.UserID = "TeamKoffein";
            builder.Password = "LCD2017!";
            builder.InitialCatalog = "ChatBotLCD";

            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {

                    //sqlCommand(Object conn, Object command, String commandText, ID, "@ID", SqlDbType.Int)
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT Adress FROM BewerberdatenLuis WHERE BewerberID =@ID";
                    command.Parameters.Add("@ID", SqlDbType.Int).Value = ID;

                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0) == false)
                        {
                            DBEntry = DBEntry + reader.GetString(0);
                        }
                        else
                        {
                            DBEntry = DBEntry + "";
                        }
                        i++;
                    }
                    reader.Close();
                    conn.Close();
                }
                using (SqlCommand command = new SqlCommand())
                {

                    //sqlCommand(Object conn, Object command, String commandText, ID, "@ID", SqlDbType.Int)
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT PostalCode FROM BewerberdatenLuis WHERE BewerberID =@ID";
                    command.Parameters.Add("@ID", SqlDbType.Int).Value = ID;

                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0) == false)
                        {
                            DBEntry = DBEntry + reader.GetString(0);
                        }
                        else
                        {
                            DBEntry = DBEntry + "";
                        }
                        i++;
                    }
                    reader.Close();
                }
            }
            return DBEntry;
        }
        //Datenbankanbindung
        //Die Methode insertDatabaseEntry legt bei neuen Bewerbern einen neuen Datenbankeintrag an.
        //Es werden der @card_Name und der Name als Eintrag benötigt und die automatisch generierte BewerberID wird zurückgegeben.
        public int insertDatabaseEntry(string entryPlace, string databaseEntry)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "chatbotlcd2017db.database.windows.net";
            builder.UserID = "TeamKoffein";
            builder.Password = "LCD2017!";
            builder.InitialCatalog = "ChatBotLCD";

            int currentEntry = 1;
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                if (entryPlace == "Name")
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "INSERT INTO BewerberdatenLuis (Name) Values (@name)";
                        command.Parameters.Add("@name", SqlDbType.NVarChar).Value = databaseEntry;
                        conn.Open();
                        command.ExecuteNonQuery();
                        conn.Close();
                    }
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "SELECT MAX(BewerberID) FROM BewerberDatenLuis";
                        conn.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        reader.Read();
                        currentEntry = reader.GetInt32(0);
                        reader.Close();
                    }
                }
            }
            return currentEntry;
        }

        //Datenbankanbindung
        //Speichert die vom Bewerber übergegebenen Daten in der Datenbank. Es wird die BewerberID, die zu aktualisierende Spalte und der entsprechende
        // Eintrag benötigt.
        public void updateDatabase(string column, int entryID, string databaseEntry)
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
                    command.CommandText = "UPDATE BewerberdatenLuis SET " + column + " = @entry WHERE BewerberID = @entryPoint";
                    command.Parameters.Add("@entry", SqlDbType.NVarChar).Value = databaseEntry;
                    command.Parameters.Add("@entryPoint", SqlDbType.NVarChar).Value = entryID;
                    conn.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void updateNewsletter(int appID)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "chatbotlcd2017db.database.windows.net";
            builder.UserID = "TeamKoffein";
            builder.Password = "LCD2017!";
            builder.InitialCatalog = "ChatBotLCD";
            string entry = "true";
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "UPDATE GespeicherteBewerber SET Newsletter = @entry WHERE BewerberID = @appID";
                    command.Parameters.Add("@entry", SqlDbType.NVarChar).Value = entry;
                    command.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
                    conn.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        //Die Methode getData sammelt alle Einträge über den Bewerber mit der übergebenen @bewerberID
        //Die gesammelten Daten werden als Array an den DataAssembler weitergegeben.
        public string[] getData(int bewerberID)
        {
            string[] active = new string[17];
            //buildDB();
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "chatbotlcd2017db.database.windows.net";
            builder.UserID = "TeamKoffein";
            builder.Password = "LCD2017!";
            builder.InitialCatalog = "ChatBotLCD";

            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                //Erfassung der Spaltennamen in @active
                using (SqlCommand command = new SqlCommand())
                {

                    //sqlCommand(Object conn, Object command, String commandText)
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT BewerberID, Job, Name, Adress, PostalCode, Place, PhoneNumber, Email, Birthday, Career, EducationalBackground, ProgrammingLanguage, SocialEngagement, Language, PrivateProjects, StartDate FROM BewerberdatenLuis";

                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);
                    DataTable schemaTable = reader.GetSchemaTable();
                    int i = 0;
                    foreach (DataRow colRow in schemaTable.Rows)
                    {
                        active[i] = colRow.Field<String>("ColumnName") + ": ";
                        i++;
                    }
                    reader.Close();
                    conn.Close();
                    active[1] = "";
                }

                //Erfassung der angegebenen Daten und Abspeicherung in @active
                using (SqlCommand command = new SqlCommand())
                {

                    //sqlCommand(Object conn, Object command, String commandText, int id, String parametersAdd1, Object parametersAdd2)
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT BewerberID, Job, Name, Adress, PostalCode, Place, PhoneNumber, Email, Birthday, Career, EducationalBackground, ProgrammingLanguage, SocialEngagement, Language, PrivateProjects, StartDate FROM BewerberdatenLuis WHERE BewerberID =@aktuellerBewerber";
                    command.Parameters.Add("@aktuellerBewerber", SqlDbType.Int).Value = bewerberID;

                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        for (int i = 2; i < active.Length - 1; i++)
                        {
                            if (reader.IsDBNull(i) == false)
                            {
                                active[i] = active[i] + reader.GetString(i);
                            }
                            else
                            {
                                active[i] = active[i] + "";
                            }
                        }
                    }
                    reader.Close();
                    conn.Close();
                }

                using (SqlCommand command = new SqlCommand())
                {

                    //sqlCommand(Object conn, Object command, String commandText, int id, String parametersAdd1, Object parametersAdd2)
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT Job FROM BewerberdatenLuis WHERE BewerberID =@aktuellerBewerber";
                    command.Parameters.Add("@aktuellerBewerber", SqlDbType.Int).Value = bewerberID;

                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0) == false)
                        {
                            active[1] = reader.GetInt32(0).ToString();
                        }
                        else
                        {
                            active[1] = "";
                        }
                    }
                    reader.Close();
                    conn.Close();
                }

                //Anzahl der Bewerber auf die gleiche Stelle
                using (SqlCommand command = new SqlCommand())
                {

                    //sqlCommand(Object conn, Object command, String commandText, int id, String parametersAdd1, Object parametersAdd2)
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT COUNT (Job) FROM BewerberdatenLuis WHERE Job = @paramStellenID";
                    command.Parameters.Add("@paramStellenID", SqlDbType.VarChar).Value = active[1];

                    conn.Open();
                    if (active[1] == "")
                    {
                        active[0] = "";
                    }
                    else
                    {
                        Int32 count = (Int32)command.ExecuteScalar();
                        active[0] = "Anzahl der Bewerber auf diese Stelle: " + count.ToString();
                    }
                    conn.Close();
                }

                //Erkennung der beworbenen Stelle, Bezeichnungsauslesung aus @Stellen.dbo
                using (SqlCommand command = new SqlCommand())
                {
                    int jobID;
                    if (active[1] == "")
                    {
                        jobID = 0;
                    }
                    else
                    {
                        jobID = Int32.Parse(active[1]);
                    }
                    //sqlCommand(conn, command, "SELECT Stellenname FROM Stellen WHERE StellenID = @paramStellenID", jobID, "@paramStellenID", SqlDbType.Int);
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT Stellenname FROM Stellen WHERE StellenID = @paramStellenID";
                    command.Parameters.Add("@paramStellenID", SqlDbType.Int).Value = jobID;

                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0) == false)
                        {
                            active[1] = "Beworbene Stelle: " + reader.GetString(0);
                        }
                        else
                        {
                            active[1] = "Beworbene Stelle: keine Angabe";
                        }
                    }
                    reader.Close();
                    conn.Close();
                }
            }
            return active;
        }

        //Rückgabe aller hinterlegten, offenen Stellen aus der DB
        public String[] getStellenDBEntry()
        {
            int count;
            String[] DBEntry;
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
                    command.CommandText = "SELECT COUNT (StellenID) FROM Stellen";
                    conn.Open();
                    count = (Int32)command.ExecuteScalar();
                    conn.Close();
                }
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT Stellenname FROM Stellen";
                    conn.Open();

                    SqlDataReader reader = command.ExecuteReader();
                    int i = 0;
                    DBEntry = new String[count];
                    while (reader.Read())
                    {
                        DBEntry[i] = reader.GetString(0);
                        i++;
                    }
                    reader.Close();
                    return DBEntry;
                }
            }
        }


        // Diese Methode übergibt alle hinterlegten FAQ-Fragen unter Angabe der @anrede
        //Diese Fragen werden gestellt um Daten über den Bewerber zu sammeln
        public String[] getFAQQuestions(int anrede)
        {
            int count;
            String[] DBEntry;
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
                    command.CommandText = "SELECT COUNT (FAQFragenID) FROM FAQFragen";
                    conn.Open();
                    count = (Int32)command.ExecuteScalar();
                    conn.Close();
                }
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT * FROM FAQFragen";
                    conn.Open();

                    SqlDataReader reader = command.ExecuteReader();
                    int i = 1;
                    DBEntry = new String[count + 1];
                    while (reader.Read())
                    {
                        DBEntry[i] = reader.GetString(anrede);
                        i++;
                    }
                    reader.Close();
                    return DBEntry;
                }
            }
        }

        //Liest die Fachfrage zu der angegebenen @StellenID aus
        public String getTechQuestion(int jobID)
        {

            String DBEntry = "";
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
                    command.CommandText = "SELECT Frage FROM Fachfragen WHERE FragenID = @jobID";
                    command.Parameters.Add("@jobID", SqlDbType.Int).Value = jobID;
                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        DBEntry = reader.GetString(0);
                    }
                    reader.Close();
                    conn.Close();
                }
                return DBEntry;
            }
        }

        public String getJobDate(int jobID)
        {

            String DBEntry = "";
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
                    command.CommandText = "SELECT Einstellungsdatum FROM Stellen WHERE StellenID = @jobID";
                    command.Parameters.Add("@jobID", SqlDbType.Int).Value = jobID;
                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        DBEntry = reader.GetString(0);
                    }
                    reader.Close();
                    conn.Close();
                }
                return DBEntry;
            }
        }

        public Int32 getCountName(string checkName)
        {
            int count;
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
                    command.CommandText = "SELECT COUNT (Name) FROM BewerberdatenLuis WHERE Name = @toCheck";
                    command.Parameters.Add("@toCheck", SqlDbType.NVarChar).Value = checkName;
                    conn.Open();
                    count = (Int32)command.ExecuteScalar();
                    conn.Close();
                }
            }
            return count;
        }

        public String getAdress(string checkName)
        {
            string adress = "";
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
                    command.CommandText = "SELECT Adress FROM BewerberdatenLuis WHERE Name = @checkName";
                    command.Parameters.Add("@checkName", SqlDbType.NVarChar).Value = checkName;
                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0) == false)
                        {
                            adress = reader.GetString(0);
                        }
                        else
                        {
                            adress = "keine Adresse hinterlegt";
                        }
                    }
                    reader.Close();
                    conn.Close();
                }
                return adress;
            }
        }

        public Int32 getApplicantID(string checkName, string checkAdress)
        {
            int applicantID = 0;
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
                    command.CommandText = "SELECT BewerberID FROM BewerberdatenLuis WHERE Name = @checkName AND Adress = @checkAdress";
                    command.Parameters.Add("@checkName", SqlDbType.NVarChar).Value = checkName;
                    command.Parameters.Add("@checkAdress", SqlDbType.NVarChar).Value = checkAdress;
                    conn.Open();
                    applicantID = (Int32)command.ExecuteScalar();
                    conn.Close();
                }
                return applicantID;
            }
        }

        public Int32 getApplicantIDMail(string checkName, string checkMail)
        {
            int applicantID = 0;
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
                    command.CommandText = "SELECT BewerberID FROM BewerberdatenLuis WHERE Name = @checkName AND Email = @checkMail";
                    command.Parameters.Add("@checkName", SqlDbType.NVarChar).Value = checkName;
                    command.Parameters.Add("@checkMail", SqlDbType.NVarChar).Value = checkMail;
                    conn.Open();
                    applicantID = (Int32)command.ExecuteScalar();
                    conn.Close();
                }
                return applicantID;
            }
        }
        public List<bool> getMissing(int applicantID)
        {
            List<bool> Question = new List<bool>();
            bool field;
            Question.Add(true);
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
                    command.CommandText = "SELECT Job, Name, Adress, PostalCode, Place, PhoneNumber, Email, Birthday, Career, EducationalBackground, ProgrammingLanguage, SocialEngagement, Language, PrivateProjects, StartDate FROM BewerberdatenLuis WHERE BewerberID = @appID";
                    //command.CommandText = "SELECT * FROM BewerberdatenLuis WHERE BewerberID = @appID";
                    command.Parameters.Add("@appID", SqlDbType.Int).Value = applicantID;
                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        for (int i = 0; i <= 14; i++)
                        {
                            field = reader.IsDBNull(i);
                            if (field) { Question.Add(false); }
                            else { Question.Add(true); }
                        }
                    }
                    reader.Close();
                    conn.Close();
                }
            }
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

