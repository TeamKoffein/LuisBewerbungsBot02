using System;
using System.Data;
using System.Data.SqlClient;

namespace Bewerbungs.Bot.Luis
{
    public class DatabaseConnector
    {
        public String getDBEntry(int ID, String commandText)
        {
            return getDBEntry(ID, commandText, 0);
        }

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


        public string[] getData(int bewerberID)
        {
            string[] active = new string[11];
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
                    command.CommandText = "SELECT * FROM BewerberTable";

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
                    command.CommandText = "SELECT * FROM BewerberTable WHERE BewerberID =@aktuellerBewerber";
                    command.Parameters.Add("@aktuellerBewerber", SqlDbType.Int).Value = bewerberID;

                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        for (int i = 1; i < active.Length - 1; i++)
                        {
                            active[i] = active[i] + reader.GetString(i);
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
                    command.CommandText = "SELECT COUNT (BeworbeneStelle) FROM BewerberTable WHERE BeworbeneStelle = @paramStellenID";
                    command.Parameters.Add("@paramStellenID", SqlDbType.VarChar).Value = active[1];

                    conn.Open();
                    Int32 count = (Int32)command.ExecuteScalar();
                    active[0] = "Anzahl der Bewerber auf diese Stelle: " + count.ToString();
                    conn.Close();
                }

                //Erkennung der beworbenen Stelle, Bezeichnungsauslesung aus @Stellen.dbo
                using (SqlCommand command = new SqlCommand())
                {
                    int jobID = Int32.Parse(active[1]);

                    //sqlCommand(conn, command, "SELECT Stellenname FROM Stellen WHERE StellenID = @paramStellenID", jobID, "@paramStellenID", SqlDbType.Int);
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT Stellenname FROM Stellen WHERE StellenID = @paramStellenID";
                    command.Parameters.Add("@paramStellenID", SqlDbType.Int).Value = jobID;

                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        active[1] = "Beworbene Stelle: " + reader.GetString(0);
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
    }
}