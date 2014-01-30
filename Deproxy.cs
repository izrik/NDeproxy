using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NDeproxy
{
    public class Deproxy
    {
        static readonly Logger log = new Logger();
        public static readonly string REQUEST_ID_HEADER_NAME = "Deproxy-Request-ID";
        public static readonly string VERSION = getVersion();
        public static readonly string VERSION_STRING = string.Format("deproxy {0}", VERSION);
        public HandlerWithContext defaultHandler = null;
        public ClientConnector defaultClientConnector;
        protected readonly object messageChainsLock = new object();
        protected Dictionary<string, MessageChain> messageChains = new Dictionary<string, MessageChain>();
        protected readonly object endpointLock = new object();
        protected List<Endpoint> endpoints = new List<Endpoint>();

        private static string getVersion()
        {
//        def res = Deproxy.class.getResourceAsStream("version.txt");
//                  def bytes = [];
//        while (true) {
//            def b = res.read();
//            if (b < 0) break;
//
//            bytes += (byte) b;
//        }
//
//        return new string(bytes as byte[], "UTF-8");
            var assembly = Assembly.GetCallingAssembly();
            var name = assembly.GetName();
            var version = name.Version;
            return version.ToString();
        }

        public Deproxy(Handler defaultHandler, ClientConnector defaultClientConnector = null)
            : this(defaultHandler.WithContext(), defaultClientConnector)
        {
        }

        public Deproxy(HandlerWithContext defaultHandler = null, ClientConnector defaultClientConnector = null)
        {

            if (defaultClientConnector == null)
            {
                defaultClientConnector = new DefaultClientConnector();
            }

            this.defaultHandler = defaultHandler;
            this.defaultClientConnector = defaultClientConnector;
        }

        public MessageChain makeRequest(
            string url,
            Handler defaultHandler,
            string host = null,
            int? port = null,
            string method = "GET",
            string path = null,
            object headers = null,
            object requestBody = null,
            Dictionary<Endpoint, HandlerWithContext> handlers = null,
            bool addDefaultHeaders = true,
            bool chunked = false,
            ClientConnector clientConnector = null)
        {
            return makeRequest(url, host, port, method, path, headers, requestBody, 
                defaultHandler.WithContext(), handlers, addDefaultHeaders,
                chunked, clientConnector);
        }

        public MessageChain makeRequest(
            string url,
            string host = null,
            int? port = null,
            string method = "GET",
            string path = null,
            object headers = null,
            object requestBody = null,
            HandlerWithContext defaultHandler = null,
            Dictionary<Endpoint, HandlerWithContext> handlers = null,
            bool addDefaultHeaders = true,
            bool chunked = false,
            ClientConnector clientConnector = null)
        {

            if (requestBody == null)
                requestBody = "";

            // url --> https host port path
            // https host port --> connection
            // method path headers requestBody --> request

            // specifying the path param overrides the path in the url param

            log.debug("begin makeRequest");

            HeaderCollection headers2 = new HeaderCollection(headers);

            if (clientConnector == null)
            {
                clientConnector = this.defaultClientConnector;
            }

            var requestId = Guid.NewGuid().ToString();

            if (!headers2.contains(REQUEST_ID_HEADER_NAME))
            {
                headers2.add(REQUEST_ID_HEADER_NAME, requestId);
            }

            var messageChain = new MessageChain(defaultHandler, handlers);
            addMessageChain(requestId, messageChain);

            bool https = false;
            if ((host == null || path == null || port == null) && url != null)
            {

                var uri = new Uri(url);

                if (string.IsNullOrWhiteSpace(host))
                {
                    host = uri.Host;
                }

                if (port == null)
                {
                    port = uri.Port;
                }

                https = (uri.Scheme == "https");

                if (path == null)
                {
                    var urib = new UriBuilder(uri.Scheme, uri.Host, uri.Port);
                    var uri2 = urib.Uri;
                    path = uri2.MakeRelativeUri(uri).ToString();
                    if (!path.StartsWith("/"))
                    {
                        path = "/" + path;
                    }
                }
            }


            log.debug("request body: {0}", requestBody);

            Request request = new Request(method, path, headers2, requestBody);

            RequestParams requestParams = new RequestParams();
            requestParams.usedChunkedTransferEncoding = chunked;
            requestParams.sendDefaultRequestHeaders = addDefaultHeaders;

            log.debug("calling sendRequest");
            Response response = clientConnector.sendRequest(request, https, host, port, requestParams);
            log.debug("back from sendRequest");

            removeMessageChain(requestId);

            messageChain.sentRequest = request;
            messageChain.receivedResponse = response;

            log.debug("end makeRequest");

            return messageChain;
        }

        public Endpoint addEndpoint(Handler defaultHandler, int? port = null, string name = null,
                                    string hostname = null,
                                    ServerConnectorFactory connectorFactory = null)
        {
            return addEndpoint(port: port, name: name, hostname: hostname,
                defaultHandler: defaultHandler.WithContext(),
                connectorFactory: connectorFactory);
        }

        public Endpoint addEndpoint(int? port = null, string name = null, string hostname = null,
                                    HandlerWithContext defaultHandler = null,
                                    ServerConnectorFactory connectorFactory = null)
        {

            lock (this.endpointLock)
            {

                Endpoint endpoint =
                    new Endpoint(
                        deproxy: this,
                        port: port,
                        name: name,
                        hostname: hostname,
                        defaultHandler: defaultHandler,
                        connectorFactory: connectorFactory);

                this.endpoints.Add(endpoint);

                return endpoint;
            }
        }

        bool  _removeEndpoint(Endpoint endpoint)
        {

            lock (this.endpointLock)
            {

                var count = this.endpoints.Count;

                this.endpoints = this.endpoints.Where(e => e != endpoint).ToList();

                return (count != this.endpoints.Count);
            }
        }

        public void shutdown()
        {

            lock (this.endpointLock)
            {
                foreach (var e in this.endpoints)
                {
                    e.shutdown();
                }
                this.endpoints.Clear();
            }
        }

        public void addMessageChain(string requestId, MessageChain messageChain)
        {

            lock (this.messageChainsLock)
            {

                this.messageChains[requestId] = messageChain;
            }
        }

        public void removeMessageChain(string requestId)
        {

            lock (this.messageChainsLock)
            {

                this.messageChains.Remove(requestId);
            }
        }

        public MessageChain getMessageChain(string requestId)
        {

            lock (this.messageChainsLock)
            {

                if (this.messageChains.ContainsKey(requestId))
                {

                    return this.messageChains[requestId];

                }
                else
                {

                    return null;
                }
            }
        }

        public void addOrphanedHandling(Handling handling)
        {

            lock (this.messageChainsLock)
            {

                foreach (var mc in this.messageChains.Values)
                {

                    mc.addOrphanedHandling(handling);
                }
            }
        }
    }
}
