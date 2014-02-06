using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace NDeproxy
{
    public class PortFinder
    {
        static readonly Logger log = new Logger("PortFinder");
        public static readonly PortFinder Singleton = new PortFinder();

        public PortFinder(int start = 10000)
        {
            currentPort = start;
        }

        public int currentPort;
        public int skips = 0;
        readonly object _lock = new object();

        public int getNextOpenPort(int newStartPort = -1, int sleepTime = 0)
        {
            lock (_lock)
            {
                if (newStartPort >= 0)
                {
                    currentPort = newStartPort;
                }

                while (currentPort < 65536)
                {
                    try
                    {
                        var url = string.Format("http://localhost:{0}/", currentPort);
                        log.debug("Trying {0}", currentPort);
                        using (Socket socket = SocketHelper.Client("localhost", currentPort))
                        {
                        }
                    }
                    catch (SocketException e)
                    {
                        if (e.SocketErrorCode == SocketError.ConnectionRefused)
                        {
                            log.debug("Didn't connect, using this one");
                            currentPort++;
                            return currentPort - 1;
                        }

                        // ignore the exception
                        log.warn("Got a SocketException: " + e.ToString());
                    }
//                catch (IOException e)
//                {
//                    // ignore the exception
//                    log.warn("Got a IOException: " + e.ToString());
//                }
                catch (Exception e)
                    {
                        log.warn("Got an Exception: " + e.ToString());
                        throw;
                    }

                    Thread.Sleep(sleepTime);
                    log.debug("Connected");

                    currentPort++;
                    skips++;
                }

                throw new InvalidOperationException("Ran out of ports");
            }
        }
    }
}
