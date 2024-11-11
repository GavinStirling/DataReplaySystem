using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimePlotDataDisplay
{
    public class DbInstance
    {
        private static string connString = "Data Source=C:\\Users\\Gavin Stirling\\source\\personal\\DataReplaySystem\\RandomWalkServer\\bin\\Debug\\net8.0\\randomWalkData.db;version=3;";

        public List<(DateTime, double)> GetAllFromDb()
        {
            List<(DateTime, double)> results = new();

            try
            {
                using (SQLiteConnection c = new SQLiteConnection(connString))
                {
                    c.Open();
                    string query = "SELECT Timestamp, Value FROM RandomWalkData ORDER BY Timestamp";
                    using (SQLiteCommand command = new SQLiteCommand(query, c))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime timestamp = DateTime.Parse(reader["Timestamp"].ToString());
                                double value = Convert.ToDouble(reader["Value"]);
                                results.Add((timestamp, value));
                            }
                        }
                    }
                }
                return results;
            } catch (Exception ex)
            {
                Console.WriteLine("An error occurred while fetching all of the data from the DB: " + ex.Message);
                throw ex;
            }
        }
    }
}
