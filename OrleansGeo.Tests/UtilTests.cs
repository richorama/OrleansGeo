using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using OrleansGeo.Grains;
using OrleansGeo.GrainInterfaces;

namespace OrleansGeo.Tests
{
    [TestClass]
    public class UtilTests
    {
        [TestMethod]
        public void TestGetAllParents()
        {
            var parents = "foobar".GetAllParents().ToArray();
            Assert.AreEqual(4, parents.Length);
            Assert.AreEqual("foobar", parents[0]);
            Assert.AreEqual("fooba", parents[1]);
            Assert.AreEqual("foob", parents[2]);
            Assert.AreEqual("foo", parents[3]);
        }

        [TestMethod]
        public void TestGetAllParentsWithNull()
        {
            string x = null;
            var parents = x.GetAllParents().ToArray();
            Assert.IsNotNull(parents);
            Assert.AreEqual(0, parents.Length);
        }

        [TestMethod]
        public void TestGetAllParentsWithShortString()
        {
            var parents = "x".GetAllParents().ToArray();
            Assert.IsNotNull(parents);
            Assert.AreEqual(0, parents.Length);
        }

        [TestMethod]
        public void TestGetQuadKey()
        {
            var key = new Position { Latitude = 50, Longitude = 1 }.ToQuadKey();
            Assert.IsNotNull(key);
            Assert.AreEqual(23, key.Length);
        }

        [TestMethod]
        public void TestDeltaEmpty()
        {
            var delta = ExtensionMethods.GetDelta("foobarbaz", "foobarbaz").ToArray();
            Assert.IsNotNull(delta);
            Assert.AreEqual(0, delta.Length);
        }

        [TestMethod]
        public void TestDeltaWithChange()
        {
            var delta = ExtensionMethods.GetDelta("foobarbaz", "foobarbax").ToArray();
            Assert.IsNotNull(delta);
            Assert.AreEqual(2, delta.Length);

            Assert.AreEqual(ExtensionMethods.Delta.Leave, delta[0].Delta);
            Assert.AreEqual("foobarbaz", delta[0].QuadKey);

            Assert.AreEqual(ExtensionMethods.Delta.Join, delta[1].Delta);
            Assert.AreEqual("foobarbax", delta[1].QuadKey);
        }

        [TestMethod]
        public void TestDeltaWithNew()
        {
            var delta = ExtensionMethods.GetDelta(null, "foobarbax").ToArray();
            Assert.AreEqual(7, delta.Length);
            Assert.AreEqual(7, delta.Where(x => x.Delta == ExtensionMethods.Delta.Join).Count());
        }

        [TestMethod]
        public void TestGetQuadKeysInRadius()
        {
            var keys = new Position { Latitude = 50, Longitude = 1 }.GetQuadKeysInRadius(1000).ToArray();
            Assert.AreEqual(9, keys.Length);

            var keys2 = new Position { Latitude = 50, Longitude = 1 }.GetQuadKeysInRadius(10000).ToArray();
            Assert.AreEqual(9, keys2.Length);
            Assert.AreNotEqual(keys[0], keys2[0]);

            Assert.IsTrue(keys[4].StartsWith(keys2[4]));
        }

        [TestMethod]
        public void TestDistanceBetweenPoints()
        {
            var position1 = new Position { Latitude = 50, Longitude = 1 };
            var position2 = new Position { Latitude = 51, Longitude = 2 };
            var distance = position1.DistanceTo(position2);

            // should be about 131 km
            Assert.AreEqual(131, Math.Floor(distance / 1000));


        }


    }
}
