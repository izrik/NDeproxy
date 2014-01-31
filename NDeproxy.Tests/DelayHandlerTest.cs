using NDeproxy;
using NUnit.Framework;
using System;

namespace NDeproxy.Tests
{
    [TestFixture]
    class DelayHandlerTest
    {
        [Test]
        public void testDelayHandler()
        {
            var handler = Handlers.Delay(1000);

            Request request = new Request("GET", "/");

            var t1 = Environment.TickCount;
            Response response = handler(request, new HandlerContext());
            var t2 = Environment.TickCount;


            Assert.AreEqual("200", response.code);
            Assert.IsTrue(t2 - t1 >= 1000);
            Assert.IsTrue(t2 - t1 <= 1250);
        }

        [Test]
        public void testDelayHandlerWithNextHandler()
        {

            var handler = Handlers.Delay(1000, x => new Response(606, "Something"));

            Request request = new Request("GET", "/");

            var t1 = Environment.TickCount;
            Response response = handler(request, new HandlerContext());
            var t2 = Environment.TickCount;


            Assert.AreEqual("606", response.code);
            Assert.IsTrue(t2 - t1 >= 1000);
            Assert.IsTrue(t2 - t1 <= 1250);
        }
    }
}