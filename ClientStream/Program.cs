using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientStream
{
    class Program
    {
        //const string SERVER_IP = "127.0.0.1";
        const string SERVER_IP = "192.168.0.110";
        const int SERVER_PORT = 8080;

        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            Console.Write("Connecting... ");

            client.Connect(SERVER_IP, SERVER_PORT);
            Console.WriteLine("Connected\n");

            using (Stream stream = client.GetStream())
            {
                while (true)
                {
                    Console.Write("Enter a string and press ENTER (empty string to exit): ");

                    string message = Console.ReadLine();
                    if (string.IsNullOrEmpty(message))
                        break;

                    byte[] data = Encoding.Default.GetBytes(message);
                    Console.WriteLine("Sending... ");

                    stream.Write(data, 0, data.Length);

                    byte[] response = new byte[4096];
                    int bytesRead = stream.Read(response, 0, response.Length);
                    Console.WriteLine("Response: " + Encoding.Default.GetString(response, 0, bytesRead));

                    Console.WriteLine();
                }
            }

            client.Close();
        }
    }
}
