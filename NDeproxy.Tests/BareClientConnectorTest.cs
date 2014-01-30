using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace NDeproxy.Tests
{
    [TestFixture]
    class BareClientConnectorTest
    {
        static readonly Logger log = new Logger();
        string requestString = ("GET / HTTP/1.1\r\n" +
                               "Content-Length: 0\r\n" +
                               "\r\n");
        string responseString = ("HTTP/1.1 200 OK\r\n" +
                                "Server: some-closure\r\n" +
                                "Content-Length: 0\r\n" +
                                "\r\n");

        [Test]
        public void testConstructorWithSocketParameter()
        {

//            var pair = LocalSocketPair.createLocalSocketPair();
//            var client = pair.Item1;
//            var server = pair.Item2;
//
//            client.ReceiveTimeout = 30000; // in milliseconds
//                           server.ReceiveTimeout = 30000;
//
//        string serverSideRequest;
//
//            var t = Thread.startDaemon("response"); x=>{
//
//            // TODO: replace this with StaticTcpServer
//            byte[] bytes = new byte[requestString.length()]
//            int n = 0;
//            while (n < bytes.length) {
//                var count = server.inputStream.read(bytes, n, bytes.length - n)
//                n += count
//            }
//            ByteBuffer bb = ByteBuffer.wrap(bytes)
//            CharBuffer cb = Charset.forName("US-ASCII").decode(bb)
//            serverSideRequest = cb.toString()
//
//
//            bytes = new byte[responseString.length()]
//            Charset.forName("US-ASCII").encode(responseString).get(bytes)
//            server.outputStream.write(bytes)
//            server.outputStream.flush()
//        }
//            ThreadPool;
//
//        BareClientConnector clientConnector = new BareClientConnector(client)
//        Request request = new Request("GET", "/", ["Content-Length": "0"])
//        RequestParams params = new RequestParams()
//        params.sendDefaultRequestHeaders = false
//
//        Response response = clientConnector.sendRequest(request, false, "localhost", server.port, params)
//
//
//
//        Assert.AreEqual(requestString, serverSideRequest)
//        Assert.AreEqual("200", response.code)
//        Assert.AreEqual("OK", response.message)
//        Assert.AreEqual(2, response.headers.size())
//        Assert.IsTrue(response.headers.contains("Server"))
//        Assert.AreEqual("some-closure", response.headers["Server"])
//        Assert.IsTrue(response.headers.contains("Content-Length"))
//        Assert.AreEqual("0", response.headers["Content-Length"])
//        Assert.AreEqual("", response.body)
//
//        client.close()
//        server.close()
        }
    }
}