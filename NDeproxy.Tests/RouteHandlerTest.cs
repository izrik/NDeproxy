using NUnit.Framework;
using NDeproxy;
using System.Threading;
using System.Net.Sockets;

namespace NDeproxy.Tests
{
    [TestFixture]
    class RouteHandlerTest
    {
        Deproxy deproxy;
        int port;
        Socket client;
        Socket server;
        Thread t;

        [SetUp]
        public void setup()
        {
            port = PortFinder.Singleton.getNextOpenPort();

            deproxy = new Deproxy();
            deproxy.addEndpoint(port: port, name: "server", hostname: "localhost", defaultHandler: this.handler);
        }

        Response handler(Request request)
        {

            return new Response(606, "Spoiler", null, "Snape Kills Dumbledore!");
        }

        [Test]
        public void testRoute()
        {

            given: //"set up the route handler and the request"
            var router = Handlers.Route("localhost", port);
            Request request = new Request("METHOD", "/path/to/resource", "Name: Value", "this is the body");

            when: //"sending the request via the router"
            Response response = router(request, new HandlerContext());

            then: //"the request is served by the endpoint and handler"
            Assert.AreEqual("606", response.code);
            Assert.AreEqual("Spoiler", response.message);
            Assert.AreEqual("Snape Kills Dumbledore!", response.body);
        }

        [Test]
        public void testRouteServerSideRequest()
        {

            given: //"define the expected request string, and the string to return from the fake server"
            string requestString = (string.Format("METHOD /path/to/resource HTTP/1.1\r\n" +
                                   "Name: Value\r\n" +
                                   "Content-Length: 0\r\n" +
                                   "Host: localhost:{0}\r\n" +
                                   "\r\n", port));
            string responseString = ("HTTP/1.1 606 Spoiler\r\n" +
                                    "Server: StaticTcpServer\r\n" +
                                    "Content-Length: 0\r\n" +
                                    "\r\n");

//        and: "set up the fake server"
            var pair = LocalSocketPair.createLocalSocketPair();
            client = pair.Item1;
            server = pair.Item2;
            client.ReceiveTimeout = 2000; // in milliseconds
            server.ReceiveTimeout = 2000;
            string serverSideRequest = null;
            t = new Thread(() =>
            {
                serverSideRequest = StaticTcpServer.handleOneRequest(server, responseString, requestString.Length);
            });
            t.Start();

//        and: "set up the connector for Route to use"
            BareClientConnector connector = new BareClientConnector(client);

//        and: "set up the router and request"
            var router = Handlers.Route("localhost", port, false, connector);
            Request request = new Request(
                                  "METHOD",
                                  "/path/to/resource",
                                  new [] { "Name: Value", "Content-Length: 0" });



            when: //"sending the request via the router"
            Response response = router(request, new HandlerContext());

//        and: "wait for the thread to assign the variable"
            t.Join(1000);

            then: //"the request that the fake server received is what we expected"
            Assert.AreEqual(requestString, serverSideRequest);

//        and: "the request is served by the StaticTcpServer"
            Assert.AreEqual("606", response.code);
            Assert.AreEqual("Spoiler", response.message);
            Assert.AreEqual(2, response.headers.size());
            Assert.IsTrue(response.headers.contains("Server"));
            Assert.AreEqual("StaticTcpServer", response.headers["Server"]);
            Assert.IsTrue(response.headers.contains("Content-Length"));
            Assert.AreEqual("0", response.headers["Content-Length"]);
        }

        [Test]
        [Ignore("Endpoint doesn't yet support HTTPS")]
        public void testRouteHttps()
        {
            // Endpoint doesn't yet support HTTPS
        }

        class DummyClientConnector : ClientConnector
        {
            public Response sendRequest(Request request, bool https, string host, int? port, RequestParams rparams)
            {
                // this custom connector sends the request and adds a single
                // custom header to the response

                Response response = (new BareClientConnector()).sendRequest(request, https, host, port, rparams);

                response.headers.add("CustomConnector", "true");

                return response;
            }
        }

        [Test]
        public void testRouteConnector()
        {

            given: //"set up the connector, route handler, and the request"

            var connector = new DummyClientConnector();

            var router = Handlers.Route("localhost", port, false, connector);
            Request request = new Request("METHOD", "/path/to/resource", "Name: Value", "this is the body");



            when: //"sending the request via the router"
            Response response = router(request, new HandlerContext());

            then: //"the request is served by the endpoint and handler"
            Assert.AreEqual("606", response.code);
            Assert.AreEqual("Spoiler", response.message);
            Assert.AreEqual("Snape Kills Dumbledore!", response.body);

//        and: "the response is modified by the connector"
            Assert.IsTrue(response.headers.contains("CustomConnector"));
            Assert.AreEqual("true", response.headers["CustomConnector"]);

        }

        [TearDown]
        public void TearDown()
        {

            if (deproxy != null)
            {
                deproxy.shutdown();
            }
            if (client != null)
            {
                client.Close();
            }
            if (server != null)
            {
                server.Close();
            }
            if (t != null)
            {
                t.Abort();
            }
        }
    }
}