using System;
using NUnit.Framework;
using NDeproxy;

namespace NDeproxy.Tests
{
    [TestFixture]
    class HostHeaderTest
    {
        [Test]
        public void TestBasicUsage()
        {

            when:
            var hh = new HostHeader("localhost", 8080);

            then:
            Assert.AreEqual("localhost", hh.host);
            Assert.AreEqual(8080, hh.port);
            Assert.AreEqual("Host", hh.name);
            Assert.AreEqual("localhost:8080", hh.value);
        }
        //    @Unroll("when we call CreateHostHeaderValue with #host, #port, and https=#https, we should get #expectedValue")
        [Test]
        [TestCase("localhost", 80, true, "localhost:80")]
        [TestCase("localhost", 80, false, "localhost:80")]
        [TestCase("localhost", 80, null, "localhost:80")]
        [TestCase("localhost", 443, true, "localhost:443")]
        [TestCase("localhost", 443, false, "localhost:443")]
        [TestCase("localhost", 443, null, "localhost:443")]
        [TestCase("localhost", 12345, true, "localhost:12345")]
        [TestCase("localhost", 12345, false, "localhost:12345")]
        [TestCase("localhost", 12345, null, "localhost:12345")]
        [TestCase("localhost", null, true, "localhost:443")]
        [TestCase("localhost", null, false, "localhost:80")]
        [TestCase("localhost", null, null, "localhost")]
        [TestCase("example.com", 80, true, "example.com:80")]
        [TestCase("example.com", 80, false, "example.com:80")]
        [TestCase("example.com", 80, null, "example.com:80")]
        [TestCase("example.com", 443, true, "example.com:443")]
        [TestCase("example.com", 443, false, "example.com:443")]
        [TestCase("example.com", 443, null, "example.com:443")]
        [TestCase("example.com", 12345, true, "example.com:12345")]
        [TestCase("example.com", 12345, false, "example.com:12345")]
        [TestCase("example.com", 12345, null, "example.com:12345")]
        [TestCase("example.com", null, true, "example.com:443")]
        [TestCase("example.com", null, false, "example.com:80")]
        [TestCase("example.com", null, null, "example.com")]
        [TestCase("12.34.56.78", 80, true, "12.34.56.78:80")]
        [TestCase("12.34.56.78", 80, false, "12.34.56.78:80")]
        [TestCase("12.34.56.78", 80, null, "12.34.56.78:80")]
        [TestCase("12.34.56.78", 443, true, "12.34.56.78:443")]
        [TestCase("12.34.56.78", 443, false, "12.34.56.78:443")]
        [TestCase("12.34.56.78", 443, null, "12.34.56.78:443")]
        [TestCase("12.34.56.78", 12345, true, "12.34.56.78:12345")]
        [TestCase("12.34.56.78", 12345, false, "12.34.56.78:12345")]
        [TestCase("12.34.56.78", 12345, null, "12.34.56.78:12345")]
        [TestCase("12.34.56.78", null, true, "12.34.56.78:443")]
        [TestCase("12.34.56.78", null, false, "12.34.56.78:80")]
        [TestCase("12.34.56.78", null, null, "12.34.56.78")]
        public void TestHostHeader(string host, int? port, bool? https, string expectedValue)
        {

            expect:
            Assert.AreEqual(expectedValue, HostHeader.CreateHostHeaderValue(host, port, https));

        }

        //    @Unroll("when we call CreateHostHeaderValue with #host and #port, we should get #expectedValue (e.g., https is not specified)")
        [Test]
        [TestCase("localhost", 80, "localhost:80")]
        [TestCase("localhost", 443, "localhost:443")]
        [TestCase("localhost", 12345, "localhost:12345")]
        [TestCase("localhost", null, "localhost")]
        [TestCase("example.com", 80, "example.com:80")]
        [TestCase("example.com", 443, "example.com:443")]
        [TestCase("example.com", 12345, "example.com:12345")]
        [TestCase("example.com", null, "example.com")]
        [TestCase("12.34.56.78", 80, "12.34.56.78:80")]
        [TestCase("12.34.56.78", 443, "12.34.56.78:443")]
        [TestCase("12.34.56.78", 12345, "12.34.56.78:12345")]
        [TestCase("12.34.56.78", null, "12.34.56.78")]
        public void TestHostHeaderDefaultHttpsArgument(string host, int? port, string expectedValue)
        {

            expect:
            Assert.AreEqual(expectedValue, HostHeader.CreateHostHeaderValue(host, port));
        }

        //    @Unroll("when we call CreateHostHeaderValue with #host, we should get #expectedValue (e.g., neither port nor https is specified)")
        [Test]
        [TestCase("localhost", "localhost")] 
        [TestCase("example.com", "example.com")]
        [TestCase("12.34.56.78", "12.34.56.78")] 
        public void TestHostHeaderDefaultPortAndHttpsArguments(string host, string expectedValue)
        {

            expect:
            Assert.AreEqual(expectedValue, HostHeader.CreateHostHeaderValue(host));
        }

        //    @Unroll("when we create a HostHeader with #host and #port, we should get #expectedValue")
        [Test]
        [TestCase("localhost", 80, "localhost:80")]
        [TestCase("localhost", 443, "localhost:443")]
        [TestCase("localhost", 12345, "localhost:12345")]
        [TestCase("localhost", null, "localhost")]
        [TestCase("example.com", 80, "example.com:80")]
        [TestCase("example.com", 443, "example.com:443")]
        [TestCase("example.com", 12345, "example.com:12345")]
        [TestCase("example.com", null, "example.com")]
        [TestCase("12.34.56.78", 80, "12.34.56.78:80")]
        [TestCase("12.34.56.78", 443, "12.34.56.78:443")]
        [TestCase("12.34.56.78", 12345, "12.34.56.78:12345")]
        [TestCase("12.34.56.78", null, "12.34.56.78")]
        public void TestHostHeaderConstructorDefaultHttpsArgument(string host, int? port, string expectedValue)
        {

            when:
            HostHeader hh = new HostHeader(host, port);

            then:
            Assert.AreEqual("Host", hh.name);
            Assert.AreEqual(expectedValue, hh.value);
        }
        //    @Unroll("Calling fromString with #value should give #host and #port")
        [Test]
        [TestCase("localhost", "localhost", null)]
        [TestCase("localhost:", "localhost", null)]
        [TestCase("localhost:443", "localhost", 443)]
        [TestCase("localhost:12345", "localhost", 12345)]
        [TestCase("example.com", "example.com", null)]
        [TestCase("example.com:", "example.com", null)]
        [TestCase("example.com:443", "example.com", 443)]
        [TestCase("example.com:12345", "example.com", 12345)]
        [TestCase("12.34.56.78", "12.34.56.78", null)]
        [TestCase("12.34.56.78:", "12.34.56.78", null)]
        [TestCase("12.34.56.78:443", "12.34.56.78", 443)]
        [TestCase("12.34.56.78:12345", "12.34.56.78", 12345)]
        [TestCase("12345.com", "12345.com", null)]
        [TestCase("12345.com:", "12345.com", null)]
        [TestCase("12345.com:443", "12345.com", 443)]
        [TestCase("12345.com:12345", "12345.com", 12345)]
        [TestCase("example.com.", "example.com.", null)]
        [TestCase("example.com.:", "example.com.", null)]
        [TestCase("example.com.:443", "example.com.", 443)]
        [TestCase("example.com.:12345", "example.com.", 12345)]
        [TestCase(" example.com:12345", "example.com", 12345)]
        [TestCase("example.com :12345", "example.com", 12345)]
        [TestCase(" example.com :12345", "example.com", 12345)]
        [TestCase("example.com: 12345", "example.com", 12345)]
        [TestCase("example.com:12345 ", "example.com", 12345)]
        [TestCase("example.com: 12345 ", "example.com", 12345)]
        [TestCase("hyp-hen.example.com", "hyp-hen.example.com", null)]
        [TestCase("hyp-hen.example.com:", "hyp-hen.example.com", null)]
        [TestCase("hyp-hen.example.com:443", "hyp-hen.example.com", 443)]
        [TestCase("hyp-hen.example.com:12345", "hyp-hen.example.com", 12345)]
        public void TestHostHeaderFromString(string value, string host, int? port)
        {

            when:
            HostHeader hh = HostHeader.fromString(value);

            then:
            Assert.AreEqual("Host", hh.name);
            Assert.AreEqual(host, hh.host);
            Assert.AreEqual(port, hh.port);
        }

        //    @Unroll("Calling fromString with #value should throw an exception")
        [Test]
        [TestCase("12.34.56.78.")]
        [TestCase("12.34.56.78.:")]
        [TestCase("12.34.56.78.:443")]
        [TestCase("12.34.56.78.:12345")]
        [TestCase("example.com:asdf")]
        [TestCase("example.com:123 45")]
        [TestCase("example.com:.123")]
        [TestCase("exam ple.com:123")]
        [TestCase("exam$ple.com:123")]
        [TestCase("12.34.56.78.90")]
        [TestCase("12.34.56")]
        public void TestHostHeaderFromStringErrors(string value)
        {

            Assert.Throws<ArgumentException>(() =>
            {
                HostHeader hh = HostHeader.fromString(value);
            });

        }
    }
}