using NDeproxy;
using NUnit.Framework;

namespace NDeproxy.Tests
{
    [TestFixture]
    class HeaderCollectionTest
    {
        HeaderCollection headers;

        [SetUp]
        public void setUp()
        {
            this.headers = new HeaderCollection();
        }

        [Test]
        public void testSize()
        {
            Assert.AreEqual(0, this.headers.size());

            this.headers.add("Name", "Value");
            Assert.AreEqual(1, this.headers.size());

            this.headers.add("Name", "Value");
            Assert.AreEqual(2, this.headers.size());

            this.headers.add("Name2", "Value");
            Assert.AreEqual(3, this.headers.size());
        }

        [Test]
        public void testContains()
        {
            this.headers.add("Name", "Value");

            Assert.AreEqual(1, this.headers.size());
            Assert.IsTrue(this.headers.contains("Name"));
        }

        [Test]
        public void testContainsCase()
        {
            this.headers.add("Name", "Value");

            Assert.AreEqual(1, this.headers.size());
            Assert.IsTrue(this.headers.contains("Name"));
            Assert.IsTrue(this.headers.contains("name"));
            Assert.IsTrue(this.headers.contains("NAME"));
            Assert.IsTrue(this.headers.contains("nAmE"));
        }

        [Test]
        public void testFindAll()
        {
            this.headers.add("A", "qwerty");
            this.headers.add("B", "asdf");
            this.headers.add("C", "zxcv");
            this.headers.add("A", "uiop");
            this.headers.add("A", "jkl;");

            CollectionAssert.AreEqual(new [] { "qwerty", "uiop", "jkl;" },
                this.headers.findAll("A"));

            CollectionAssert.AreEqual(new [] { "asdf" },
                this.headers.findAll("B"));

            CollectionAssert.AreEqual(new [] { "zxcv" },
                this.headers.findAll("C"));
        }

        [Test]
        public void testGetFirstValue()
        {
            this.headers.add("Name", "Value");

            Assert.AreEqual(1, this.headers.size());
            Assert.AreEqual("Value", this.headers.getFirstValue("Name"));
            Assert.AreEqual("Value", this.headers.getFirstValue("name"));
            Assert.AreEqual("Value", this.headers.getFirstValue("NAME"));
            Assert.AreEqual("Value", this.headers.getFirstValue("nAmE"));
        }

        [Test]
        public void testGetFirstValueWithDefault()
        {
            this.headers.add("Name", "Value");

            Assert.AreEqual(1, this.headers.size());
            Assert.AreEqual("Value", this.headers.getFirstValue("Name", "Other"));
            Assert.AreEqual("Value", this.headers.getFirstValue("name", "Other"));
            Assert.AreEqual("Value", this.headers.getFirstValue("NAME", "Other"));
            Assert.AreEqual("Value", this.headers.getFirstValue("nAmE", "Other"));

            Assert.AreEqual("Something", this.headers.getFirstValue("Other Name", "Something"));
        }

        [Test]
        public void testGetAt()
        {
            this.headers.add("Name", "Value");

            Assert.AreEqual(1, this.headers.size());
            Assert.AreEqual("Value", this.headers.getAt("Name"));
            Assert.AreEqual("Value", this.headers.getAt("name"));
            Assert.AreEqual("Value", this.headers.getAt("NAME"));
            Assert.AreEqual("Value", this.headers.getAt("nAmE"));
        }

        [Test]
        public void testMapNotation()
        {
            this.headers.add("Name", "Value");

            Assert.AreEqual(1, this.headers.size());
            Assert.AreEqual("Value", this.headers["Name"]);
            Assert.AreEqual("Value", this.headers["name"]);
            Assert.AreEqual("Value", this.headers["NAME"]);
            Assert.AreEqual("Value", this.headers["nAmE"]);
        }

        [Test]
        public void testGetCountByName()
        {

            this.headers.add("A", "qwerty");
            this.headers.add("B", "asdf");
            this.headers.add("C", "zxcv");
            this.headers.add("A", "uiop");
            this.headers.add("A", "jkl;");

            Assert.AreEqual(3, this.headers.getCountByName("A"));
            Assert.AreEqual(3, this.headers.getCountByName("a"));
            Assert.AreEqual(1, this.headers.getCountByName("B"));
            Assert.AreEqual(1, this.headers.getCountByName("C"));
        }
    }
}