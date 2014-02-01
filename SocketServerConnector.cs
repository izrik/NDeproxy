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
        static readonly Logger log = new Logger();

        ListenerThread serverThread;
        Socket serverSocket;

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
            SocketServerConnector parent;
            Socket socket;
            Thread thread;
            bool _stop = false;

            public ListenerThread(SocketServerConnector parent, Socket socket, string name)
            {

                this.parent = parent;
                this.socket = socket;

                thread = new Thread(this.Run);
                thread.Name = name;
                thread.Start();
            }

            public void Run()
            {

                while (!_stop)
                {
                    try
                    {
//                        this.socket.setSoTimeout(1000);

                        Socket socket;
                        try
                        {
                            socket = this.socket.Accept();
                        }
                        catch (SocketException e)
                        {
                            log.error("There was an exception, caught in Run: {0}", e);
                            throw;
                        }

                        log.debug("Accepted a new connection");
                        //socket.setSoTimeout(1000);
                        log.debug("Creating the handler thread");

                        string connectionName = Guid.NewGuid().ToString();

                        log.debug("Starting the handler thread");
                        HandlerThread handlerThread = new HandlerThread(this.parent, socket, connectionName, thread.Name + "-connection-" + connectionName);
                        log.debug("Handler thread started");

                    }
//                    catch (SocketTimeoutException ste)
//                    {
//                        // do nothing
//                    }
                    catch (IOException ex)
                    {
                        log.error(null, ex);
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
                }
            }

            public void Stop()
            {
                _stop = true;

                thread.Join(1000);
                thread.Abort();
            }
        }

        public class HandlerThread
        {
            static readonly Logger log = new Logger();

            SocketServerConnector parent;
            Socket socket;
            string connectionName;
            Thread thread;

            public HandlerThread(SocketServerConnector parent,
                                 Socket socket,
                                 string connectionName,
                                 string threadName)
            {

                this.parent = parent;
                this.socket = socket;
                this.connectionName = connectionName;

                thread = new Thread(this.run);
                thread.Name = threadName;
                thread.Start();
            }

            public void run()
            {

                log.debug("Processing new connection");

                this.parent.processNewConnection(this.socket, connectionName);

                log.debug("Connection processed");
            }
        }

        public Socket createRawConnection()
        {
            return SocketHelper.Client("localhost", this.port);
        }

        void processNewConnection(Socket socket, string connectionName)
        {

            log.debug("processing new connection...");
            Stream stream;

            try
            {

                log.debug("getting reader");
                log.debug("getting writer");
                stream = new NetworkStream(socket);

                try
                {

                    log.debug("starting loop");
                    bool persistConnection = true;
                    while (persistConnection)
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
                        ResponseWithContext rwc = endpoint.handleRequest(request, connectionName);
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
                    log.error("there was an error", e);
                    sendResponse(stream,
                        new Response(500, "Internal Server Error", null,
                            "The server encountered an unexpected condition which prevented it from fulfilling the request."));
                    throw;
                }
            }
            finally
            {
                //socket.shutdownInput()
                //socket.shutdownOutput()
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            log.debug("done processing");

        }

        public void shutdown()
        {

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

            var words = requestLine.Split(' ','\t');
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
                context.usedChunkedTransferEncoding);

            log.debug("finished sending response");
        }
    }
}
