using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace NDeproxy
{
    public class LocalSocketPair
    {
        public static Tuple<Socket, Socket> createLocalSocketPair(int? port = null)
        {

            if (port == null)
            {
                port = PortFinder.Singleton.getNextOpenPort();
            }

            // create the listener socket
            Socket listener = SocketHelper.Server(port.Value);
            Socket server = null;
            ManualResetEvent mre = new ManualResetEvent(false);

            // start listening on a separate thread
            ThreadPool.QueueUserWorkItem((x) =>
            {
                server = listener.Accept();
                mre.Set();
            });

            // create the client socket and connect
            var client = SocketHelper.Client("localhost", port.Value);

            mre.WaitOne(500);

            listener.Close();

            return new Tuple<Socket, Socket>(client, server);
        }
    }
}
