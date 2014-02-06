using NDeproxy;
using NUnit.Framework;

namespace NDeproxy.Tests
{
    [TestFixture]
    class EchoHandlerTest
    {
        [Test]
        public void testEchoHandler()
        {

            var response = Handlers.echoHandler(
                               new Request("GET", "/", "x-header: 12345", "this is the body"),
                               new HandlerContext());

            Assert.AreEqual("200", response.code);
            Assert.IsTrue(response.headers.contains("x-header"));
            Assert.AreEqual("12345", response.headers.getFirstValue("x-header"));
            Assert.AreEqual("this is the body", response.body);
        }
    }
}