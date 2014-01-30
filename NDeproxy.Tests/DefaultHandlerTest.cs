using NUnit.Framework;
using NDeproxy;

namespace NDeproxy.Tests
{
    [TestFixture]
    public class DefaultHandlerTest
    {
        static readonly Logger log = new Logger();
        int _port;
        Deproxy _deproxy;
        Endpoint _endpoint;

        [SetUp]
        public void setUp()
        {
            log.debug("creating a deproxy");
            _deproxy = new Deproxy();

            log.debug("getting a port");
            _port = PortFinder.Singleton.getNextOpenPort();

            log.debug("creating the endpoint");
            _endpoint = _deproxy.addEndpoint(_port);

            log.debug("setUp done");
        }

        [Ignore]
        [Test]
        public void TestDefaultHandler()
        {
            log.debug("making request");
            var mc = _deproxy.makeRequest(string.Format("http://localhost:{0}/", _port));
            log.debug("making assertion");
            Assert.AreEqual(mc.receivedResponse.code, "200");
            log.debug("test done");
        }

        [TearDown]
        public void tearDown()
        {
            log.debug("shutting down");
            _deproxy.shutdown();
            log.debug("tearDown done");
        }
    }
}