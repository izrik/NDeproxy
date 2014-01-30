using System;
using System.Net.Sockets;
using System.Net;

namespace NDeproxy
{
    public static class SocketHelper
    {
        public static Socket Server(int port, int listenQueue=5)
        {
            var s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Bind(new IPEndPoint(IPAddress.Any, port));
            s.Listen(listenQueue);
            return s;
        }

        public static Socket Client(string remoteHost, int port)
        {
            var s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Connect(remoteHost, port);
            return s;
        }

        public static int GetLocalPort(this Socket socket)
        {
            return ((IPEndPoint)socket.LocalEndPoint).Port;
        }

        public static int GetRemotePort(this Socket socket)
        {
            return ((IPEndPoint)socket.RemoteEndPoint).Port;
        }
    }
}

