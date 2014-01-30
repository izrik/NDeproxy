using NUnit.Framework;
using NDeproxy;

namespace NDeproxy.Tests
{
    [TestFixture]
    class DefaultHandlerTest2
    {
        [SetUp]
        public void Setup()
        {
            deproxyPort = PortFinder.Singleton.getNextOpenPort();
            deproxy = new Deproxy();
            endpoint = deproxy.addEndpoint(deproxyPort);
        }

        int deproxyPort;
        Deproxy deproxy;
        Endpoint endpoint;

        [Test]
        public void _DefaultHandlerTest2()
        {

            var url = string.Format("http://localhost:{0}", deproxyPort);
            MessageChain mc = deproxy.makeRequest(url);

            Assert.AreEqual("200", mc.receivedResponse.code);
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