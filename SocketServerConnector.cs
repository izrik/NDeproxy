using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;

namespace NDeproxy
{
    public class SocketServerConnector : ServerConnector
    {
        static readonly Logger log = new Logger("SocketServerConnector");
        ListenerThread serverThread;
        Socket serverSocket;
        bool _stop = false;
        List<Action> _shutdownActions = new List<Action>();
        object _shutdownActionsLock = new object();
        public readonly Endpoint endpoint;
        public readonly int port;
        public readonly string name;

        public SocketServerConnector(Endpoint endpoint, string name, int port)
        {

            if (endpoint == null)
            {
                throw new ArgumentNullException("endpoint");
            }

            this.endpoint = endpoint;
            this.name = name;
            this.port = port;

            serverSocket = SocketHelper.Server(port);

            serverThread = new ListenerThread(this, serverSocket, string.Format("Thread-{0}", name));
        }

        public class ListenerThread
        {
            static readonly Logger log = new Logger("ListenerThread");
            public SocketServerConnector parent;
            public Socket socket;
            public Thread thread;
            ManualResetEvent _stopSignal = new ManualResetEvent(false);

            public ListenerThread(SocketServerConnector parent, Socket socket, string name)
            {
                this.parent = parent;
                parent.AddShutdownAction(this.Stop);
                this.socket = socket;

                thread = new Thread(this.Run);
                thread.Name = name;
                thread.IsBackground = true;
                thread.Start();
            }

            public void Run()
            {
                var acceptedSignal = new ManualResetEvent(false);
                var acceptedOrStop = new WaitHandle[2] { acceptedSignal, _stopSignal };

                try
                {
                    while (!_stopSignal.WaitOne(0))
                    {
                        Socket handlerSocket = null;

                        try
                        {
                            log.debug("calling BeginAccept");
                            acceptedSignal.Reset();
                            var ar = socket.BeginAccept((_ar) =>
                            {
                                log.debug("calling EndAccept");
                                try
                                {
                                    handlerSocket = socket.EndAccept(_ar);
                                }
                                catch (Exception ex)
                                {
                                    log.debug("Caught an exception in EndAccept: {0}", ex);
                                }
                                log.debug("returned from EndAccept, signalling completion");
                                acceptedSignal.Set();
                            }, null);
                            log.debug("returned from BeginAccept");

                            log.debug("waiting on the wait handles");
                            var signal = WaitHandle.WaitAny(acceptedOrStop);
                            if (acceptedOrStop[signal] == _stopSignal)
                            {
                                break;
                            }

                            log.debug("Accepted a new connection");

                            string connectionName = Guid.NewGuid().ToString();

                            log.debug("Starting the handler thread");
                            HandlerThread handlerThread = new HandlerThread(this.parent, handlerSocket, connectionName, thread.Name + "-connection-" + connectionName);
                            handlerSocket = null;
                            log.debug("Handler thread started");

                        }
                        catch (Exception e)
                        {
                            log.error("Exception caught in Run, in the second try-block: {0}", e);
                            throw;
                        }
                        finally
                        {
                            if (handlerSocket != null)
                            {
                                handlerSocket.Close();
                            }
                        }
                    }
                }
                finally
                {
                    socket.Close();
                }
            }

            public void Stop()
            {
                _stopSignal.Set();

                if (thread.IsAlive)
                {
                    int time = Environment.TickCount;
                    thread.Join(1000);
                    log.debug("while Stop()ing, joined for {0} ms", Environment.TickCount - time);
                }
                if (thread.IsAlive)
                {
                    thread.Abort();
                }
            }
        }

        public delegate void ConnectionProcessor(Socket socket, string connectionName);

        public class HandlerThread
        {
            static readonly Logger log = new Logger("HandlerThread");
            public SocketServerConnector parent;
            public Socket socket;
            public string connectionName;
            public Thread thread;
            ManualResetEvent _stopSignal = new ManualResetEvent(false);

            public HandlerThread(SocketServerConnector parent,
                                 Socket socket,
                                 string connectionName,
                                 string threadName)
            {
                if (socket == null)
                    throw new ArgumentNullException("socket");

                this.parent = parent;
                parent.AddShutdownAction(this.Stop);

                this.socket = socket;
                this.connectionName = connectionName;

                thread = new Thread(this.processNewConnection);
                thread.Name = threadName;
                thread.IsBackground = true;
                thread.Start();
            }

            public void Stop()
            {
                _stopSignal.Set();

                if (socket != null)
                {
                    ShutdownSocket();
                }

                if (thread.IsAlive)
                {
                    log.debug("thread is alive");
                    int time = Environment.TickCount;
                    log.debug("joining thread");
                    thread.Join(1000);
                    log.debug("while Stop()ing, joined for {0} ms", Environment.TickCount - time);
                }
                if (thread.IsAlive)
                {
                    log.debug("thread is still alive");
                    thread.Abort();
                    log.debug("thread aborted");
                }
            }

            void ShutdownSocket()
            {
                Socket s = Interlocked.Exchange(ref socket, null);
                if (s != null)
                {
                    log.debug("shutting down the socket");
                    s.Shutdown(SocketShutdown.Both);
                    log.debug("closing the socket");
                    s.Close();
                }
            }

            void processNewConnection()
            {

                log.debug("processing new connection...");
                Stream stream = new UnbufferedSocketStream(socket);

                try
                {
                    log.debug("starting loop");
                    bool persistConnection = true;
                    while (persistConnection && !_stopSignal.WaitOne(0))
                    {

                        log.debug("calling parseRequest");
                        var ret = parseRequest(stream);
                        log.debug("returned from parseRequest");

                        if (ret == null)
                        {
                            break;
                        }

                        Request request = ret.Item1;
                        persistConnection = ret.Item2;

                        if (persistConnection &&
                            request.headers.contains("Connection"))
                        {
                            foreach (var value in request.headers.findAll("Connection"))
                            {
                                if (value == "close")
                                {
                                    persistConnection = false;
                                    break;
                                }
                            }
                        }

                        log.debug("about to handle one request");
                        ResponseWithContext rwc = parent.endpoint.handleRequest(request, connectionName);
                        log.debug("handled one request");


                        log.debug("send the response");
                        sendResponse(stream, rwc.response, rwc.context);

                        if (persistConnection &&
                            rwc.response.headers.contains("Connection"))
                        {
                            foreach (var value in rwc.response.headers.findAll("Connection"))
                            {
                                if (value == "close")
                                {
                                    persistConnection = false;
                                    break;
                                }
                            }
                        }
                    }

                    log.debug("ending loop");

                }
                catch (Exception e)
                {
                    log.error("there was an error: {0}", e);
                    if (_stopSignal.WaitOne(0) ||
                        (socket != null &&
                        socket.IsClosed()))
                    {
                        // do nothing
                    }
                    else
                    {
                        sendResponse(stream,
                            new Response(500, "Internal Server Error", null,
                                "The server encountered an unexpected condition which prevented it from fulfilling the request."));
                    }
                }
                finally
                {
                    ShutdownSocket();
                }

                log.debug("done processing");

            }

            Tuple<Request, bool> parseRequest(Stream stream)
            {

                log.debug("reading request line");
                var requestLine = LineReader.readLine(stream);

                if (requestLine == null)
                {
                    log.debug("request line is null: {0}", requestLine);

                    return null;
                }

                log.debug("request line is not null: {0}", requestLine);

                var words = requestLine.Split(' ', '\t');
                log.debug("{0}", words);

                string version;
                string method;
                string path;
                if (words.Length == 3)
                {

                    method = words[0];
                    path = words[1];
                    version = words[2];

                    log.debug("{0}, {1}, {2}", method, path, version);

                    if (!version.StartsWith("HTTP/", StringComparison.Ordinal))
                    {
                        sendResponse(stream, new Response(400, null, null, string.Format("Bad request version \"{0}\"", version)));
                        return null;
                    }

                }
                else
                {

                    sendResponse(stream, new Response(400));
                    return null;
                }

                log.debug("checking http protocol version: {0}", version);
                if (version != "HTTP/1.1" &&
                    version != "HTTP/1.0" &&
                    version != "HTTP/0.9")
                {

                    sendResponse(stream, new Response(505, null, null, string.Format("Invalid HTTP Version \"{0}\"}", version)));
                    return null;
                }

                HeaderCollection headers = HeaderReader.readHeaders(stream);

                var persistentConnection = false;
                if (version == "HTTP/1.1")
                {
                    persistentConnection = true;
                    foreach (var value in headers.findAll("Connection"))
                    {
                        if (value == "close")
                        {
                            persistentConnection = false;
                        }
                    }
                }

                log.debug("reading the body");
                var body = BodyReader.readBody(stream, headers);

                int length;
                if (body == null)
                {
                    length = 0;
                }
                else if (body is byte[])
                {
                    length = (body as byte[]).Length;
                }
                else
                {
                    length = body.ToString().Length;
                }

                log.debug("Done reading body, length {0}", length);

                log.debug("returning");
                return new Tuple<Request, bool>(
                    new Request(method, path, headers, body),
                    persistentConnection
                );
            }

            void sendResponse(Stream stream, Response response, HandlerContext context = null)
            {

                var writer = new StreamWriter(stream, Encoding.ASCII);

                if (response.message == null)
                {
                    response.message = "";
                }

                writer.Write("HTTP/1.1 {0} {1}", response.code, response.message);
                writer.Write("\r\n");

                writer.Flush();

                HeaderWriter.writeHeaders(stream, response.headers);

                BodyWriter.writeBody(response.body, stream,
                    (context != null && context.usedChunkedTransferEncoding));

                log.debug("finished sending response");
            }
        }

        public Socket createRawConnection()
        {
            return SocketHelper.Client("localhost", this.port);
        }

        public void shutdown()
        {
            _stop = true;
            ExecuteShutdownActions();

            if (serverThread != null)
            {
//                serverThread.interrupt();
//                throw new NotImplementedException();
            }
            if (serverSocket != null)
            {
                serverSocket.Close();
            }
        }

        private void AddShutdownAction(Action action)
        {
            lock (_shutdownActionsLock)
            {
                _shutdownActions.Add(action);
            }
        }

        private void ExecuteShutdownActions()
        {
            lock (_shutdownActionsLock)
            {
                foreach (var action in _shutdownActions)
                {
                    action();
                }

                _shutdownActions.Clear();
            }
        }
    }
}
