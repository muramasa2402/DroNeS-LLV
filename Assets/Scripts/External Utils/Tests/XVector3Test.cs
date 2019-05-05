using System;
using System.Collections.Generic;
using Drones.Utils;
using NUnit.Framework;

namespace Tests.External
{
    [TestFixture]
    public class XVector3Test
    {
        XVector3 a = new XVector3(1, 1, 1);
        [Test]
        public void EqualityTest()
        {
            Assert.True( a == new XVector3(1, 1, 1));
            Assert.False(a != new XVector3(1.00000005f, 1.00000005f, 1.00000005f));
        }

        [Test]
        public void ArrayConstructor()
        {
            float[] arr = { 1, 1, 1 };
            var b = new XVector3(arr);

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
        public void PointAdditionTest()
        {
            var result = new XVector3(3, 3, 3);

            Assert.True(result == a + a + a);
        }

        [Test]
        public void PointSubtractionTest()
        {
            var result = new XVector3(-1, -1, -1);

            Assert.True(result == a - a - a);
        }

        [Test]
        public void ScalarMultiplicationTest()
        {
            var result = new XVector3(-2, -2, -2);

            Assert.True(result.x * a == result);
            Assert.True(a * result.x == result);
        }

        [Test]
        public void ScalarDivisionOnlyOnRightSide()
        {
            var result = new XVector3(0.5f, 0.5f, 0.5f);

            Assert.True(a / 2 == result);
        }

        [Test]
        public void DotProductTest()
        {
            var b = new XVector3(1, 2, 3);

            Assert.AreEqual(6, XVector3.Dot(b, a));
        }

        [Test]
        public void NormalizeReturnsUnitVector()
        {
            var result = new XVector3(1 / (float)Math.Sqrt(3), 1 / (float)Math.Sqrt(3), 1 / (float)Math.Sqrt(3));

            Assert.True(result == a.normalized);
            Assert.True(result == XVector3.Normalize(a));
        }

        [Test]
        public void DistanceReturnsTheCartesianDistance()
        {
            Assert.AreEqual((float)Math.Sqrt(3), XVector3.Distance(new XVector3(), a));
        }

        [Test]
        public void Hashable()
        {
            HashSet<XVector3> hs = new HashSet<XVector3>
            {
                a,
                new XVector3(5, 3, 5),
                new XVector3(5, 3, 5)
            };

            Assert.AreEqual(2, hs.Count);
            hs.Add(new XVector3(5, 3.1f, 5));
            Assert.AreEqual(3, hs.Count);
        }


    }
}
