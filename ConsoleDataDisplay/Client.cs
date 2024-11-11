using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDataDisplay
{
    public class Client
    {
        private TcpClient _client;

        public Client()
        {
            try
            {
                _client = new TcpClient("127.0.0.1", 5000);
                Console.WriteLine("Connected to the server.");
                Run();
            } catch (Exception ex)
            {
                Console.WriteLine("Error creating the client: " + ex.Message);
            }

        }

        private void Run()
        {
            try
            {
                NetworkStream stream = _client.GetStream();
                byte[] buffer = new byte[256];

                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string receivedValue = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Current value: " + receivedValue);
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
