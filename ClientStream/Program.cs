using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientStream
{
    /// <summary>
    ///  Test class
    ///  Not in use
    /// </summary>
    class Program
    {
        const string SERVER_IP = "192.168.0.110";
        const int SERVER_PORT = 8080;


        static void startServer(object o)
        {
            TcpListener listener = new TcpListener(SERVER_PORT + 1);

            listener.Start();
            while (true)
            {
                Socket client = listener.AcceptSocket();
                Console.WriteLine("Connection accepted.");

                var childSocketThread = new Thread(() =>
                {
                    byte[] data = new byte[100];
                    int size = client.Receive(data);
                    Console.WriteLine("Recieved data: ");
                    for (int i = 0; i < size; i++)
                        Console.Write(Convert.ToChar(data[i]));

                    Console.WriteLine();

                    client.Close();
                });
                childSocketThread.Start();
            }
        }
        static void Main(string[] args)
        {
            new Thread(new ParameterizedThreadStart(startServer)).Start();
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
