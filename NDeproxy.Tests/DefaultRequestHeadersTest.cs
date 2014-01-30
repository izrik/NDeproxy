using NUnit.Framework;
using NDeproxy;

namespace NDeproxy.Tests
{
    [TestFixture]
    class DefaultRequestHeadersTest
    {
        int _port;
        Deproxy _deproxy;
        Endpoint _endpoint;
        string _url;

        [SetUp]
        void setUp()
        {
            _port = PortFinder.Singleton.getNextOpenPort();
            _deproxy = new Deproxy();
            _endpoint = _deproxy.addEndpoint(_port);
            _url = string.Format("http://localhost:{0}/", _port);
        }

        [Test]
        void testNotSpecified()
        {
            var mc = _deproxy.makeRequest(_url);
            Assert.IsTrue(mc.sentRequest.headers.contains("Host"));
            Assert.IsTrue(mc.sentRequest.headers.contains("Accept"));
            Assert.IsTrue(mc.sentRequest.headers.contains("Accept-Encoding"));
            Assert.IsTrue(mc.sentRequest.headers.contains("User-Agent"));
        }

        [Test]
        void testExplicitOn()
        {
            var mc = _deproxy.makeRequest(url: _url, addDefaultHeaders: true);
            Assert.IsTrue(mc.sentRequest.headers.contains("Host"));
            Assert.IsTrue(mc.sentRequest.headers.contains("Accept"));
            Assert.IsTrue(mc.sentRequest.headers.contains("Accept-Encoding"));
            Assert.IsTrue(mc.sentRequest.headers.contains("User-Agent"));
        }

        [Test]
        void testExplicitOff()
        {
            var mc = _deproxy.makeRequest(url: _url, addDefaultHeaders: false);
            Assert.IsFalse(mc.sentRequest.headers.contains("Host"), "host header exists");
            Assert.IsFalse(mc.sentRequest.headers.contains("Accept"), "accept header exists");
            Assert.IsFalse(mc.sentRequest.headers.contains("Accept-Encoding"), "accept encoding exists");
            Assert.IsFalse(mc.sentRequest.headers.contains("User-Agent"), "user agent exists");
        }

        [TearDown]
        void tearDown()
        {
            if (_deproxy != null)
            {
                _deproxy.shutdown();
            }
        }
    }
}