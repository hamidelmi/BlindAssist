using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Socket = System.Net.Sockets.Socket;

namespace BlindAssist
{
    public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);

    public class SocketServer
    {
        public const int DEFAULT_SERVER_PORT = 8080;
        private Socket socket;
        private int port;

        public event DataReceivedEventHandler DataReceived;

        public SocketServer()
            : this(DEFAULT_SERVER_PORT)
        { }


        public SocketServer(int port)
        {
            this.port = port;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start(string ip) 
        {
            //IPHostEntry entry = Dns.GetHostEntry(Dns.GetHostName());

            //find ip address for your adapter here
            IPAddress localAddress = IPAddress.Parse(ip);

            IPEndPoint localEndPoint = new IPEndPoint(localAddress, port);
            //IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            socket.Bind(localEndPoint);
            socket.Listen(Int32.MaxValue);

            new Thread(StartServerInternal).Start();
        }

        private void StartServerInternal()
        {
            while (true)
            {
                try
                {
                    // Wait for a request from a client.
                    Socket clientSocket = socket.Accept();

                    // Process the client request.
                    var request = new ProcessClientRequest(this, clientSocket);
                    request.Process();
                }
                catch
                {

                }
            }
        }

        private void OnDataReceived(DataReceivedEventArgs e)
        {
            if (DataReceived != null)
                DataReceived(this, e);
        }

        private class ProcessClientRequest
        {
            private Socket clientSocket;
            private SocketServer socket;

            public ProcessClientRequest(SocketServer socket, Socket clientSocket)
            {
                this.socket = socket;
                this.clientSocket = clientSocket;
            }

            public void Process()
            {
                // Handle the request in a new thread.
                new Thread(ProcessRequest).Start();
            }

            private void ProcessRequest()
            {
                const int c_microsecondsPerSecond = 1000000;

                using (clientSocket)
                {
                    while (true)
                    {
                        try
                        {
                            if (clientSocket.Poll(5 * c_microsecondsPerSecond, SelectMode.SelectRead))
                            {
                                // If the butter is zero-lenght, the connection has been closed or terminated.
                                if (clientSocket.Available == 0)
                                    break;

                                byte[] buffer = new byte[clientSocket.Available];
                                int bytesRead = clientSocket.Receive(buffer, clientSocket.Available, SocketFlags.None);

                                byte[] data = new byte[bytesRead];
                                buffer.CopyTo(data, 0);

                                DataReceivedEventArgs args = new DataReceivedEventArgs(clientSocket.LocalEndPoint, clientSocket.RemoteEndPoint, data);
                                socket.OnDataReceived(args);

                                if (args.ResponseData != null)
                                    clientSocket.Send(args.ResponseData);

                                if (args.Close)
                                    break;
                            }
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                }
            }
        }
    } 
}
