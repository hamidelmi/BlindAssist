using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Socket = System.Net.Sockets.Socket;

namespace BlindAssist
{
    /// <summary>
    /// Socket Server Class
    /// The gadgeteer will be in listenning on port No. 8080 mode once the application is started
    /// A socket server is crated once the app is started and receives the data on Port 8080 from the the connected clients
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);

    public class SocketServer
    {

        #region Variables
        private Socket socket;
        private int port;
        private string remoteIP;
        public string RemoteIP
        {
            get
            {
                return remoteIP;
            }
            set
            {
                if (remoteIP == value)
                    return;
                remoteIP = value;
                if (RemoteIPChanged != null)
                    RemoteIPChanged(this, null);
            }
        } 
       

        /// <summary>
        /// It would be raised when a new data arrives from the remote sender.
        /// </summary>
        public event DataReceivedEventHandler DataReceived;

        /// <summary>
        /// It would be raised when a new device is connected.
        /// </summary>
        public event EventHandler RemoteIPChanged;
        #endregion

        /// <summary>
        /// Constructor
        /// Create the SocketServer
        /// </summary>
        /// <param name="port"></param>
        public SocketServer(int port)
        {
            this.port = port;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// start the server on the given IP address
        /// </summary>
        /// <param name="ip">IP address of the server</param>
        public void Start(string ip)
        {
            //find ip address for your adapter here
            IPAddress localAddress = IPAddress.Parse(ip);
            IPEndPoint localEndPoint = new IPEndPoint(localAddress, port);
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
                    var clientSocket = socket.Accept();
                    var ip = (clientSocket.RemoteEndPoint as IPEndPoint).Address.ToString();
                    if (this.RemoteIP != ip)
                        this.RemoteIP = ip;
                    // Process the client request.
                    var request = new ProcessClientRequest(this, clientSocket);
                    request.Process();
                }
                catch (Exception e)
                {
                    Debug.Print(e.Message);
                }
            }
        }

        private void OnDataReceived(DataReceivedEventArgs e)
        {
            if (DataReceived != null)
                DataReceived(this, e);
        }

        #region ProcessClientRequest
        /// <summary>
        /// This class is in charge of processing data coming from the remote sender.
        /// This class internally use a new thread to handle the request 
        /// and it will be done when it received close message from the event handler 
        /// </summary>
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
            #endregion
    }
}