using System;
using System.Collections.Generic;

namespace NDeproxy
{
    public class Endpoint
    {
        static readonly Logger log = new Logger();
        public readonly string name;
        public readonly string hostname;
        public readonly HandlerWithContext defaultHandler;
        public readonly ServerConnector serverConnector;
        public readonly Deproxy deproxy;

        public Endpoint(Deproxy deproxy, Handler defaultHandler, int? port = null,
                        string name = null, string hostname = null,
                        ServerConnectorFactory connectorFactory = null)
            : this(deproxy: deproxy, port: port, name: name, hostname: hostname,
                   defaultHandler: defaultHandler.WithContext(), connectorFactory: connectorFactory)
        {
        }

        public Endpoint(Deproxy deproxy, int? port = null, string name = null,
                        string hostname = null, HandlerWithContext defaultHandler = null,
                        ServerConnectorFactory connectorFactory = null)
        {

            if (deproxy == null)
                throw new ArgumentNullException();

            if (name == null)
                name = string.Format("Endpoint-{0}", System.Environment.TickCount);
            if (hostname == null)
                hostname = "localhost";

            this.deproxy = deproxy;
            this.name = name;
            this.hostname = hostname;
            this.defaultHandler = defaultHandler;

            if (connectorFactory != null)
            {
                this.serverConnector = connectorFactory(this, name);
            }
            else
            {
                if (port == null)
                {
                    port = PortFinder.Singleton.getNextOpenPort();
                }
                this.serverConnector = new SocketServerConnector(this, name, port.Value);
            }
        }

        public void shutdown()
        {
            log.debug("Shutting down {0}", this.name);
            serverConnector.shutdown();
            log.debug("Finished shutting down {0}", this.name);
        }

        public ResponseWithContext handleRequest(Request request, string connectionName)
        {

            log.debug("Begin handleRequest");

            try
            {

                MessageChain messageChain = null;

                var requestId = request.headers.getFirstValue(Deproxy.REQUEST_ID_HEADER_NAME);
                if (requestId != null)
                {

                    log.debug("the request has a request id: {0}={1}", Deproxy.REQUEST_ID_HEADER_NAME, requestId);

                    messageChain = this.deproxy.getMessageChain(requestId);

                }
                else
                {

                    log.debug("the request does not have a request id");
                }

                // Handler resolution:
                // 1. Check the handlers mapping specified to ``makeRequest``
                //   a. By reference
                //   b. By name
                // 2. Check the defaultHandler specified to ``makeRequest``
                // 3. Check the default for this endpoint
                // 4. Check the default for the parent Deproxy
                // 5. Fallback to simpleHandler

                HandlerWithContext handler;
                if (messageChain != null &&
                    messageChain.handlers != null &&
                    messageChain.handlers.ContainsKey(this))
                {
                    handler = messageChain.handlers[this];
                }
                else if (messageChain != null &&
                         messageChain.defaultHandler != null)
                {
                    handler = messageChain.defaultHandler;
                }
                else if (this.defaultHandler != null)
                {
                    handler = this.defaultHandler;
                }
                else if (this.deproxy.defaultHandler != null)
                {
                    handler = this.deproxy.defaultHandler;
                }
                else
                {
                    handler = Handlers.simpleHandler;
                }

                log.debug("calling handler");
                Response response;
                HandlerContext context = new HandlerContext();

                response = handler(request, context);

                log.debug("returned from handler");


                if (context.sendDefaultResponseHeaders)
                {

                    if (!response.headers.contains("Server"))
                    {
                        response.headers.add("Server", Deproxy.VERSION_STRING);
                    }
                    if (!response.headers.contains("Date"))
                    {
                        response.headers.add("Date", datetimeString());
                    }

                    if (response.body != null)
                    {

                        if (context.usedChunkedTransferEncoding)
                        {

                            if (!response.headers.contains("Transfer-Encoding"))
                            {
                                response.headers.add("Transfer-Encoding", "chunked");
                            }

                        }
                        else if (!response.headers.contains("Transfer-Encoding") ||
                                 response.headers["Transfer-Encoding"] == "identity")
                        {

                            int length;
                            string contentType;
                            if (response.body is string)
                            {
                                length = ((string)response.body).Length;
                                contentType = "text/plain";
                            }
                            else if (response.body is byte[])
                            {
                                length = ((byte[])response.body).Length;
                                contentType = "application/octet-stream";
                            }
                            else
                            {
                                throw new InvalidOperationException("Unknown data type in requestBody");
                            }

                            if (length > 0)
                            {
                                if (!response.headers.contains("Content-Length"))
                                {
                                    response.headers.add("Content-Length", length);
                                }
                                if (!response.headers.contains("Content-Type"))
                                {
                                    response.headers.add("Content-Type", contentType);
                                }
                            }
                        }
                    }

                    if (!response.headers.contains("Content-Length") &&
                        !response.headers.contains("Transfer-Encoding"))
                    {

                        response.headers.add("Content-Length", 0);
                    }
                }

                if (requestId != null &&
                    !response.headers.contains(Deproxy.REQUEST_ID_HEADER_NAME))
                {
                    response.headers.add(Deproxy.REQUEST_ID_HEADER_NAME, requestId);
                }

                var handling = new Handling(this, request, response, connectionName);
                if (messageChain != null)
                {
                    messageChain.addHandling(handling);
                }
                else
                {
                    this.deproxy.addOrphanedHandling(handling);
                }

                return new ResponseWithContext{ response = response, context = context };

            }
            finally
            {

            }
        }

        string datetimeString()
        {
            // Return the current date and time formatted for a message header.

            return DateTime.UtcNow.ToString("r");
        }
    }
}
