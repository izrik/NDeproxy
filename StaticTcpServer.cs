using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NDeproxy
{
    public class StaticTcpServer
    {
        public static string handleOneRequest(Socket socket, string responseString, int requestLength)
        {

            byte[] bytes = new byte[requestLength];
            int n = 0;
            var stream = new NetworkStream(socket);
            while (n < bytes.Length)
            {
                var count = stream.Read(bytes, n, bytes.Length - n);
                n += count;
            }

            String serverSideRequest = Encoding.ASCII.GetString(bytes);

            bytes = Encoding.ASCII.GetBytes(responseString);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();

            return serverSideRequest;
        }

        public static string handleOneRequestTimeout(Socket socket, string responseString, int timeoutMillis)
        {

            int oldTimeout = socket.ReceiveTimeout;
            long startTimeMillis = System.Environment.TickCount;

            List<byte> bytes = new List<byte>();
            var stream = new NetworkStream(socket);

            try
            {

                socket.ReceiveTimeout = 100;

                while (true)
                {
                    try
                    {

                        var value = stream.ReadByte();
                        if (value >= 0)
                        {
                            bytes.Add((byte)value);
                        }

                    }
                    catch (SocketException ignored)
                    {
                        //TODO: make sure it's a socket timeout
                        throw;

                        long currentTimeMillis = System.Environment.TickCount;

                        if (currentTimeMillis >= startTimeMillis + timeoutMillis)
                        {
                            break;
                        }
                    }
                }

            }
            finally
            {

                if (socket != null)// &&
                    //!socket.isClosed())
                {
                    socket.ReceiveTimeout = oldTimeout;
                }
            }

            string serverSideRequest = Encoding.ASCII.GetString(bytes.ToArray());

            var bytes2 = Encoding.ASCII.GetBytes(responseString);
            stream.Write(bytes2, 0, bytes2.Length);
            stream.Flush();

            return serverSideRequest;
        }
    }
}