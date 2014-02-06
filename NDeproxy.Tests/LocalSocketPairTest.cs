using NDeproxy;
using NUnit.Framework;
using System.IO;
using System.Net.Sockets;

namespace NDeproxy.Tests
{
    [TestFixture]
    class LocalSocketPairTest
    {
        [Test]
        public void testLocalSocketPairCreation()
        {

            var pair = LocalSocketPair.createLocalSocketPair();
            var client = pair.Item1;
            var server = pair.Item2;

            var clientStream = new UnbufferedSocketStream(client);
            var serverStream = new UnbufferedSocketStream(server);


            var writer = new StreamWriter(clientStream);
            var reader = new UnbufferedStreamReader(serverStream);

            writer.WriteLine("asdf");
            writer.Flush();
            var asdf = LineReader.readLine(reader);

            Assert.AreEqual(asdf, "asdf");



            writer = new StreamWriter(serverStream);
            reader = new UnbufferedStreamReader(clientStream);

            writer.WriteLine("qwerty");
            writer.Flush();
            var qwerty = LineReader.readLine(reader);

            Assert.AreEqual(qwerty, "qwerty");

            client.Close();
            server.Close();
        }

        [Test]
        public void testLocalSocketPairCreationWithPort()
        {

            var pair = LocalSocketPair.createLocalSocketPair(12345);
            var client = pair.Item1;
            var server = pair.Item2;

            Assert.AreEqual(12345, client.GetRemotePort());
            Assert.AreEqual(12345, server.GetLocalPort());

            var clientStream = new UnbufferedSocketStream(client);
            var serverStream = new UnbufferedSocketStream(server);

            var writer = new StreamWriter(clientStream);
            var reader = new UnbufferedStreamReader(serverStream);

            writer.WriteLine("asdf");
            writer.Flush();
            var asdf = LineReader.readLine(reader);

            Assert.AreEqual(asdf, "asdf");



            writer = new StreamWriter(serverStream);
            reader = new UnbufferedStreamReader(clientStream);

            writer.WriteLine("qwerty");
            writer.Flush();
            var qwerty = LineReader.readLine(reader);

            Assert.AreEqual(qwerty, "qwerty");

            client.Close();
            server.Close();
        }
    }
}