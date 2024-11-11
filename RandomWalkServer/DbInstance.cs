using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace RandomWalkServer
{
    public class DbInstance
    {
        private static string connString = "Data Source=randomWalkData.db;version=3;";

        public DbInstance()
        {
            InitialiseDb();
        }

        private void InitialiseDb()
        {
            using (SQLiteConnection c = new SQLiteConnection(connString))
            {
                c.Open();
                string createTableQuery = "CREATE TABLE IF NOT EXISTS RandomWalkData (Timestamp DATETIME, Value REAL)";
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, c))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void LogValue(double value)
        {
            using (SQLiteConnection c = new SQLiteConnection(connString))
            {
                c.Open();
                string insertQuery = "INSERT INTO RandomWalkData (Timestamp, Value) VALUES (datetime('now'), @value)";
                using (SQLiteCommand command = new SQLiteCommand(insertQuery, c))
                {
                    command.Parameters.AddWithValue("@value", value);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
