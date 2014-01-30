using NUnit.Framework;
using NDeproxy;

namespace NDeproxy.Tests
{
    [TestFixture]
    class QueryParametersTest
    {
        Deproxy deproxy;
        int port;
        string url;

        [SetUp]
        public void SetUp()
        {

            this.deproxy = new Deproxy();
            this.port = PortFinder.Singleton.getNextOpenPort();
            this.url = string.Format("http://localhost:{0}/", this.port);
            this.deproxy.addEndpoint(this.port);
        }

        [Test]
        public void TestIncludesQueryParamsInRequestImplicitDefaultConnector()
        {

            var mc = this.deproxy.makeRequest(url: url + "?a=b&c=d");

            Assert.AreEqual("/?a=b&c=d", mc.sentRequest.path);
            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual("/?a=b&c=d", mc.handlings[0].request.path);
        }

        [Test]
        public void TestIncludesQueryParamsInRequestExplicitDefaultConnector()
        {

            var mc = this.deproxy.makeRequest(url: url + "?a=b&c=d", clientConnector: new DefaultClientConnector());

            Assert.AreEqual("/?a=b&c=d", mc.sentRequest.path);
            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual("/?a=b&c=d", mc.handlings[0].request.path);
        }

        [Test]
        public void TestIncludesQueryParamsInRequestBareConnector()
        {

            var mc = this.deproxy.makeRequest(url: url + "?a=b&c=d", clientConnector: new BareClientConnector());

            Assert.AreEqual("/?a=b&c=d", mc.sentRequest.path);
            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual("/?a=b&c=d", mc.handlings[0].request.path);
        }
        // TODO: Test for non-ascii characters, illegal characters, escaping, etc.
        [TearDown]
        public void TearDown()
        {
            if (this.deproxy != null)
            {
                this.deproxy.shutdown();
            }
        }
    }
}
