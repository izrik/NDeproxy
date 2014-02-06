using NUnit.Framework;
using NDeproxy;

namespace NDeproxy.Tests
{
    [TestFixture]
    class CustomHandlersTest
    {
        int port;
        string url;
        Deproxy deproxy;
        Endpoint endpoint;

        [SetUp]
        public void setUp()
        {
            this.port = PortFinder.Singleton.getNextOpenPort();
            this.url = string.Format("http://localhost:{0}/", this.port);
            this.deproxy = new Deproxy();
            this.endpoint = this.deproxy.addEndpoint(this.port);
        }

        [Test]
        public void testCustomHandlerInlineClosure()
        {

            var mc = this.deproxy.makeRequest(url: this.url,
                         defaultHandler: (request =>
                    new Response(
                             code: 606,
                             message: "Spoiler",
                             headers: "Header-Name: Header-Value",
                             body: "Snape kills Dumbledore")
                         ));

            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual("606", mc.handlings[0].response.code);
            Assert.AreEqual("606", mc.receivedResponse.code);
        }

        Response customHandlerMethod(Request request)
        {
            return new Response(
                code: 606,
                message: "Spoiler",
                headers: "Header-Name: Header-Value",
                body: "Snape kills Dumbledore");
        }

        [Test]
        public void testCustomHandlerMethod()
        {
            var mc = this.deproxy.makeRequest(url: this.url,
                         defaultHandler: customHandlerMethod);

            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual("606", mc.handlings[0].response.code);
            Assert.AreEqual("606", mc.receivedResponse.code);
        }

        public static Response customHandlerStaticMethod(Request request)
        {
            return new Response(
                code: 606,
                message: "Spoiler",
                headers: "Header-Name: Header-Value",
                body: "Snape kills Dumbledore");
        }

        [Test]
        public void testCustomHandlerStaticMethod()
        {
            var mc = this.deproxy.makeRequest(url: this.url,
                         defaultHandler: CustomHandlersTest.customHandlerStaticMethod);

            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual("606", mc.handlings[0].response.code);
            Assert.AreEqual("606", mc.receivedResponse.code);
        }

        [Test]
        public void testCustomHandlerInlineClosureWithContext()
        {

            var mc = this.deproxy.makeRequest(url: this.url,
                         defaultHandler: (request, context) =>
                    new Response(
                             code: 606,
                             message: "Spoiler",
                             headers: new [] { "Header-Name: Header-Value", "Context-Object: " + context.ToString() },
                             body: "Snape kills Dumbledore")
                     );

            Assert.AreEqual("606", mc.receivedResponse.code);
            Assert.IsTrue(mc.receivedResponse.headers.contains("Header-Name"));
            Assert.AreEqual("Header-Value", mc.receivedResponse.headers["Header-Name"]);
            Assert.IsTrue(mc.receivedResponse.headers.contains("Context-Object"));
            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual("606", mc.handlings[0].response.code);
            Assert.IsTrue(mc.handlings[0].response.headers.contains("Header-Name"));
            Assert.AreEqual("Header-Value", mc.handlings[0].response.headers["Header-Name"]);
            Assert.IsTrue(mc.handlings[0].response.headers.contains("Context-Object"));
        }

        Response customHandlerMethodWithContext(Request request,
                                                HandlerContext context)
        {
            return new Response(
                606,
                "Spoiler",
                new [] { "Header-Name: Header-Value", "Context-Object: " + context.ToString() },
                "Snape kills Dumbledore");
        }

        [Test]
        public void testCustomHandlerMethodWithContext()
        {
            var mc = this.deproxy.makeRequest(url: this.url,
                         defaultHandler: this.customHandlerMethodWithContext);

            Assert.AreEqual("606", mc.receivedResponse.code);
            Assert.IsTrue(mc.receivedResponse.headers.contains("Header-Name"));
            Assert.AreEqual("Header-Value", mc.receivedResponse.headers["Header-Name"]);
            Assert.IsTrue(mc.receivedResponse.headers.contains("Context-Object"));
            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual("606", mc.handlings[0].response.code);
            Assert.IsTrue(mc.handlings[0].response.headers.contains("Header-Name"));
            Assert.AreEqual("Header-Value", mc.handlings[0].response.headers["Header-Name"]);
            Assert.IsTrue(mc.handlings[0].response.headers.contains("Context-Object"));
        }

        public static Response customHandlerStaticMethodWithContext(
            Request request,
            HandlerContext context)
        {
            return new Response(
                606,
                "Spoiler",
                new [] { "Header-Name: Header-Value", "Context-Object: " + context.ToString() },
                "Snape kills Dumbledore");
        }

        [Test]
        public void testCustomHandlerStaticMethodWithContext()
        {
            var mc = this.deproxy.makeRequest(url: this.url,
                         defaultHandler: CustomHandlersTest.customHandlerStaticMethodWithContext);

            Assert.AreEqual("606", mc.receivedResponse.code);
            Assert.IsTrue(mc.receivedResponse.headers.contains("Header-Name"));
            Assert.AreEqual("Header-Value", mc.receivedResponse.headers["Header-Name"]);
            Assert.IsTrue(mc.receivedResponse.headers.contains("Context-Object"));
            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual("606", mc.handlings[0].response.code);
            Assert.IsTrue(mc.handlings[0].response.headers.contains("Header-Name"));
            Assert.AreEqual("Header-Value", mc.handlings[0].response.headers["Header-Name"]);
            Assert.IsTrue(mc.handlings[0].response.headers.contains("Context-Object"));
        }

        [TearDown]
        public void tearDown()
        {
            if (this.deproxy != null)
            {
                this.deproxy.shutdown();
            }
        }
    }
}