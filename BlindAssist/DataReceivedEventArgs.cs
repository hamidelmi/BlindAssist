using System;
using Microsoft.SPOT;
using System.Net;
using System.Net.Sockets;

namespace BlindAssist
{
    /// <summary>
    /// Event argument of Data receive event which would be used
    /// when a data received by the SocketServer to let the client 
    /// know about incoming data.
    /// It also provide the ability to the client in order to pass some 
    /// data as the respose and use the same stream to push back
    /// the data to the remote sender.
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        public EndPoint LocalEndPoint { get; private set; }
        public EndPoint RemoteEndPoint { get; private set; }
        public byte[] Data { get; private set; }
        public bool Close { get; set; }
        public byte[] ResponseData { get; set; }

        public DataReceivedEventArgs(EndPoint localEndPoint, EndPoint remoteEndPoint, byte[] data)
        {
            LocalEndPoint = localEndPoint;
            RemoteEndPoint = remoteEndPoint;
            if (data != null)
            {
                Data = new byte[data.Length];
                data.CopyTo(Data, 0);
            }
        }
    }
}
