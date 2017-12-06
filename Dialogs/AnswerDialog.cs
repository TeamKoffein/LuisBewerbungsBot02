using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Bot.Luis
{
    public class AnswerDialog
    {
        int jobID;
        int applicantID;
        /*
        Die Methode bekommt einen String übergeben in Form von z.B "{ "card_Name": "Max Mustermann"}, welcher auseinander
        genommen wird. Darauf hin wird nach dem ersten String Teil (hier: "card_Name") gesucht und dessen Position in der uebergebenden Liste
        im Interger positionID abgespeichert. Dieses positionID wird daraufhin auch returnt. Desweiterem wird die Aussage (hier: "Max Mustermann")
        in die Datenbank mit der zugehörigen QuestionsIDReferenz und der bewerberID geschrieben.
        Bei der QuestionIDReference "Name" wird die applicantID von der DB erstellt und abgespeichert. Die jobID wird bei der QuestionIDReference "Stelle" von der Methode GetMessage gespeichert. DIese wiederum sind dann über ihre Getter abrufbar.
        */
        public async Task<int> GetMessage(String message, Dictionary<string, string> internalCheck, List<string> QuestionIDReference, int applicantIDdelivery)
        {
            int positionID = -1;
            applicantID = applicantIDdelivery;
            message = message.Replace("\"", string.Empty);
            message = message.Trim(new Char[] { '{', '}', ' ' });
            message = Regex.Replace(message, @"[^\u0000-\u007F]+", string.Empty);
            message = message.Replace(":", ",");
            String[] message_array = message.Split(',');
            for (int counter = 0; counter < message_array.Length; counter++)
                message_array[counter] = message_array[counter].Trim();
            if (internalCheck.ContainsKey(message_array[0]))
            {
                positionID = QuestionIDReference.IndexOf(internalCheck[message_array[0]]);
                if (positionID == 1)
                {
                    jobID = Int32.Parse(message_array[1]);
                }
                if (applicantID != -1)
                {
                    if (message_array.Length > 2)
                    {
                        string stitches = "";
                        for (int internCounter = 1; internCounter < message_array.Length; internCounter += 2)
                        {
                            stitches = stitches + " " + message_array[internCounter];
                        }
                        stitches = stitches.Trim();
                        updateDatabaseEntry(applicantID, QuestionIDReference[positionID], stitches);
                    }
                    else
                    {
                        updateDatabaseEntry(applicantID, QuestionIDReference[positionID], message_array[1]);
                    }
                }
                else
                {
                    applicantID = insertDatabaseEntry(QuestionIDReference[positionID], message_array[1]);
                }
                return positionID;
            }
            return positionID;


        }
        //Returnt die abgespeicherte BewerberID.
        public async Task<int> GetApplicantID()
        {
            return applicantID;
        }
        //Returnt die abgespeicherte jobID
        public async Task<int> GetJobID()
        {
            return jobID;
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
                        command.CommandText = "INSERT INTO BewerberTable (Name) Values (@name)";
                        command.Parameters.Add("@name", SqlDbType.NVarChar).Value = databaseEntry;
                        conn.Open();
                        command.ExecuteNonQuery();
                        conn.Close();
                    }
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "SELECT MAX(BewerberID) FROM BewerberTable";
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
        public void typeDatabase(string entry, int entryID, string databaseEntry)
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
                    command.CommandText = "UPDATE bewerberTable SET " + entry + " = @entry WHERE BewerberID = @entryPoint";
                    command.Parameters.Add("@entry", SqlDbType.NVarChar).Value = databaseEntry;
                    command.Parameters.Add("@entryPoint", SqlDbType.NVarChar).Value = entryID;
                    conn.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        //Datenbankanbindung
        //updateDatabaseEntry dient dazu, den neuen Eintrag des Bewerbers in der richtigen Spalte von @BewerberTable zu hinterlegen.
        //Es werden die BewerberID, der @card_Name, als Spaltenerkkenung, und der zu ändernde Eintrag benötigt.
        public void updateDatabaseEntry(int entryID, string entryColumn, string databaseEntry)
        {
            if (entryColumn == "Stellen")
            {
                typeDatabase("BeworbeneStelle", entryID, databaseEntry);
            }
            else if (entryColumn == "Geburtsdatum")
            {
                typeDatabase("Geburtsdatum", entryID, databaseEntry);
            }
            else if (entryColumn == "Qualifikationen")
            {
                typeDatabase("Qualifikationen", entryID, databaseEntry);
            }
            else if (entryColumn == "Private Projekte")
            {
                typeDatabase("PrivProjekte", entryID, databaseEntry);
            }
            else if (entryColumn == "Soziales Engagement")
            {
                typeDatabase("SozEngagement", entryID, databaseEntry);
            }
            else if (entryColumn == "Berufliche Laufbahn")
            {
                typeDatabase("Laufbahn", entryID, databaseEntry);
            }
            else if (entryColumn == "Sprachen")
            {
                typeDatabase("Sprachen", entryID, databaseEntry);
            }
            else if (entryColumn == "Kontaktdaten")
            {
                typeDatabase("Kontaktdaten", entryID, databaseEntry);
            }
        }
    }
}