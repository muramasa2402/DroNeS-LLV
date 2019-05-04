using System;
using System.Collections.Generic;
using Drones.Utils;
using NUnit.Framework;

namespace Tests.External
{
    [TestFixture]
    public class PointTest
    {
        Point a = new Point(1, 1, 1);
        [Test]
        public void EqualityTest()
        {
            Assert.True( a == new Point(1, 1, 1));
            Assert.False(a != new Point(1, 1, 1));
            Point b = null;
            Point c = null;
            Assert.True(b == c);
            Assert.False(a == c);
        }

        [Test]
        public void ArrayConstructor()
        {
            float[] arr = { 1, 1, 1 };
            var b = new Point(arr);

            Assert.True(b == a);
        }

        [Test]
        public void ConvertsToArray()
        {
            float[] arr = a.ToArray();
            for (int i = 0; i < arr.Length; i++)
                Assert.AreEqual(1, arr[i]);
        }

        [Test]
        public void CanBeCloned()
        {
            Assert.True(a == a.Clone());
        }

        [Test]
        public void PointAdditionTest()
        {
            var result = new Point(3, 3, 3);

            Assert.True(result == a + a + a);
        }

        [Test]
        public void PointSubtractionTest()
        {
            var result = new Point(-1, -1, -1);

            Assert.True(result == a - a - a);
        }

        [Test]
        public void ScalarMultiplicationTest()
        {
            var result = new Point(-2, -2, -2);

            Assert.True(result.x * a == result);
            Assert.True(a * result.x == result);
        }

        [Test]
        public void ScalarDivisionOnlyOnRightSide()
        {
            var result = new Point(0.5f, 0.5f, 0.5f);

            Assert.True(a / 2 == result);
        }

        [Test]
        public void DotProductTest()
        {
            var b = new Point(1, 2, 3);

            Assert.AreEqual(6, Point.Dot(new Point(), b, a));
        }

        [Test]
        public void NormalizeReturnsPointOneUnitAwayFromSuppliedOrigin()
        {
            var result = new Point(1 / (float)Math.Sqrt(3), 1 / (float)Math.Sqrt(3), 1 / (float)Math.Sqrt(3));

            Assert.True(result == Point.Normalize(new Point(), a));
        }

        [Test]
        public void DistanceReturnsTheCartesianDistance()
        {
            Assert.AreEqual((float)Math.Sqrt(3), Point.Distance(new Point(), a));
        }

        [Test]
        public void Hashable()
        {
            HashSet<Point> hs = new HashSet<Point>();
            hs.Add(a);
            hs.Add(new Point(5, 3, 5));
            hs.Add(new Point(5, 3, 5));
            Assert.AreEqual(2, hs.Count);
            hs.Add(new Point(5, 3.1f, 5));
            Assert.AreEqual(3, hs.Count);
        }


    }
}
