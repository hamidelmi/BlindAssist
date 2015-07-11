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
    /// Socket client class to send the result(Found RFID tags) from the gadgeteer to the Mobile phone
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
        /// Sends the data to the Mobilephone server (The opened port by the Client as a receiving server) 8081
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
