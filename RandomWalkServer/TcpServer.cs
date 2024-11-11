using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RandomWalkServer
{
    public class TcpServer
    {
        private Boolean _isRunning;
        private TcpListener _server;
        private Dictionary<NetworkStream, TcpClient> _subscribedClients = new Dictionary<NetworkStream, TcpClient>();
        private DbInstance _dbInstance;

        private Random random = new Random();

        private double currentValue = 0;
        private double probabilityUp = 0.6;

        public TcpServer(int port, DbInstance dbInstance) 
        {   
            try
            {
                _server = new TcpListener(IPAddress.Any, port);
                _server.Start();
                Console.WriteLine("Server started, waiting for client to connect...");

                _dbInstance = dbInstance;
                _isRunning = true;

                currentValue = Math.Round(random.NextDouble() * (80.00 - 20.00) + 20, 2);
                Thread dataThread = new Thread(GenerateDataAndSend);
                dataThread.Start();

                LoopClients();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on server: " + ex.Message);
            }

        }

        public void GenerateDataAndSend()
        {
            while (_isRunning)
            {
                Console.WriteLine("Sending: " + currentValue.ToString("F2"));
                BroadcastMessage(currentValue.ToString("F2"));

                _dbInstance.LogValue(currentValue);

                int direction = random.Next(1, 101) >= (int)(probabilityUp * 100) ? -1 : 1;
                double stepSize = currentValue >= 60.0 ? 0.05 : 0.01;

                currentValue += direction * stepSize;
                currentValue = Math.Round(currentValue, 2);

                Thread.Sleep(1000);
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
                } catch (Exception ex)
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
            } catch (Exception ex)
            {
                Console.WriteLine("Error within the client: " + ex.Message);
            }
        }

        public void BroadcastMessage(string message)
        {
            foreach(var clientEntry in _subscribedClients)
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
            } catch (Exception ex)
            {
                Console.WriteLine("Error sending message: " + ex.Message);
            }
            
        }
    }
}
