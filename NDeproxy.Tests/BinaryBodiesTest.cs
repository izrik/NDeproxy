using NUnit.Framework;
using System.Linq;
using NDeproxy;

namespace NDeproxy.Tests
{
    [TestFixture]
    class BinaryBodiesTest
    {
        Deproxy deproxy;
        int port;
        string url;
        byte[] body;

        [SetUp]
        public void setUp()
        {
            deproxy = new Deproxy();
            port = PortFinder.Singleton.getNextOpenPort();
            url = string.Format("http://localhost:{0}/", this.port);
            deproxy.addEndpoint(this.port);
            body = Enumerable.Range(-128, 256).Select(x => (byte)x).ToArray();
        }

        [Test]
        public void testBinaryRequestBody()
        {

            var mc = this.deproxy.makeRequest(
                         url: this.url,
                         method: "POST",
                         headers: new Header("Content-type", "application/octet-stream"),
                         requestBody: body);

            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual(body, mc.sentRequest.body);
            Assert.AreEqual(body, mc.handlings[0].request.body);
        }

        [Test]
        public void testBinaryResponseBody()
        {

            Handler handler = (request => new Response(200, "OK", new Header("Content-type", "application/octet-stream"), body));

            var mc = this.deproxy.makeRequest(url: this.url, defaultHandler: handler);

            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual(body, mc.handlings[0].response.body);
            Assert.AreEqual(body, mc.receivedResponse.body);
        }

        [Test]
        public void testDefaultRequestHeadersForBinaryBody()
        {

            var mc = this.deproxy.makeRequest(url: this.url, method: "POST",
                         requestBody: body);

            Assert.AreEqual(body, mc.sentRequest.body);
            Assert.AreEqual(1, mc.sentRequest.headers.getCountByName("Content-Type"));
            Assert.AreEqual("application/octet-stream", mc.sentRequest.headers["Content-Type"]);
            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual(body, mc.handlings[0].request.body);
            Assert.AreEqual(1, mc.handlings[0].request.headers.getCountByName("Content-Type"));
            Assert.AreEqual("application/octet-stream", mc.handlings[0].request.headers["Content-Type"]);
        }

        [Test]
        public void testNoDefaultRequestHeadersForBinaryBody()
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
        public void testDefaultResponseHeadersForBinaryBody()
        {

            Handler handler = (request =>
            new Response(200, "OK", body: body)
                              );

            var mc = this.deproxy.makeRequest(url: this.url,
                         defaultHandler: handler);

            Assert.AreEqual(body, mc.receivedResponse.body);
            Assert.AreEqual(1, mc.receivedResponse.headers.getCountByName("Content-Type"));
            Assert.AreEqual("application/octet-stream", mc.receivedResponse.headers["Content-Type"]);
            Assert.AreEqual(1, mc.handlings.Count);
            Assert.AreEqual(body, mc.handlings[0].response.body);
            Assert.AreEqual(1, mc.handlings[0].response.headers.getCountByName("Content-Type"));
            Assert.AreEqual("application/octet-stream", mc.handlings[0].response.headers["Content-Type"]);
        }

        [Test]
        public void testNoDefaultResponseHeadersForBinaryBody()
        {

            HandlerWithContext handler = (request, context) =>
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