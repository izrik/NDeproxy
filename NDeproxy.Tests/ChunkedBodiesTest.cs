using NUnit.Framework;
using System.Net.Sockets;
using NDeproxy;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System;
using System.IO;

namespace NDeproxy.Tests
{
    [TestFixture]
    public class ChunkedBodiesTest
    {
        static readonly Logger log = new Logger();
        Deproxy deproxy;
        Socket client;
        Socket server;
        string body;

        [SetUp]
        public void Setup()
        {
            body = " This is another body\n\nThis is the next paragraph.\n";
        }

        [Test]
        public void testChunkedRequestBodyInBareClientConnector()
        {


            string length = body.Length.ToString("X");

            string requestString = (string.Format("GET / HTTP/1.1\r\n" +
                               "Transfer-Encoding: chunked\r\n" +
                               "\r\n" +
                               "{0}\r\n" + // chunk-size, with no chunk-extension
                               "{1}\r\n" + // chunk-data
                               "0\r\n" + // last-chunk, with no chunk-extension
                               "\r\n", length, body)); // end of chunked body, no trailer

            string responseString = ("HTTP/1.1 200 OK\r\n" +
                                "Server: StaticTcpServer\r\n" +
                                "Content-Length: 0\r\n" +
                                "\r\n");

            var pair = LocalSocketPair.createLocalSocketPair();
            var client = pair.Item1;
            var server = pair.Item2;
            client.ReceiveTimeout = 5000;
            server.ReceiveTimeout = 5000;

            string serverSideRequest = null;

            var t = new Thread(() =>
            {
                //"static-tcp-server"
                serverSideRequest = StaticTcpServer.handleOneRequest(server, responseString,
                    requestString.Length);
            });
            t.Start();

            Request request = new Request("GET", "/", "Transfer-Encoding: chunked", body);
            RequestParams rparams = new RequestParams();
            rparams.usedChunkedTransferEncoding = true;

            BareClientConnector clientConnector = new BareClientConnector(client);

            Response response = clientConnector.sendRequest(request, false,
                                "localhost", ((IPEndPoint)server.LocalEndPoint).Port, rparams);



            Assert.AreEqual(requestString, serverSideRequest);
            Assert.AreEqual("200", response.code);
            Assert.AreEqual("OK", response.message);
            Assert.AreEqual(2, response.headers.size());
            Assert.IsTrue(response.headers.contains("Server"));
            Assert.AreEqual("StaticTcpServer", response.headers["Server"]);
            Assert.IsTrue(response.headers.contains("Content-Length"));
            Assert.AreEqual("0", response.headers["Content-Length"]);
            Assert.AreEqual("", response.body);
        }

        [Test]
        public void testChunkedRequestBodyInDefaultClientConnector()
        {


            string length = body.Length.ToString("X");

            var pair = LocalSocketPair.createLocalSocketPair();
            var client = pair.Item1;
            var server = pair.Item2;
            client.ReceiveTimeout = 5000;
            server.ReceiveTimeout = 5000;

            var port = (server.GetLocalPort() == 80 ? "" : (":" + server.GetLocalPort().ToString()));
            string requestString = (string.Format("GET / HTTP/1.1\r\n" +
                               "Transfer-Encoding: chunked\r\n" +
                               "Host: localhost{0}\r\n" +
                               "Accept: */*\r\n" +
                               "Accept-Encoding: identity\r\n" +
                               "User-Agent: {1}\r\n" +
                               "\r\n" +
                               "{2}\r\n" + // chunk-size, with no chunk-extension
                               "{3}\r\n" + // chunk-data
                               "0\r\n" + // last-chunk, with no chunk-extension
                               "\r\n", 
                                   port,
                                   Deproxy.VERSION_STRING,
                                   length,
                                   body)); // end of chunked body, no trailer

            string responseString = ("HTTP/1.1 200 OK\r\n" +
                                "Server: StaticTcpServer\r\n" +
                                "Content-Length: 0\r\n" +
                                "\r\n");

            string serverSideRequest = null;

            var t = new Thread(() =>
            {
                //"static-tcp-server"
                serverSideRequest = StaticTcpServer.handleOneRequest(server, responseString,
                    requestString.Length);
            });
            t.Start();

            Request request = new Request("GET", "/", new Dictionary<string, string>(), body);
            RequestParams rparams = new RequestParams();
            rparams.usedChunkedTransferEncoding = true;

            DefaultClientConnector clientConnector = new DefaultClientConnector(client);

            Response response = clientConnector.sendRequest(request, false,
                                "localhost", server.GetLocalPort(), rparams);



            Assert.AreEqual(requestString, serverSideRequest);
            Assert.AreEqual("200", response.code);
            Assert.AreEqual("OK", response.message);
            Assert.AreEqual(2, response.headers.size());
            Assert.IsTrue(response.headers.contains("Server"));
            Assert.AreEqual("StaticTcpServer", response.headers["Server"]);
            Assert.IsTrue(response.headers.contains("Content-Length"));
            Assert.AreEqual("0", response.headers["Content-Length"]);
            Assert.AreEqual("", response.body);
        }

        [Test]
        public void testChunkedRequestBodyInDeproxyEndpoint()
        {

            // setup - create canned request; setup deproxy and endpoint

            deproxy = new Deproxy();
            int port = PortFinder.Singleton.getNextOpenPort();
            string url = string.Format("http://localhost:{0}/", port);
            Endpoint endpoint = deproxy.addEndpoint(port);

            string length = body.Length.ToString("X");
            string requestString = (string.Format("GET / HTTP/1.1\r\n" +
                               "Host: localhost:{0}\r\n" +
                               "Content-Type: text/plain\r\n" +
                               "Transfer-Encoding: chunked\r\n" +
                               "Accept: */*\r\n" +
                               "Accept-Encoding: identity\r\n" +
                               "User-Agent: Canned-string\r\n" +
                               "\r\n" +
                               "{1}\r\n" + // chunk-size, with no chunk-extension
                               "{2}\r\n" + // chunk-data
                               "0\r\n" + // last-chunk, with no chunk-extension
                               "\r\n",
                                   port,
                                   length,
                                   body)); // end of chunked body, no trailer

            string responseString = ("HTTP/1.1 200 OK\r\n" +
                                "Server: StaticTcpServer\r\n" +
                                "Content-Length: 0\r\n" +
                                "\r\n");


            var ssc = (SocketServerConnector)endpoint.serverConnector;
            client = ssc.createRawConnection();
            client.ReceiveTimeout = 3000;

            MessageChain mc = null;
            var t = new Thread((x) =>
            //request-on-the-side-to-get-orphan
            mc = deproxy.makeRequest(url: url, defaultHandler: Handlers.Delay(2000),
                    addDefaultHeaders: true)
                );
            t.Start();

            var stream = new NetworkStream(client);
            var bytes = Encoding.ASCII.GetBytes(requestString);
            stream.Write(bytes, 0, bytes.Length);


            t.Join();


            Assert.AreEqual(1, mc.orphanedHandlings.Count);
            Assert.AreEqual(body, mc.orphanedHandlings[0].request.body);

        }

        [Test]
        public void testChunkedResponseBodyInBareClientConnector()
        {


            string length = body.Length.ToString("X");

            string responseString = (string.Format("HTTP/1.1 200 OK\r\n" +
                                "Server: StaticTcpServer\r\n" +
                                "Transfer-Encoding: chunked\r\n" +
                                "\r\n" +
                                "{0}\r\n" + // chunk-size, with no chunk-extension
                                "{1}\r\n" + // chunk-data
                                "0\r\n" + // last-chunk, with no chunk-extension
                                "\r\n",  // end of chunked body, no trailer
                                    length,
                                    body
                                ));

            var pair = LocalSocketPair.createLocalSocketPair();
            var client = pair.Item1;
            var server = pair.Item2;
            client.ReceiveTimeout = 5000;
            server.ReceiveTimeout = 5000;

            string serverSideRequest;

            var t = new Thread(x =>
            {
                //"static-tcp-server"
                serverSideRequest = StaticTcpServer.handleOneRequest(server,
                    responseString, 1);
            });
            t.Start();

            Request request = new Request("GET", "/",
                              "Transfer-Encoding: chunked", body);

            BareClientConnector clientConnector = new BareClientConnector(client);



            Response response = clientConnector.sendRequest(request, false,
                                "localhost", server.GetLocalPort(), new RequestParams());



            Assert.AreEqual("200", response.code);
            Assert.AreEqual("OK", response.message);
            Assert.AreEqual(2, response.headers.size());
            Assert.IsTrue(response.headers.contains("Server"));
            Assert.AreEqual("StaticTcpServer", response.headers["Server"]);
            Assert.IsTrue(response.headers.contains("Transfer-Encoding"));
            Assert.AreEqual("chunked", response.headers["Transfer-Encoding"]);
            Assert.AreEqual(body, response.body);
        }

        [Test]
        public void testChunkedResponseBodyInDefaultClientConnector()
        {


            string length = body.Length.ToString("X");

            string responseString = (string.Format("HTTP/1.1 200 OK\r\n" +
                                "Server: StaticTcpServer\r\n" +
                                "Transfer-Encoding: chunked\r\n" +
                                "\r\n" +
                                "{0}\r\n" + // chunk-size, with no chunk-extension
                                "{1}\r\n" + // chunk-data
                                "0\r\n" + // last-chunk, with no chunk-extension
                                "\r\n",  // end of chunked body, no trailer
                                    length, body
                                ));

            var pair = LocalSocketPair.createLocalSocketPair();
            var client = pair.Item1;
            var server = pair.Item2;
            client.ReceiveTimeout = 5000;
            server.ReceiveTimeout = 5000;

            string serverSideRequest;

            var t = new Thread(() =>
            {
                //"static-tcp-server"
                serverSideRequest = StaticTcpServer.handleOneRequest(server,
                    responseString, 1);
            });
            t.Start();

            Request request = new Request("GET", "/",
                              "Transfer-Encoding: chunked", body);

            DefaultClientConnector clientConnector = new DefaultClientConnector(client);



            Response response = clientConnector.sendRequest(request, false,
                                "localhost", (server.LocalEndPoint as IPEndPoint).Port, new RequestParams());



            Assert.AreEqual("200", response.code);
            Assert.AreEqual("OK", response.message);
            Assert.AreEqual(2, response.headers.size());
            Assert.IsTrue(response.headers.contains("Server"));
            Assert.AreEqual("StaticTcpServer", response.headers["Server"]);
            Assert.IsTrue(response.headers.contains("Transfer-Encoding"));
            Assert.AreEqual("chunked", response.headers["Transfer-Encoding"]);
            Assert.AreEqual(body, response.body);
        }

        [Test]
        public void testChunkedResponseBodyInDeproxyEndpoint()
        {

            // create canned request & response

            string length = body.Length.ToString("X");
            string requestString = ("GET / HTTP/1.1\r\n" +
                               "Host: localhost\r\n" +
                               "Content-Length: 0\r\n" +
                               "Accept: */*\r\n" +
                               "User-Agent: Canned-string\r\n" +
                               "\r\n");

            string responseString = (string.Format("HTTP/1.1 200 OK\r\n" +
                                "Server: {0}\r\n" +
                                "Date: EEE, dd MMM yyyy HH:mm:ss zzz\r\n" +
                                "Transfer-Encoding: chunked\r\n" +
                                "\r\n" +
                                "{1}\r\n" + // chunk-size, with no chunk-extension
                                "{2}\r\n" + // chunk-data
                                "0\r\n" + // last-chunk, with no chunk-extension
                                "\r\n",
                                    Deproxy.VERSION_STRING,
                                    length,
                                    body // end of chunked body, no trailer
                                ));
            byte[] responseBytes = Encoding.ASCII.GetBytes(responseString);

            // setup deproxy and endpoint

            deproxy = new Deproxy();
            int port = PortFinder.Singleton.getNextOpenPort();
            string url = string.Format("http://localhost:{0}/", port);
            HandlerWithContext handler = (Request request, HandlerContext context) =>
            {
                context.usedChunkedTransferEncoding = true;
                return new Response(200, "OK", null, body);
            };
            Endpoint endpoint = deproxy.addEndpoint(port: port, defaultHandler: handler);

            // create raw connection

            var ssc = (SocketServerConnector)endpoint.serverConnector;
            client = ssc.createRawConnection();
            client.ReceiveTimeout = 3000;

            // send the data to the endpoint

            var stream = new NetworkStream(client);
            var bytes = Encoding.ASCII.GetBytes(requestString);
            stream.Write(bytes, 0, bytes.Length);

            // read the response

            byte[] bytesRecieved = new byte[responseBytes.Length];
            try
            {

                int n = 0;
                while (n < bytesRecieved.Length)
                {
                    var count = stream.Read(bytesRecieved, n, bytesRecieved.Length - n);
                    n += count;
                }

            }
//        catch (SocketTimeoutException ignored) {
//
//        }
        catch (Exception ignored)
            {
                throw;
            }
            string stringRecieved = Encoding.ASCII.GetString(bytesRecieved);




            // compare the expected and actual responses. They are only partially
            // identical, though

            var expected = new StringReader(responseString);
            var actual = new StringReader(stringRecieved);

            // response lines should be the same
            string expectedLine = LineReader.readLine(expected);
            string actualLine = LineReader.readLine(actual);
            Assert.AreEqual(expectedLine, actualLine);

            // server headers have potentially variable length
            expectedLine = LineReader.readLine(expected);
            actualLine = LineReader.readLine(actual);
            Assert.IsTrue(expectedLine.StartsWith("Server: "));
            Assert.IsTrue(actualLine.StartsWith("Server: "));

            // date headers will be different in content, but equal in length
            expectedLine = LineReader.readLine(expected);
            actualLine = LineReader.readLine(actual);
            Assert.IsTrue(expectedLine.StartsWith("Date: "));
            Assert.IsTrue(actualLine.StartsWith("Date: "));
            Assert.AreEqual(expectedLine.Length, actualLine.Length);

            // the rest should be identical
            expectedLine = LineReader.readLine(expected);
            actualLine = LineReader.readLine(actual);
            while (expectedLine != null && actualLine != null)
            {
                Assert.AreEqual(expectedLine, actualLine);
                expectedLine = LineReader.readLine(expected);
                actualLine = LineReader.readLine(actual);
            }
        }

        [Test]
        public void testChunkedBodyInBodyWriter1()
        {

            string length = body.Length.ToString("X");
            var cbody = (string.Format(
                        "{0}\r\n" + // chunk-size, with no chunk-extension
                        "{1}\r\n" + // chunk-data
                        "0\r\n" + // last-chunk, with no chunk-extension
                        "\r\n", // end of chunked body, no trailer
                        length,
                        body
                    ));
            var chunkedBody = Encoding.ASCII.GetBytes(cbody);
            var outStream = new MemoryStream(chunkedBody.Length);


            BodyWriter.writeBodyChunked(body, outStream);
            byte[] bytesWritten = outStream.ToArray();


            Assert.AreEqual(chunkedBody, bytesWritten);
        }

        [Test]
        public void testChunkedBodyInBodyWriter2()
        {

            string length = body.Length.ToString("X");
            var cbody = (string.Format(
                        "{0}\r\n" + // chunk-size, with no chunk-extension
                        "{1}\r\n" + // chunk-data
                        "0\r\n" + // last-chunk, with no chunk-extension
                        "\r\n", // end of chunked body, no trailer
                        length,
                        body
                    ));
            byte[] chunkedBody = Encoding.ASCII.GetBytes(cbody);
            var outStream = new MemoryStream(chunkedBody.Length);


            BodyWriter.writeBody(body, outStream, true);
            byte[] bytesWritten = outStream.ToArray();


            Assert.AreEqual(chunkedBody, bytesWritten);
        }

        [Test]
        public void testChunkedBodyInBodyReader1()
        {

            string length = body.Length.ToString("X");
            var cbody = string.Format(
                        "{0}\r\n" + // chunk-size, with no chunk-extension
                        "{1}\r\n" + // chunk-data
                        "0\r\n" + // last-chunk, with no chunk-extension
                        "\r\n", // end of chunked body, no trailer
                        length,
                        body);
            byte[] chunkedBody = Encoding.ASCII.GetBytes(cbody);
            var inStream = new MemoryStream(chunkedBody);


            byte[] bytesRead = BodyReader.readChunkedBody(inStream);


            Assert.AreEqual(Encoding.ASCII.GetBytes(body), bytesRead);
        }

        [Test]
        public void testChunkedBodyInBodyReader2()
        {

            string length = body.Length.ToString("X");
            var cbody = (string.Format(
                        "{0}\r\n" + // chunk-size, with no chunk-extension
                        "{1}\r\n" + // chunk-data
                        "0\r\n" + // last-chunk, with no chunk-extension
                        "\r\n", // end of chunked body, no trailer
                        length,
                        body
                    ));
            byte[] chunkedBody = Encoding.ASCII.GetBytes(cbody);
            var inStream = new MemoryStream(chunkedBody);
            HeaderCollection headers = new HeaderCollection("Transfer-Encoding: chunked");


            byte[] bytesRead = (byte[])BodyReader.readBody(inStream, headers);


            Assert.AreEqual(Encoding.ASCII.GetBytes(body), bytesRead);
        }

        [TearDown]
        public void tearDown()
        {
            if (this.deproxy != null)
            {
                this.deproxy.shutdown();
            }

            if (client != null)
            {
                client.Close();
                client = null;
            }

            if (server != null)
            {
                server.Close();
                server = null;
            }
        }
    }
}