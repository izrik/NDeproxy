using NUnit.Framework;
using NDeproxy;
using System.Threading;

namespace NDeproxy.Tests
{
    [TestFixture]
    class OrphanedHandlingsTest
    {
        Deproxy deproxy;
        Endpoint endpoint;
        int port;
        // this just acts as another HTTP client to make requests with
        Deproxy otherClient;

        [SetUp]
        public void setup()
        {
            this.deproxy = new Deproxy();
            this.port = PortFinder.Singleton.getNextOpenPort();
            this.endpoint = this.deproxy.addEndpoint(port);
            this.otherClient = new Deproxy();
        }

        [Test]
        public void testOrphanedHandlings()
        {

            var handler = Handlers.Delay(2000);

            MessageChain mc = null;
            var t = new Thread(x =>
            {
                mc = this.deproxy.makeRequest(url: string.Format("http://localhost:{0}/", this.port),
                    defaultHandler: handler);
            });

            // the first request will take a few seconds to finish. during that
            // time, we'll make another request to the same endpoint from another
            // client. because it won't have a record of the Deproxy-Request-ID,
            // the other client's request will be orphaned from the perspective
            // of the first deproxy.

            MessageChain otherClientMc;
            otherClientMc = this.otherClient.makeRequest(
                string.Format("http://localhost:{0}/", this.port));

            t.Join();

            Assert.AreEqual(1, mc.orphanedHandlings.Count);
            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual(1, mc.orphanedHandlings[0].request.headers.getCountByName(Deproxy.REQUEST_ID_HEADER_NAME));
            Assert.AreEqual(0, otherClientMc.handlings.Count);
            Assert.AreEqual(0, otherClientMc.orphanedHandlings.Count);
            Assert.AreEqual(1, otherClientMc.sentRequest.headers.getCountByName(Deproxy.REQUEST_ID_HEADER_NAME));
            Assert.AreEqual(mc.orphanedHandlings[0].request.headers[Deproxy.REQUEST_ID_HEADER_NAME],
                otherClientMc.sentRequest.headers[Deproxy.REQUEST_ID_HEADER_NAME]);
        }

        [TearDown]
        public void cleanup()
        {

            if (this.deproxy != null)
            {
                this.deproxy.shutdown();
            }

            if (this.otherClient != null)
            {
                this.otherClient.shutdown();
            }
        }
    }
}