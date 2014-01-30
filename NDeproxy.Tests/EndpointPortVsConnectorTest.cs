using NUnit.Framework;
using NDeproxy;

namespace NDeproxy.Tests
{
    [TestFixture]
    class EndpointPortVsConnectorTest
    {
        Deproxy deproxy;

        [SetUp]
        public void setup()
        {
            deproxy = new Deproxy();
        }

        [Test]
        public void whenInstantiatingWith_port_ButNo_connectorFactory_CreateANew_SocketServerConnector()
        {

            given:
            int port = PortFinder.Singleton.getNextOpenPort();

            when:
            var endpoint = new Endpoint(deproxy, port);

            then:
            Assert.IsInstanceOf<SocketServerConnector>(endpoint.serverConnector);
            SocketServerConnector ssc = (SocketServerConnector)(endpoint.serverConnector);
            Assert.AreEqual(port, ssc.port);
            Assert.AreSame(endpoint, ssc.endpoint);
            Assert.AreEqual(endpoint.name, ssc.name);
        }

        class DummyServerConnector : ServerConnector
        {
            public void shutdown()
            {
            }
        }

        [Test]
        public void whenInstantiatingWithNo_port_But_connectorFactory_UseTheFactory()
        {

            given:
            var connector = new DummyServerConnector();
            ServerConnectorFactory factory = (e, n) => connector;

            when:
            var endpoint = new Endpoint(deproxy, connectorFactory: factory);

            then:
            Assert.AreSame(connector, endpoint.serverConnector);
        }

        [Test]
        public void whenInstantiatingWith_port_And_connectorFactory_IgnoreThePortAndUseTheFactory()
        {

            given:
            var connector = new DummyServerConnector();
            ServerConnectorFactory factory = (e, n) => connector;
            int port = PortFinder.Singleton.getNextOpenPort();

            when:
            var endpoint = new Endpoint(deproxy, port: port, connectorFactory: factory);

            then:
            Assert.AreSame(connector, endpoint.serverConnector);
        }

        [Test]
        public void whenInstantiatingWithNeither_port_Nor_factory_GrabAnOpenPortAndCreateANew_SocketServerConnector()
        {

            when:
            var endpoint = new Endpoint(deproxy);

            then:
            Assert.IsInstanceOf<SocketServerConnector>(endpoint.serverConnector);

            SocketServerConnector ssc = (SocketServerConnector)(endpoint.serverConnector);
            // this next assertion is possibly flaky if PortFinder.Singleton.getNextOpenPort()
            // gets called somewhere else between "new Endpoint(deproxy)" and here
            Assert.AreEqual(PortFinder.Singleton.currentPort - 1, ssc.port);
            Assert.AreSame(endpoint, ssc.endpoint);
            Assert.AreEqual(endpoint.name, ssc.name);
        }

        [Test]
        public void whenCalling_addEndpoint_With_port_ButNo_connectorFactory_CreateANew_SocketServerConnector()
        {

            given:
            int port = PortFinder.Singleton.getNextOpenPort();

            when:
            var endpoint = deproxy.addEndpoint(port);

            then:
            Assert.IsInstanceOf<SocketServerConnector>(endpoint.serverConnector);
            SocketServerConnector ssc = (SocketServerConnector)(endpoint.serverConnector);
            Assert.AreEqual(port, ssc.port);
            Assert.AreSame(endpoint, ssc.endpoint);
            Assert.AreEqual(endpoint.name, ssc.name);
        }

        [Test]
        public void whenCalling_addEndpoint_WithNo_port_But_connectorFactory_UseTheFactory()
        {

            given:
            var connector = new DummyServerConnector();
            ServerConnectorFactory factory = (e, n) => connector;

            when:
            var endpoint = deproxy.addEndpoint(connectorFactory: factory);

            then:
            Assert.AreSame(connector, endpoint.serverConnector);
        }

        [Test]
        public void whenCalling_addEndpoint_With_port_And_connectorFactory_IgnoreThePortAndUseTheFactory()
        {

            given:
            var connector = new DummyServerConnector();
            ServerConnectorFactory factory = (e, n) => connector;
            int port = PortFinder.Singleton.getNextOpenPort();

            when:
            var endpoint = deproxy.addEndpoint(port: port, connectorFactory: factory);

            then:
            Assert.AreSame(connector, endpoint.serverConnector);

        }

        [Test]
        public void whenCalling_addEndpoint_WithNeither_port_Nor_Factory_GrabAnOpenPortAndCreateANew_SocketServerConnector()
        {

            when:
            var endpoint = deproxy.addEndpoint();

            then:
            Assert.IsInstanceOf<SocketServerConnector>(endpoint.serverConnector);
            SocketServerConnector ssc = (SocketServerConnector)(endpoint.serverConnector);
            // this next assertion is possibly flaky if PortFinder.Singleton.getNextOpenPort()
            // gets called somewhere else between "new Endpoint(deproxy)" and here
            Assert.AreEqual(PortFinder.Singleton.currentPort - 1, ssc.port);
            Assert.AreSame(endpoint, ssc.endpoint);
            Assert.AreEqual(endpoint.name, ssc.name);
        }

        [TearDown]
        public void TearDown()
        {

            if (deproxy != null)
            {
                deproxy.shutdown();
            }
        }
    }
}