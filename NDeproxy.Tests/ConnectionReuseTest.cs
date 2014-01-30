







//class ConnectionReuseTest {
//
//    Deproxy deproxy;
//    int port;
//    string url;
//    ApacheClientConnector client;
//
//    [SetUp]
//    void setUp() {
//
//        // HttpClient re-uses connection by default
//        // we don't need to roll our own code to create and re-use connections
//        client = new ApacheClientConnector()
//
//        this.deproxy = new Deproxy(null, client);
//
//        this.port = PortFinder.Singleton.getNextOpenPort();
//        this.url = "http://localhost:${this.port}/";
//
//        this.deproxy.addEndpoint(this.port);
//    }
//
//    [Test]
//    void testServerSideConnectionReuse() {
//
//        var mc1 = deproxy.makeRequest(url: url)
//        Assert.AreEqual(1, mc1.handlings.Count)
//
//        var mc2 = deproxy.makeRequest(url: url)
//        Assert.AreEqual(1, mc2.handlings.Count)
//
//        Assert.AreEqual(mc1.handlings[0].connection, mc2.handlings[0].connection)
//    }
//
//    [TearDown]
//    void tearDown() {
//        if (this.deproxy != null) {
//            this.deproxy.shutdown();
//        }
//    }
//}
