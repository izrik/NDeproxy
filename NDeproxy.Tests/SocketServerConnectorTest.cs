using NUnit.Framework;
using System;
using System.Threading;
using System.Net.Sockets;

namespace NDeproxy.Tests
{
    class DummyServerConnector : ServerConnector
    {
        public void shutdown()
        {
        }
    }

    [TestFixture]
    public class SocketServerConnectorTest
    {
        static readonly Logger log = new Logger("SocketServerConnectorTest");

        [Test]
        public void Test()
        {
            var deproxy = new Deproxy();
            var capturedRequest = new Request("GET", "/");
            var endpoint = new Endpoint(
                               deproxy, 
                               defaultHandler: x =>
                {
                    capturedRequest = x;
                    return new Response(200);
                },
                               connectorFactory: (e, n) => new DummyServerConnector());

            int port = PortFinder.Singleton.getNextOpenPort();
            var ssc = new SocketServerConnector(endpoint, endpoint.name, port);
        }

//        [Test]
//        public void ListenerThreadTest()
//        {
//            var port = PortFinder.Singleton.getNextOpenPort();
//            var socket = SocketHelper.Server(port);
//            var listener = new SocketServerConnector.ListenerThread(null, socket, "name");
//
//            Thread.Sleep(100);
//
//            Assert.IsNull(listener.parent);
//            Assert.AreSame(socket, listener.socket);
//            Assert.IsTrue(listener.thread.IsAlive);
//            Assert.AreEqual(ThreadState.Background, listener.thread.ThreadState);
//            Assert.IsTrue(listener.thread.IsBackground);
//            Assert.AreEqual("name", listener.thread.Name);
//        }

//        [Test]
//        public void HandlerThreadTest()
//        {
//            given:
//            ManualResetEvent mre = new ManualResetEvent(false);
//            var pair = LocalSocketPair.createLocalSocketPair();
//            var client = pair.Item1;
//            var server = pair.Item2;
//            bool processed = false;
//            SocketServerConnector.ConnectionProcessor processor = (s, n) =>
//            {
//                processed = true;
//                mre.WaitOne();
//            };
//
//            try
//            {
//                when:
//                var handlerThread = new SocketServerConnector.HandlerThread(
//                                    processor,
//                                    server,
//                                    "connection-name",
//                                    "thread-name");
//
//                then:
//                Assert.IsTrue(handlerThread.thread.IsAlive);
//                Assert.AreEqual("thread-name", handlerThread.thread.Name);
//                Assert.IsTrue(handlerThread.thread.IsBackground);
//                Assert.AreSame(server, handlerThread.socket);
//                Assert.AreSame(processor, handlerThread.processor);
//                Assert.AreEqual("connection-name", handlerThread.connectionName);
//                Assert.IsTrue(processed);
//            }
//            finally
//            {
//                cleanup:
//                mre.Set();
//            }
//        }
    }
}

