using System;
using System.Collections;
using System.Collections.Generic;
using Drones.Utils;
using NUnit.Framework;

namespace Tests.External
{
    [TestFixture]
    public class PointTest
    {
        [Test]
        public void EqualityTest()
        {
            var a = new Point(1, 1, 1);

            Assert.True( a == new Point(1, 1, 1));
        }

        [Test]
        public void ArrayConstructor()
        {
            float[] arr = { 1, 1, 1 };
            var a = new Point(arr);

            Assert.True(a == new Point(1, 1, 1));
        }
    }
}
