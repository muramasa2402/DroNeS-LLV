using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.External
{
 
    using Drones.Utils;
    [TestFixture]
    public class MapTest
    {

        [Test]
        public void AddingAndRemovingChangesCount()
        {
        Map<int, string> map = new Map<int, string>();
        Assert.AreEqual(0, map.Count);

        map.Add(1, "one");
        Assert.AreEqual(1, map.Count);

        map.Add(2, "two");
        Assert.AreEqual(2, map.Count);

        map.Remove(1);
        Assert.AreEqual(1, map.Count);

        map.Remove(2);
        Assert.AreEqual(0, map.Count);
        }

        [Test]
        public void ClearResetsMapToEmptyState()
        {
            Map<int, string> map = new Map<int, string>();
            Assert.AreEqual(0, map.Count);

            map.Add(1, "one");
            map.Add(2, "two");

            map.Clear();
            Assert.AreEqual(0, map.Count);
        }

        [Test]
        public void ContainsReturnsTrueIfElementIsInTheMap()
        {
            Map<int, string> map = new Map<int, string>();
           
            map.Add(1, "one");
            map.Add(2, "two");

            Assert.True(map.Contains(1));
            Assert.True(map.Contains("one"));
            Assert.False(map.Contains(3));
            Assert.False(map.Contains("three"));
        }
        [Test]
        public void RemoveWorksOnBothKeyAndValue()
        {
            Map<int, string> map = new Map<int, string>
            {
                { 1, "one" },
                { 2, "two" }
            };
            map.Remove(1);
            map.Remove("two");

            Assert.False(map.Contains(1));
            Assert.False(map.Contains(2));
        }

        [Test]
        public void MapCanBeIteratedUsingForEach()
        {
            Map<int, string> map = new Map<int, string>
            {
                { 1, "one" },
                { 2, "two" }
            };
            int i = 0;
            foreach (var kvPair in map)
            {
                if (i++ == 0)
                {
                    Assert.AreEqual("one", kvPair.Value);
                    Assert.AreEqual(1, kvPair.Key);
                }
                else
                {
                    Assert.AreEqual("two", kvPair.Value);
                    Assert.AreEqual(2, kvPair.Key);
                }
            }
        }


      [Test]
      public void ForwardReturnsStringValue()
      {
        Map<int, string> map = new Map<int, string>();

        map.Add(1, "one");
        map.Add(2, "two");
        map.Add(3, "three");

        Assert.AreEqual(map.Forward[1],"one");
        Assert.AreEqual(map.Forward[2],"two");
        Assert.AreEqual(map.Forward[3], "three");
      }

      [Test]
      public void ReverseReturnsIntValue()
      {
        Map<int, string> map = new Map<int, string>();

        map.Add(1, "one");
        map.Add(2, "two");
        map.Add(3, "three");

        Assert.AreEqual(map.Reverse["one"], 1);
        Assert.AreEqual(map.Reverse["two"], 2);
        Assert.AreEqual(map.Reverse["three"],3);
      }
    }
}
