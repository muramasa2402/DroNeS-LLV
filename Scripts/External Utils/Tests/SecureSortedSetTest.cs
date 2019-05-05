using System;
using Drones.Utils;
using NUnit.Framework;

namespace Tests.External
{
    [TestFixture]
    public class SecureSortedSetTest
    {
        SecureSortedSet<int, string> testSet = new SecureSortedSet<int, string>();
        [Test]
        public void CanAddAndRemoveKeyAndValue()
        {
            testSet.Add(1, "a");
            testSet.Add(2, "b");
            testSet.Add(3, "c");
            Assert.AreEqual(3, testSet.Count);
            testSet.Remove("b");
            Assert.AreEqual(2, testSet.Count);
            testSet.Remove(3);
            Assert.AreEqual(1, testSet.Count);
            testSet.Clear();
            Assert.AreEqual(0, testSet.Count);
        }

        [Test]
        public void ReturnsFalseWhenSameKeyOrValueAdded()
        {
            testSet.Add(1, "a");
            Assert.False(testSet.Add(1, "b"));
            Assert.False(testSet.Add(2, "a"));
            testSet.Clear();
        }

        [Test]
        public void IndexableFromEitherKeyOrValue()
        {
            testSet.Add(1, "a");
            Assert.AreEqual(1, testSet["a"]);
            Assert.AreEqual("a", testSet[1]);
            testSet.Clear();
        }

        [Test]
        public void ContainmentCheckCanBeDoneOnBothKeyOrValue()
        {
            testSet.Add(1, "a");
            Assert.True(testSet.Contains(1));
            Assert.True(testSet.Contains("a"));
            Assert.False(testSet.Contains("b"));
            Assert.False(testSet.Contains(2));
            testSet.Clear();
        }

        [Test]
        public void ValuesCanBeCountedWithPredicate()
        {
            testSet.Add(1, "a");
            testSet.Add(2, "b");
            testSet.Add(3, "c");
            testSet.Add(4, "d");
            Assert.AreEqual(3, testSet.CountWhere((s) => s != "b"));

            testSet.Clear();
        }

        [Test]
        public void GetMaxAndGetMinLIFOIfNoComparerSupplied()
        {
            testSet.Add(1, "a");
            testSet.Add(2, "b");
            testSet.Add(3, "c");
            testSet.Add(4, "d");
            Assert.AreEqual("d", testSet.GetMax(false));
            Assert.AreEqual("c", testSet.GetMax(false));
            testSet.Clear();
        }

        [Test]
        public void GetMaxAndGetMinGetsMaximumAndMinimumOfValueWhenReSortedWithComparer()
        {
            testSet.Add(1, "a");
            testSet.Add(2, "b");
            testSet.Add(3, "c");
            testSet.Add(4, "d");
            testSet.ReSort((x, y) => StringComparer.Ordinal.Compare(x, y));
            Assert.AreEqual("d", testSet.GetMax(false));
            Assert.AreEqual("a", testSet.GetMin(false));
            testSet.Clear();
            testSet = new SecureSortedSet<int, string>();
        }

        [Test]
        public void GetMaxOrGetMinOnlyReSortsWhenArgIsTrue()
        {
            var testSet2 = new SecureSortedSet<int, Point>((Point x, Point y) =>
            {
                if (Point.Distance(new Point(), x) <= Point.Distance(new Point(), y)) return -1;
                return 1;
            });
            var b = new Point(0.4f, 0.4f, 0.4f);
            var d = new Point(1.4f, 1.4f, 1.4f);
            testSet2.Add(1, new Point(0.1f, 0.1f, 0.1f));
            testSet2.Add(2, b);
            testSet2.Add(3, new Point(0.8f, 0.8f, 0.8f));
            testSet2.Add(4, d);

            Assert.AreSame(d, testSet2.GetMax(false));

            b.x = 20;
            b.y = 20;
            b.z = 20;
            Assert.AreSame(b, testSet2.GetMax(true));
            testSet2.Add(2, b);
            b.x = 0.01f;
            b.y = 0.02f;
            b.z = 0.02f;
            Assert.AreSame(b, testSet2.GetMin(true));
            testSet2.Add(2, b);
            b.x = 20;
            b.y = 20;
            b.z = 20;
            Assert.AreSame(b, testSet2.GetMin(false));
        }


    }
}
