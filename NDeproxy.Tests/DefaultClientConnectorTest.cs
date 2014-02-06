using System.Net.Sockets;
using NUnit.Framework;
using NDeproxy;
using System.Net;

namespace NDeproxy.Tests
{
    [TestFixture]
    class DefaultClientConnectorTest
    {
        Socket client;
        Socket server;

        [Test]
        public void testConstructorWithSocketParameter()
        {

            given: //"a client socket and a server socket"
            var pair = LocalSocketPair.createLocalSocketPair();
            client = pair.Item1;
            server = pair.Item2;


            client.ReceiveTimeout = 100; // in milliseconds
            server.ReceiveTimeout = 2000;

            and: //"a DefaultClientConnector using the provided client socket"
            DefaultClientConnector clientConnector = new DefaultClientConnector(client);

//        and: "a simple request"
            Request request = new Request("GET", "/", "Content-Length: 0");

//        and: "request params that don't involve adding default headers"
            var rparams = new RequestParams { sendDefaultRequestHeaders = false };


            when: //"we send the request through the connector"
            try
            {

                Response response = clientConnector.sendRequest(request, false, "localhost", (server.LocalEndPoint as IPEndPoint).Port, rparams);

            }
            catch (SocketException ignored)
            {
                // read times out, as expected
                // check the SocketErrorCode property for time out
                throw;
            }

//        and: "read the request that the connector sent from the server-side socket"
            var serverStream = new UnbufferedSocketStream(server);
            string requestLine = LineReader.readLine(serverStream);
            HeaderCollection headers = HeaderCollection.fromStream(serverStream);
            string body = (string)BodyReader.readBody(serverStream, headers);


//        then: "it formats the request correctly and only has the header we specified"

            Assert.AreEqual(requestLine, "GET / HTTP/1.1");
            Assert.AreEqual(headers.size(), 1);
            Assert.IsTrue(headers.contains("Content-Length"));
            Assert.AreEqual(headers["Content-Length"], "0");
            Assert.IsTrue(string.IsNullOrEmpty(body));
        }

        [TestCase("localhost", 80, true, "localhost:80")]       
        [TestCase("localhost", 80, false, "localhost")]
        [TestCase("localhost", 443, true, "localhost")]
        [TestCase("localhost", 443, false, "localhost:443")]
        [TestCase("localhost", 12345, true, "localhost:12345")]
        [TestCase("localhost", 12345, false, "localhost:12345")]
        [TestCase("example.com", 80, true, "example.com:80")]
        [TestCase("example.com", 80, false, "example.com")]
        [TestCase("example.com", 443, true, "example.com")]
        [TestCase("example.com", 443, false, "example.com:443")]
        [TestCase("example.com", 12345, true, "example.com:12345")]
        [TestCase("example.com", 12345, false, "example.com:12345")]
        [TestCase("12.34.56.78", 80, true, "12.34.56.78:80")]
        [TestCase("12.34.56.78", 80, false, "12.34.56.78")]
        [TestCase("12.34.56.78", 443, true, "12.34.56.78")]
        [TestCase("12.34.56.78", 443, false, "12.34.56.78:443")]
        [TestCase("12.34.56.78", 12345, true, "12.34.56.78:12345")]
        [TestCase("12.34.56.78", 12345, false, "12.34.56.78:12345")]
        //        @Unroll("when we call sendRequest with https=#https, #host, and #port, we should get Host: #expectedValue")
        public void testHostHeader(string host, int port, bool https, string expectedValue)
        {

//        given: "a client socket and a server socket"
            var pair = LocalSocketPair.createLocalSocketPair();
            client = pair.Item1;
            server = pair.Item2;

            client.ReceiveTimeout = 100; // in milliseconds
            server.ReceiveTimeout = 2000;

//        and: "a DefaultClientConnector using the provided client socket"
            DefaultClientConnector clientConnector = new DefaultClientConnector(client);

//        and: "a simple request and basic request params"
            Request request = new Request("GET", "/");
            var rparams = new RequestParams { sendDefaultRequestHeaders = true };



//        when: "we send the request through the connector"
            try
            {

                // we're explicitly setting the https, host, and port parameters.
                // the connector was created with a client socket, however. it
                // will use the socket instead of trying to open a new connection,
                // and just use the parameters for the Host header.
                Response response = clientConnector.sendRequest(request, https, host, port, rparams);

            }
            catch (SocketException ignored)
            {

                // we're expecting the connector to send the request, and then
                // wait for a server response. since there is no server in this
                // case, it will timeout while waiting. then we just read the
                // request from the server side of the socket.

                //check SocketErrorCode property
                throw;
            }

//        and: "read the request that the connector sent from the server-side socket"
            var serverStream = new UnbufferedSocketStream(server);
            string requestLine = LineReader.readLine(serverStream);
            HeaderCollection headers = HeaderCollection.fromStream(serverStream);



//        then: "it formats the request correctly and has a Host header with the right value"
            Assert.AreEqual(requestLine, "GET / HTTP/1.1");
            Assert.IsTrue(headers.contains("Host"));
            Assert.AreEqual(expectedValue, headers["Host"]);



        }

        [TearDown]
        public void Teardown()
        {
            if (client != null)
            {
                client.Close();
            }
            if (server != null)
            {
                server.Close();
            }
        }
    }
}