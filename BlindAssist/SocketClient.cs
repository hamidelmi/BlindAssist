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
    /// This class provides an interface to its client to be able to send data via
    /// Socket.
    /// This class would close the connection immediately after sending the data.
    /// </summary>
    public class SocketClient
    {
        string server;
        int port;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="server"></param>
        /// <param name="port"></param>
        public SocketClient(string server, int port)
        {
            this.server = server;
            this.port = port;
        }
        /// <summary>
        /// Sends the data to the server 
        /// </summary>
        /// <param name="data">data to be sent</param>
        public void Send(string data)
        {
            try
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(new IPEndPoint(IPAddress.Parse(server), port));
                socket.Send(new System.Text.UTF8Encoding().GetBytes(data));

                socket.Close();
            }
            catch (Exception x)
            {
                Debug.Print(x.Message);
            }
        }
    }
}
