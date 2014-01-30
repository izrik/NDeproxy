using NUnit.Framework;
using NDeproxy;

namespace NDeproxy.Tests
{
    [TestFixture]
    class HeaderCollectionConstructorsTest
    {
        [Test]
        public void noArgumentsToTheConstructor()
        {

            when: //"call the constructor with no arguments"
            var hc = new HeaderCollection();

            then: //"should be valid, empty collection"
            Assert.IsNotNull(hc);
            Assert.IsInstanceOf<HeaderCollection>(hc);
            Assert.AreEqual(0, hc.size());
        }

        [Test]
        public void nullArgumentToTheConstructor()
        {

            when: //"call the constructor with a single null argument"
            var hc = new HeaderCollection((object)null);

            then: //"should be valid, empty collection"
            Assert.IsNotNull(hc);
            Assert.IsInstanceOf<HeaderCollection>(hc);
            Assert.AreEqual(0, hc.size());
        }

        [Test]
        public void HeaderArgument()
        {

            when:
            HeaderCollection hc = new HeaderCollection(new Header("n1", "v1"));

            then:
            Assert.IsNotNull(hc);
            Assert.AreEqual(1, hc.size());
            Assert.AreEqual("n1", hc[0].name);
            Assert.AreEqual("v1", hc[0].value);
        }

        [Test]
        public void StringArgument()
        {

            when:
            HeaderCollection hc = new HeaderCollection("n1:v1");

            then:
            Assert.IsNotNull(hc);
            Assert.AreEqual(1, hc.size());
            Assert.AreEqual("n1", hc[0].name);
            Assert.AreEqual("v1", hc[0].value);

        }

        [Test]
        public void StringArgumentLeadingSpaceInName()
        {

            when:
            HeaderCollection hc = new HeaderCollection("  n1:v1");

            then:
            Assert.IsNotNull(hc);
            Assert.AreEqual(1, hc.size());
            Assert.AreEqual("n1", hc[0].name);
            Assert.AreEqual("v1", hc[0].value);
        }

        [Test]
        public void StringArgumentTrailingSpaceInName()
        {

            when:
            HeaderCollection hc = new HeaderCollection("n1  :v1");

            then:
            Assert.IsNotNull(hc);
            Assert.AreEqual(1, hc.size());
            Assert.AreEqual("n1", hc[0].name);
            Assert.AreEqual("v1", hc[0].value);
        }

        [Test]
        public void StringArgumentLeadingSpaceInValue()
        {

            when:
            HeaderCollection hc = new HeaderCollection("n1:  v1");

            then:
            Assert.IsNotNull(hc);
            Assert.AreEqual(1, hc.size());
            Assert.AreEqual("n1", hc[0].name);
            Assert.AreEqual("v1", hc[0].value);
        }

        [Test]
        public void StringArgumentTrailingSpaceInValue()
        {

            when:
            HeaderCollection hc = new HeaderCollection("n1:v1  ");

            then:
            Assert.IsNotNull(hc);
            Assert.AreEqual(1, hc.size());
            Assert.AreEqual("n1", hc[0].name);
            Assert.AreEqual("v1", hc[0].value);
        }

        [Test]
        public void StringArgumentSinglePart()
        {

            when:
            HeaderCollection hc = new HeaderCollection("n1");

            then:
            Assert.IsNotNull(hc);
            Assert.AreEqual(1, hc.size());
            Assert.AreEqual("n1", hc[0].name);
            Assert.AreEqual("", hc[0].value);
        }

        [Test]
        public void StringArgumentThreeParts()
        {

            when:
            HeaderCollection hc = new HeaderCollection("n1: v1: v2");

            then:
            Assert.IsNotNull(hc);
            Assert.AreEqual(1, hc.size());
            Assert.AreEqual("n1", hc[0].name);
            Assert.AreEqual("v1: v2", hc[0].value);
        }
        //    [Test]
        //    public void "object argument"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(new DummyObject(id: 3))
        //
        //        then:
        //        hc != null
        //        hc.size() == 1
        //        hc[0].name == "dummy object, id = 3"
        //        hc[0].value == ""
        //
        //    }
        //
        //    [Test]
        //    public void "empty list"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection([ ]);
        //
        //        then:
        //        hc != null
        //        hc.size() == 0
        //
        //    }
        //
        //
        //    [Test]
        //    public void "list with string element"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection([ "n1: v1" ]);
        //
        //        then:
        //        hc != null
        //        hc.size() == 1
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //
        //    }
        //
        //    [Test]
        //    public void "list with null element"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection([
        //                new Header("n1", "v1"),
        //                null,
        //                new Header("n2", "v2")
        //        ]);
        //
        //        then:
        //        hc != null
        //        hc.size() == 2
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //
        //    }
        //
        //    [Test]
        //    public void "list of headers argument"() {
        //
        //        when: "call the constructor with a list of headers"
        //        HeaderCollection hc = new HeaderCollection([ new Header("name1", "value1"), new Header("name2", "value2") ])
        //
        //        then: "should be valid collection with the given headers in the given order"
        //        hc != null
        //        hc instanceof HeaderCollection
        //        hc.size() == 2
        //        hc[0].name == "name1"
        //        hc[0].value == "value1"
        //        hc[1].name == "name2"
        //        hc[1].value == "value2"
        //    }
        //
        //    [Test]
        //    public void "map argument"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection([
        //                n1: "v1",
        //                n2: "v2",
        //                n3: "v3",
        //                n4: "v4",
        //        ])
        //
        //        then:
        //        hc != null
        //        hc.size() == 4
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //        hc[2].name == "n3"
        //        hc[2].value == "v3"
        //        hc[3].name == "n4"
        //        hc[3].value == "v4"
        //
        //    }
        //
        //    [Test]
        //    public void "empty map"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection([:])
        //
        //        then:
        //        hc != null
        //        hc.size() == 0
        //
        //    }
        //
        //    [Test]
        //    public void "map with null key"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection([
        //                n1: "v1",
        //                (null): "v2",
        //        ])
        //
        //        then:
        //        hc != null
        //        hc.size() == 2
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == ""
        //        hc[1].value == "v2"
        //
        //    }
        //
        //    [Test]
        //    public void "map with null value"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection([
        //                n1: "v1",
        //                n2: null,
        //        ])
        //
        //        then:
        //        hc != null
        //        hc.size() == 2
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == ""
        //
        //    }
        //
        //    [Test]
        //    public void "another HeaderCollection as argument"() {
        //
        //        given:
        //        var hc1 = new HeaderCollection()
        //        hc1.add("n1", "v1")
        //        hc1.add("n2", "v2")
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(hc1)
        //
        //        then:
        //        hc != null
        //        hc.size() == 2
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //    }
        //
        //    [Test]
        //    public void "empty array"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection([] as Object[]);
        //
        //        then:
        //        hc != null
        //        hc.size() == 0
        //
        //    }
        //
        //    [Test]
        //    public void "array with string element"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection([
        //                "n1: v1",
        //        ] as Object[]);
        //
        //        then:
        //        hc != null
        //        hc.size() == 1
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //
        //    }
        //
        //    [Test]
        //    public void "array with null element"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection([
        //                new Header("n1", "v1"),
        //                null,
        //                new Header("n2", "v2")
        //        ] as Object[]);
        //
        //        then:
        //        hc != null
        //        hc.size() == 2
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //
        //    }
        //
        //    [Test]
        //    public void "array of headers argument"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection([
        //                new Header("n1", "v1"),
        //                new Header("n2", "v2")
        //        ] as Object[]);
        //
        //        then:
        //        hc != null
        //        hc.size() == 2
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //
        //    }
        //
        //    [Test]
        //    public void "multiple null arguments"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(null, null, null)
        //
        //        then:
        //        hc != null
        //        hc.size() == 0
        //
        //    }
        //
        //    [Test]
        //    public void "multiple string arguments"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(
        //                "n1: v1",
        //                "n2: v2",
        //                "n3: v3")
        //
        //        then:
        //        hc != null
        //        hc.size() == 3
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //        hc[2].name == "n3"
        //        hc[2].value == "v3"
        //
        //    }
        //
        //    [Test]
        //    public void "multiple object arguments"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(
        //                new DummyObject(id: 6),
        //                new DummyObject(id: 7),
        //                new DummyObject(id: 8))
        //
        //        then:
        //        hc != null
        //        hc.size() == 3
        //        hc[0].name == "dummy object, id = 6"
        //        hc[0].value == ""
        //        hc[1].name == "dummy object, id = 7"
        //        hc[1].value == ""
        //        hc[2].name == "dummy object, id = 8"
        //        hc[2].value == ""
        //
        //    }
        //
        //    [Test]
        //    public void "multiple map arguments"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(
        //                [ n1: "v1", n2: "v2" ],
        //                [ n3: "v3", n4: "v4" ])
        //
        //        then:
        //        hc != null
        //        hc.size() == 4
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //        hc[2].name == "n3"
        //        hc[2].value == "v3"
        //        hc[3].name == "n4"
        //        hc[3].value == "v4"
        //
        //    }
        //
        //    [Test]
        //    public void "multiple list arguments"() {
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(
        //                [ new Header("n1", "v1"), new Header("n2", "v2") ],
        //                [ new Header("n3", "v3"), new Header("n4", "v4") ],
        //        )
        //
        //        then:
        //        hc != null
        //        hc.size() == 4
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //        hc[2].name == "n3"
        //        hc[2].value == "v3"
        //        hc[3].name == "n4"
        //        hc[3].value == "v4"
        //
        //    }
        //
        //    [Test]
        //    public void "multiple HeaderCollection arguments"() {
        //
        //        given:
        //        var hc1 = new HeaderCollection()
        //        hc1.add("n1", "v1")
        //        hc1.add("n2", "v2")
        //        var hc2 = new HeaderCollection()
        //        hc2.add("n3", "v3")
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(hc1, hc2)
        //
        //        then:
        //        hc != null
        //        hc.size() == 3
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //        hc[2].name == "n3"
        //        hc[2].value == "v3"
        //
        //    }
        //
        //    [Test]
        //    public void "multiple mixed arguments"() {
        //
        //        given:
        //        var hc1 = new HeaderCollection()
        //        hc1.add("n1", "v1")
        //        hc1.add("n2", "v2")
        //        DummyObject dobj = new DummyObject(id: 10)
        //        var str = "is a"
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(
        //                [ new Header("n3", "v3"), new Header("n4", "v4"), "n5: v5" ],
        //                [ n6: "v6", n7: "v7" ],
        //                [
        //                        [
        //                                [ n8: "v8"],
        //                                hc1,
        //                        ],
        //                        [ n9: "v9" ],
        //                        dobj,
        //                        "Groovy: This ${str} groovy string"
        //                ])
        //
        //        then:
        //        hc != null
        //        hc.size() == 11
        //        hc[0].name == "n3"
        //        hc[0].value == "v3"
        //        hc[1].name == "n4"
        //        hc[1].value == "v4"
        //        hc[2].name == "n5"
        //        hc[2].value == "v5"
        //        hc[3].name == "n6"
        //        hc[3].value == "v6"
        //        hc[4].name == "n7"
        //        hc[4].value == "v7"
        //        hc[5].name == "n8"
        //        hc[5].value == "v8"
        //        hc[6].name == "n1"
        //        hc[6].value == "v1"
        //        hc[7].name == "n2"
        //        hc[7].value == "v2"
        //        hc[8].name == "n9"
        //        hc[8].value == "v9"
        //        hc[9].name == "dummy object, id = 10"
        //        hc[9].value == ""
        //        hc[10].name == "Groovy"
        //        hc[10].value == "This is a groovy string"
        //    }
        //
        //    [Test]
        //    public void "re-use list argument"() {
        //
        //        given:
        //        var list = [ new Header("n1", "v1"), new Header("n2", "v2") ]
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(list, "n3: v3", list)
        //
        //        then:
        //        hc != null
        //        hc.size() == 5
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //        hc[2].name == "n3"
        //        hc[2].value == "v3"
        //        hc[3].name == "n1"
        //        hc[3].value == "v1"
        //        hc[4].name == "n2"
        //        hc[4].value == "v2"
        //
        //    }
        //
        //    [Test]
        //    public void "cycle list argument"() {
        //
        //        given:
        //        var list = [ new Header("n1", "v1"), new Header("n2", "v2") ]
        //        list.add(list)
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(list)
        //
        //        then:
        //        hc != null
        //        hc.size() == 2
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //
        //    }
        //
        //    [Test]
        //    public void "re-use map argument"() {
        //
        //        given:
        //        var map = [ n1: "v1", n2: "v2" ]
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(map, "n3: v3", map)
        //
        //        then:
        //        hc != null
        //        hc.size() == 5
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //        hc[2].name == "n3"
        //        hc[2].value == "v3"
        //        hc[3].name == "n1"
        //        hc[3].value == "v1"
        //        hc[4].name == "n2"
        //        hc[4].value == "v2"
        //
        //    }
        //
        //    [Test]
        //    public void "cycle map argument as name"() {
        //
        //        given:
        //        var map = [ n1: "v1", n2: "v2" ]
        //        var extras = [ (map): "v3" ]
        //        map.putAll(extras)
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(map)
        //
        //        then:
        //        hc != null
        //        hc.size() == 3
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //        hc[2].name == ""
        //        hc[2].value == "v3"
        //
        //    }
        //
        //    [Test]
        //    public void "cycle map argument as value"() {
        //
        //        given:
        //        var map = [ n1: "v1", n2: "v2", n3: "v3" ]
        //        map["n3"] = map
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(map)
        //
        //        then:
        //        hc != null
        //        hc.size() == 3
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //        hc[2].name == "n3"
        //        hc[2].value == map.toString()
        //
        //    }
        //
        //    [Test]
        //    public void "re-use array argument"() {
        //
        //        given:
        //        Object[] arr = [ new Header("n1", "v1"), new Header("n2", "v2") ]
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(arr, "n3: v3", arr)
        //
        //        then:
        //        hc != null
        //        hc.size() == 5
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //        hc[2].name == "n3"
        //        hc[2].value == "v3"
        //        hc[3].name == "n1"
        //        hc[3].value == "v1"
        //        hc[4].name == "n2"
        //        hc[4].value == "v2"
        //
        //    }
        //
        //    [Test]
        //    public void "cycle array argument"() {
        //
        //        given:
        //        Object[] arr = [ new Header("n1", "v1"), new Header("n2", "v2"), null ]
        //        arr[2] = arr
        //
        //        when:
        //        HeaderCollection hc = new HeaderCollection(arr)
        //
        //        then:
        //        hc != null
        //        hc.size() == 2
        //        hc[0].name == "n1"
        //        hc[0].value == "v1"
        //        hc[1].name == "n2"
        //        hc[1].value == "v2"
        //    }
    }
}