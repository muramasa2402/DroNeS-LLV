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

        private float Sum(float[] nums)
        {
            float tot = 0;
            foreach (var i in nums)
            {
                tot += i;
            }
            return tot;
        }

        [Test]
        public void GetMaxOrGetMinOnlyReSortsWhenArgIsTrue()
        {
            var testSet2 = new SecureSortedSet<int, float[]>((float[] x, float[] y) => Sum(x) <= Sum(y) ? -1 : 1);
            float[] b = { 0.4f, 0.4f, 0.4f };
            float[] d = { 1.4f, 1.4f, 1.4f };
            testSet2.Add(1, new float[] { 0.1f, 0.1f, 0.1f });
            testSet2.Add(2, b);
            testSet2.Add(3, new float[] { 0.8f, 0.8f, 0.8f });
            testSet2.Add(4, d);

            Assert.AreSame(d, testSet2.GetMax(false));
            b[0] = 20;
            Assert.AreSame(b, testSet2.GetMax(true));
            testSet2.Add(2, b);
            b[0] = 0.01f;
            b[1] = 0.02f;
            b[2] = 0.02f;
            Assert.AreSame(b, testSet2.GetMin(true));
            testSet2.Add(2, b);
            b[0] = 200;
            Assert.AreSame(b, testSet2.GetMin(false));
        }

        [Test]
        public void PeekMaxAndPeekMinDoesNotRemoveElement()
        {
            testSet.Add(1, "a");
            testSet.Add(2, "b");
            testSet.Add(3, "c");
            testSet.Add(4, "d");
            testSet.ReSort((x, y) => StringComparer.Ordinal.Compare(x, y));
            Assert.AreEqual("d", testSet.PeekMax(false));
            Assert.AreEqual("d", testSet.PeekMax(false));
            Assert.AreEqual(4, testSet.Count);
            Assert.AreEqual("a", testSet.PeekMin(false));
            Assert.AreEqual("a", testSet.PeekMin(false));
            Assert.AreEqual(4, testSet.Count);

            testSet.Clear();
        }

        [Test]
        public void FindWillRetrieveTheFirstValueThatMathcesPredicateBasedOnOrderOfInsertion()
        {
            var testSet2 = new SecureSortedSet<int, float>();
            testSet2.Add(1, 33f);
            testSet2.Add(2, 54f);
            testSet2.Add(3, 55f);
            testSet2.Add(4, 15f);

            Assert.AreEqual(33f, testSet2.Find((x) => x < 50));
            Assert.AreEqual(54f, testSet2.Find((x) => x > 50));

            testSet2.Remove((int)1);

            Assert.AreEqual(15f, testSet2.Find((x) => x < 50));
        }

    }
}
