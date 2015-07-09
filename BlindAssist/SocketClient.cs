using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Socket = System.Net.Sockets.Socket;

namespace BlindAssist
{
    public class SocketClient
    {
        string server;
        int port;
        public SocketClient(string server, int port)
        {
            this.server = server;
            this.port = port;
        }
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
