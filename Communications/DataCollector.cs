using System;
using System.Data;
using System.Data.SqlClient;

namespace Application.Bot.Luis
{
    public class DataCollector
    {
        public string[] getData(int bewerberID)
        {
            string[] active = new string[11];
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
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("@paramStellenID", SqlDbType.VarChar).Value = active[1];
                    command.CommandText = "SELECT COUNT (BeworbeneStelle) FROM BewerberTable WHERE BeworbeneStelle = @paramStellenID";
                    conn.Open();
                    Int32 count = (Int32)command.ExecuteScalar();   
                    active[0] = "Anzahl der Bewerber auf diese Stelle: " + count.ToString();
                    conn.Close();
                }

                //Erkennung der beworbenen Stelle, Bezeichnungsauslesung aus @Stellen.dbo
                using (SqlCommand command = new SqlCommand())
                {
                    int jobID = Int32.Parse(active[1]);
                    command.Connection = conn;
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("@paramStellenID", SqlDbType.Int).Value = jobID;
                    command.CommandText = "SELECT Stellenname FROM Stellen WHERE StellenID = @paramStellenID";
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
    }
}