using System;
using Microsoft.SPOT;
using System.Net;
using System.Net.Sockets;

namespace BlindAssist
{
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
