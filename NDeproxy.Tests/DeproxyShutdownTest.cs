using NUnit.Framework;
using NDeproxy;
using System.Threading;

namespace NDeproxy.Tests
{
    [TestFixture]
    class DeproxyShutdownTest
    {
        Deproxy deproxy;

        [Test]
        void testShutdown()
        {

            /*
         *  When a Deproxy shuts down, all of its endpoints are shut down and
         *  removed, which means that the ports they were using should be available
         *  again.
         *
         */

            var port1 = PortFinder.Singleton.getNextOpenPort();
            var port2 = PortFinder.Singleton.getNextOpenPort();

            deproxy = new Deproxy();

            var e1 = deproxy.addEndpoint(port1);
            var e2 = deproxy.addEndpoint(port2);

            deproxy.shutdown();

            Thread.Sleep(1000);

            Endpoint e3 = null;
            Assert.DoesNotThrow(() => e3 = deproxy.addEndpoint(port1));
            Assert.IsNotNull(e3);

            Endpoint e4 = null;
            Assert.DoesNotThrow(() => e4 = deproxy.addEndpoint(port2));
            Assert.IsNotNull(e4);

        }

        [TearDown]
        void tearDown()
        {

            if (deproxy != null)
            {
                deproxy.shutdown();
            }
        }
    }
}