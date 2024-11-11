using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Common;
using System.Data;

namespace RandomWalkReplayServer
{
    class StartAndEnd(DateTime s, DateTime e)
    {
        public DateTime StartTime = s;
        public DateTime EndTime = e;
    }
    public class ReplayServer
    {
        private Boolean _isRunning;
        private TcpListener _server;
        private Dictionary<NetworkStream, TcpClient> _subscribedClients = new Dictionary<NetworkStream, TcpClient>();
        private static string connString = "Data Source=C:\\Users\\Gavin Stirling\\source\\personal\\DataReplaySystem\\RandomWalkServer\\bin\\Debug\\net8.0\\randomWalkData.db;version=3;";

        public ReplayServer(int port)
        {
            try
            {
                Console.WriteLine("Enter the start time (YYYY-MM-DD HH:MM:SS): ");
                DateTime startTime = DateTime.Parse(Console.ReadLine());

                Console.WriteLine("Enter the end time (YYYY-MM-DD HH:MM:SS): ");
                DateTime endTime = DateTime.Parse(Console.ReadLine());

                StartAndEnd startAndEnd = new StartAndEnd(startTime, endTime);

                _server = new TcpListener(IPAddress.Any, port);
                _server.Start();
                Console.WriteLine("Server started, waiting for client to connect...");
                
                _isRunning = true;

                Thread dataThread = new Thread(new ParameterizedThreadStart(ReplayData));
                dataThread.Start(startAndEnd);

                LoopClients();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on server: " + ex.Message);
            }

        }



        public void ReplayData(Object obj)
        {
            StartAndEnd startAndEnd = (StartAndEnd)obj;
            List<(DateTime, double)> dataToReplay = new List<(DateTime, double)> ();

            while (_isRunning)
            {
                using (SQLiteConnection c = new SQLiteConnection(connString))
                {
                    c.Open();
                    string query = "SELECT Timestamp, Value FROM RandomWalkData WHERE Timestamp BETWEEN @start AND @end ORDER BY Timestamp";
                    using (SQLiteCommand command = new SQLiteCommand(query, c))
                    {
                        command.Parameters.AddWithValue("@start", startAndEnd.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@end", startAndEnd.EndTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime timeStamp = DateTime.Parse(reader["Timestamp"].ToString());
                                double value = Convert.ToDouble(reader["Value"]);

                                dataToReplay.Add((timeStamp, value));
                            }
                        }
                    }
                }

                foreach (var data in dataToReplay)
                {
                    Console.WriteLine("Replaying: " + data.Item2.ToString("F2"));
                    BroadcastMessage(data.Item2.ToString("F2"));
                    Thread.Sleep(1000);
                }
            }
        }

        public void LoopClients()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient client = _server.AcceptTcpClient();
                    Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                    t.Start(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to create new thread: " + ex.Message);
                }
            }
        }

        public void HandleClient(object obj)
        {
            try
            {
                TcpClient client = (TcpClient)obj;
                if (client != null)
                {
                    NetworkStream stream = client.GetStream();
                    Console.WriteLine("Subscribing client...");
                    _subscribedClients.Add(stream, client);
                    Console.WriteLine("Client subscribed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error within the client: " + ex.Message);
            }
        }

        public void BroadcastMessage(string message)
        {
            foreach (var clientEntry in _subscribedClients)
            {
                NetworkStream clientStream = clientEntry.Key;
                TcpClient client = clientEntry.Value;

                if (!client.Connected)
                {
                    continue;
                }

                SendMessage(clientStream, message);
            }
        }

        public void SendMessage(NetworkStream stream, string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending message: " + ex.Message);
            }

        }
    }
}
