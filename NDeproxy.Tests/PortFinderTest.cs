using System;
using NUnit.Framework;
using NDeproxy;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Net.Sockets;

namespace NDeproxy.Tests
{
    [TestFixture]
    class PortFinderTest
    {
        PortFinder pf;
        Accepter accepter;

        [SetUp]
        public void setup()
        {
            pf = new PortFinder();
        }

        [Test]
        public void whenAPortNumberIsNotInUseReturnIt()
        {

            when:
            int port = pf.getNextOpenPort(12345);

            then:
            Assert.AreEqual(12345, port);
        }

        class Accepter : IDisposable
        {
            Thread _th;
            Socket _socket;
            ManualResetEvent _stopSignal = new ManualResetEvent(true);

            public Accepter(Socket socket)
            {
                _socket = socket;
                _th = new Thread(x =>
                {
                    while (_stopSignal.WaitOne())
                    {
                        _socket.Accept();
                    }
                });
                _th.Start();
            }

            public void Dispose()
            {
                _stopSignal.Reset();
                _th.Join(1000);
                if (_th.IsAlive)
                {
                    _th.Abort();
                }
                _socket.Close();
            }
        }

        [Test]
        public void whenAPortNumberIsAlreadyInUseReturnTheNextPortNumber()
        {

            given:
            // create the listener socket
            var listener = SocketHelper.Server(12345);
            accepter = new Accepter(listener);

            when:
            int port = pf.getNextOpenPort(12345);

            then:
            Assert.AreEqual(12346, port);
        }

        [Test]
        public void whenAPortNumberIsAlreadyInUseIncrementTheSkipsCount()
        {

            given:
            // create the listener socket
            var listener = SocketHelper.Server(23456);
            accepter = new Accepter(listener);

            when:
            int port = pf.getNextOpenPort(23456);

            then:
            Assert.AreEqual(23457, port);
            Assert.AreEqual(1, pf.skips);
        }

        [Test]
        public void whenInstantiatingWithoutParameterShouldHaveTheDefault()
        {

            when:
            var pf2 = new PortFinder();

            then:
            Assert.AreEqual(10000, pf2.currentPort);
        }

        [Test]
        public void whenInstantiatingWithParameterShouldHaveTheGivenValue()
        {

            when:
            var pf2 = new PortFinder(23456);

            then:
            Assert.AreEqual(23456, pf2.currentPort);
        }

        [Test]
        public void whenUsingTheSingletonAmongMultipleThreadsShouldBeThreadSafe()
        {

            var prevCurrentPort = PortFinder.Singleton.currentPort;

            var threads = new List<Thread>();
            var startSignal = new ManualResetEvent(false);
            var listLock = new object();
            var ports = new List<int>();

            int i;
            for (i = 0; i < 100; i++)
            {
                threads.Add(new Thread(x =>
                {
                    startSignal.WaitOne(1000);
                    int port = PortFinder.Singleton.getNextOpenPort();
                    lock (listLock)
                    {
                        ports.Add(port);
                    }
                }));
            }

            startSignal.Set();

            foreach (var th in threads)
            {
                th.Join();
            }

            expect:
            Assert.AreEqual(prevCurrentPort + threads.Count + PortFinder.Singleton.skips, PortFinder.Singleton.currentPort);
            Assert.AreEqual(threads.Count, ports.Distinct().Count());
        }

        [TearDown]
        public void TearDown()
        {
            if (accepter != null)
            {
                accepter.Dispose();
            }
        }
    }
}