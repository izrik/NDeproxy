using NUnit.Framework;
using NDeproxy;

namespace NDeproxy.Tests
{
    [TestFixture]
    class BodiesTest
    {
        Deproxy deproxy;
        int port;
        string url;
        string body;

        [SetUp]
        public void setUp()
        {
            deproxy = new Deproxy();
            port = PortFinder.Singleton.getNextOpenPort();
            url = string.Format("http://localhost:{0}/", this.port);
            deproxy.addEndpoint(this.port);
            body = " This is another body\n\nThis is the next paragraph.\n";
        }

        [Test]
        public void testRequestBody()
        {
            var mc = this.deproxy.makeRequest(url: this.url, method: "POST", requestBody: body);

            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual(body, mc.sentRequest.body);
            Assert.AreEqual(body, mc.handlings[0].request.body);
        }

        [Test]
        public void testResponseBody()
        {

            Handler handler = (request =>
            new Response(200, "OK", "Content-type: text/plain", body)
                          );

            var mc = this.deproxy.makeRequest(url: this.url, defaultHandler: handler);

            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual(body, mc.handlings[0].response.body);
            Assert.AreEqual(body, mc.receivedResponse.body);
        }

        [Test]
        public void testDefaultRequestHeadersForTextBody()
        {

            var mc = this.deproxy.makeRequest(url: this.url, method: "POST",
                     requestBody: body);

            Assert.AreEqual(body, mc.sentRequest.body);
            Assert.AreEqual(1, mc.sentRequest.headers.getCountByName("Content-Type"));
            Assert.AreEqual("text/plain", mc.sentRequest.headers["Content-Type"]);
            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual(body, mc.handlings[0].request.body);
            Assert.AreEqual(1, mc.handlings[0].request.headers.getCountByName("Content-Type"));
            Assert.AreEqual("text/plain", mc.handlings[0].request.headers["Content-Type"]);
        }

        [Test]
        public void testNoDefaultRequestHeadersForTextBody()
        {

            var mc = this.deproxy.makeRequest(url: this.url, method: "POST",
                     requestBody: body,
                     addDefaultHeaders: false);

            Assert.AreEqual(body, mc.sentRequest.body);
            Assert.AreEqual(0, mc.sentRequest.headers.getCountByName("Content-Type"));
            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual("", mc.handlings[0].request.body); // because there's no Content-Length header, it doesn't read the body
            Assert.AreEqual(0, mc.handlings[0].request.headers.getCountByName("Content-Type"));
        }

        [Test]
        public void testDefaultResponseHeadersForTextBody()
        {

            Handler handler = (request =>
             new Response(200, "OK", null, body)
                          );

            var mc = this.deproxy.makeRequest(url: this.url,
                     defaultHandler: handler);

            Assert.AreEqual(body, mc.receivedResponse.body);
            Assert.AreEqual(1, mc.receivedResponse.headers.getCountByName("Content-Type"));
            Assert.AreEqual("text/plain", mc.receivedResponse.headers["Content-Type"]);
            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual(body, mc.handlings[0].response.body);
            Assert.AreEqual(1, mc.handlings[0].response.headers.getCountByName("Content-Type"));
            Assert.AreEqual("text/plain", mc.handlings[0].response.headers["Content-Type"]);
        }

        [Test]
        public void testNoDefaultResponseHeadersForTextBody()
        {

            HandlerWithContext handler = ( Request request, HandlerContext context) =>
            {
                context.sendDefaultResponseHeaders = false;
                return new Response(200, "OK", null, body);
            };

            var mc = this.deproxy.makeRequest(url: this.url,
                     defaultHandler: handler);

            Assert.AreEqual("200", mc.receivedResponse.code);
            Assert.AreEqual(1, mc.receivedResponse.headers.size());
            Assert.AreEqual(1, mc.receivedResponse.headers.getCountByName(Deproxy.REQUEST_ID_HEADER_NAME));
            Assert.AreEqual(0, mc.receivedResponse.headers.getCountByName("Content-Length"));
            Assert.AreEqual(0, mc.receivedResponse.headers.getCountByName("Content-Type"));
            Assert.AreEqual("", mc.receivedResponse.body);
            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual(1, mc.handlings[0].response.headers.size());
            Assert.AreEqual(1, mc.receivedResponse.headers.getCountByName(Deproxy.REQUEST_ID_HEADER_NAME));
            Assert.AreEqual(0, mc.handlings[0].response.headers.getCountByName("Content-Length"));
            Assert.AreEqual(0, mc.handlings[0].response.headers.getCountByName("Content-Type"));
            Assert.AreEqual(body, mc.handlings[0].response.body);
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