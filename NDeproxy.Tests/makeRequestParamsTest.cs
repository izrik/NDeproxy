using NUnit.Framework;
using NDeproxy;
using System;

namespace NDeproxy.Tests
{
    [TestFixture]
    class makeRequestParamsTest
    {
        Deproxy deproxy;
        int port;
        string urlbase;

        [SetUp]
        public void setup()
        {

            this.deproxy = new Deproxy();
            this.port = PortFinder.Singleton.getNextOpenPort();
            this.urlbase = string.Format("http://localhost:{0}", port);
            this.deproxy.addEndpoint(this.port);
        }
        //    @Unroll("url param: path and query parameter combos: \"#pathPart#queryPart\" -> \"#expectedResult\"")
        [Test]
        [TestCase("", "", "/")]   
        [TestCase("/", "", "/")]
        [TestCase("/path", "", "/path")]
        [TestCase("/path/", "", "/path/")]
        [TestCase("", "?", "/?")]
        [TestCase("/", "?", "/?")]
        [TestCase("/path", "?", "/path?")]
        [TestCase("/path/", "?", "/path/?")]
        [TestCase("", "?name=value", "/?name=value")]
        [TestCase("/", "?name=value", "/?name=value")]
        [TestCase("/path", "?name=value", "/path?name=value")]
        [TestCase("/path/", "?name=value", "/path/?name=value")]
        public void urlParam_PathAndQueryParameterCombos(string pathPart, string queryPart, string expectedResult)
        {

            when: //"making the request"
            var mc = this.deproxy.makeRequest(url: string.Format("{0}{1}{2}", urlbase, pathPart, queryPart));

            then:
            Assert.AreEqual(expectedResult, mc.sentRequest.path);
            Assert.AreEqual(1, mc.handlers.Count);
            Assert.AreEqual(expectedResult, mc.handlings[0].request.path);

        }

        class DummyClientConnector : ClientConnector
        {
            public DummyClientConnector(Func<Request, bool, string, int?, RequestParams, Response> sender)
            {
                _sender = sender;
            }

            Func<Request, bool, string, int?, RequestParams, Response> _sender;

            #region ClientConnector implementation

            public Response sendRequest(Request request, bool https, string host, int? port, RequestParams rparams)
            {
                return _sender(request, https, host, port, rparams);
            }

            #endregion

        }

        [Test]
        public void hostParam_ShouldOverrideTheHostInUrlParam()
        {

            given:
            string hostValueAtConnector = null;
            var captureHostParam = new DummyClientConnector(
                                   (request, https, host, port, rparams) =>
                {

                    // by overriding the client connector, we can record what host value it received and skip  an actual TCP connection
                    hostValueAtConnector = host;
                    return new Response(200);
                });

            var hostname = "example.com";

            when: //"send a request with an explicit path param"
            var mc = this.deproxy.makeRequest(
                     url: string.Format("{0}/urlpath", urlbase),
                     host: hostname,
                     clientConnector: captureHostParam);

            then: //"the host in the sent request (and in the Host header) should be that of the host param"
            Assert.AreEqual(hostname, hostValueAtConnector);
        }

        [Test]
        public void hostParam_ShouldAllowInvalidCharacters()
        {

            given:
            string hostValueAtConnector = null;
            var captureHostParam = new DummyClientConnector(
                                   (request, https, host, port, rparams) =>
                {

                    // by overriding the client connector, we can record what host value it received and skip  an actual TCP connection
                    hostValueAtConnector = host;
                    return new Response(200);
                });

            var hostname = "!@#$";

            when: //"send a request with an explicit path param"
            var mc = this.deproxy.makeRequest(
                     url: string.Format("{0}/urlpath", urlbase),
                     host: hostname,
                     clientConnector: captureHostParam);

            then: //"the host in the sent request (and in the Host header) should be that of the host param"
            Assert.AreEqual(hostname, hostValueAtConnector);
        }

        [Test]
        public void portParam_ShouldOverrideThePortInUrlParam()
        {

            given:
            int? portValueAtConnector = null;
            var capturePortParam = new DummyClientConnector(
                                   (request, https, host, port, rparams) =>
                {
                    // by overriding the client connector, we can record what port value it received and skip an actual TCP connection
                    portValueAtConnector = port;
                    return new Response(200);
                });

            int expectedPort = 12345;

            when: //"send a request with an explicit path param"
            var mc = this.deproxy.makeRequest(
                     url: "http://localhost:8080/urlpath",
                     port: expectedPort,
                     clientConnector: capturePortParam);

            then: //"the host in the sent request (and in the Host header) should be that of the host param"
            Assert.AreEqual(expectedPort, portValueAtConnector);
        }

        [Test]
        public void pathParam_ShouldOverrideThePathIn_urlurl()
        {

            when: //"send a request with an explicit path param"
            var mc = this.deproxy.makeRequest(url: urlbase + "/urlpath", path: "/parampath");

            then: //"the path in the sent request should be that of the path param"
            Assert.AreEqual("/parampath", mc.sentRequest.path);
        }

        [Test]
        public void pathParam_ShouldAllowOtherwiseInvalidCharacters()
        {

            when: //"send a request with an explicit path param with invalid characters"
            var mc = this.deproxy.makeRequest(url: urlbase + "/urlpath", path: "/parampath?query=value@%");

            then: //"the path in the sent request should be that of the path param"
            Assert.AreEqual("/parampath?query=value@%", mc.sentRequest.path);
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
