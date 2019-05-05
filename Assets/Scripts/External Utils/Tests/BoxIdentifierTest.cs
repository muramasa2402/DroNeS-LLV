using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NSubstitute;
using Drones.Utils;
using Drones.Interface;
using System;

namespace Tests.External
{
    [TestFixture]
    public class BoxIdentifierTest
    {
        private static readonly IClosestPoint unity = Substitute.For<IClosestPoint>();
        private const float EPSILON = 0.5f;

        private float[] RotateY(float deg, float[] v)
        {
            var rad = deg / 180 * Math.PI;
            var cos = (float)Math.Cos(rad);
            var sin = (float)Math.Sin(rad);

            return new float[] { v[0] * cos + v[2] * sin, v[1], v[0] * -sin + v[2] * cos };
        }
        [Test]
        public void BoxIdentifierBoundsTrapezoids()
        {
            float[] centre = { 0, 100, 0 };


            float[] v0 = RotateY(60, new float[] { 20, 100, 60 });
            float[] v1 = RotateY(60, new float[] { 20, 100, -20 });
            float[] v2 = RotateY(60, new float[] { -20, 100, -20 });
            float[] v3 = RotateY(60, new float[] { -20, 100, 30 });
            float[] h = { 0, 300, 0 };
            float[] l = { 0, 0, 0 };

            unity.GetCentre().Returns(centre);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.Up).Returns(h);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.Down).Returns(l);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.North).Returns(v3);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.South).Returns(v1);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.East).Returns(v0);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.West).Returns(v2);
            var id = new BoxIdentifier(unity);
            unity.Received(1).GetCentre();
            unity.Received(6).GetClosestPoint(Arg.Any<float[]>(), Arg.Any<Directions>());
            v1[1] = 0;
            v2[1] = 0;
            Assert.True(id.Start == v1 || id.Start == v2);
            Assert.True(Math.Abs(id.Length - 80) < 1 && Math.Abs(id.Width - 40) < 1 || Math.Abs(id.Length - 40) < 1 &&
                Math.Abs(id.Width - 80) < 1);
        }

        [Test]
        public void BoxIdentifierBoundsRectangles()
        {
            float[] centre = { 0, 100, 0 };


            float[] v0 = RotateY(60, new float[] { 20, 100, 25 });
            float[] v1 = RotateY(60, new float[] { 20, 100, -25 });
            float[] v2 = RotateY(60, new float[] { -20, 100, -25 });
            float[] v3 = RotateY(60, new float[] { -20, 100, 25 });
            float[] h = { 0, 300, 0 };
            float[] l = { 0, 0, 0 };

            unity.GetCentre().Returns(centre);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.Up).Returns(h);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.Down).Returns(l);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.North).Returns(v3);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.South).Returns(v1);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.East).Returns(v0);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.West).Returns(v2);

            var id = new BoxIdentifier(unity);
            v1[1] = 0;
            v2[1] = 0;
            v3[1] = 0;
            v0[1] = 0;
            Assert.True(id.Start == v1 || id.Start == v2 || id.Start == v0 || id.Start == v3);
            Assert.True(Math.Abs(id.Length - 40) < 1 && Math.Abs(id.Width - 50) < 1 || Math.Abs(id.Length - 50) < 1 &&
                Math.Abs(id.Width - 40) < 1);
        }

        [Test]
        public void BoxIdentifierBoundsAxisAlignedRectangles()
        {
            float[] centre = { 0, 100, 0 };


            float[] v0 = RotateY(0, new float[] { 20, 100, 25 });
            float[] v1 = RotateY(0, new float[] { 20, 100, -25 });
            float[] v2 = RotateY(0, new float[] { -20, 100, -25 });
            float[] v3 = RotateY(0, new float[] { -20, 100, 25 });
            float[] h = { 0, 300, 0 };
            float[] l = { 0, 0, 0 };

            unity.GetCentre().Returns(centre);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.Up).Returns(h);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.Down).Returns(l);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.North).Returns(v3);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.South).Returns(v1);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.East).Returns(v0);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.West).Returns(v2);

            var id = new BoxIdentifier(unity);
            v1[1] = 0;
            v2[1] = 0;
            v3[1] = 0;
            v0[1] = 0;
            Assert.True(id.Start == v1 || id.Start == v2 || id.Start == v0 || id.Start == v3);
            Assert.True(Math.Abs(id.Length - 40) < 1 && Math.Abs(id.Width - 50) < 1 || Math.Abs(id.Length - 50) < 1 &&
                Math.Abs(id.Width - 40) < 1);
        }

        [Test]
        public void BoxIdentifierBoundsIsoscelesTrapezoids()
        {
            float[] centre = { 0, 100, 0 };


            float[] v0 = RotateY(-30, new float[] { 10, 100, 25 });
            float[] v1 = RotateY(-30, new float[] { 20, 100, -15 });
            float[] v2 = RotateY(-30, new float[] { -20, 100, -15 });
            float[] v3 = RotateY(-30, new float[] { -10, 100, 25 });
            float[] h = { 0, 300, 0 };
            float[] l = { 0, 0, 0 };

            unity.GetCentre().Returns(centre);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.Up).Returns(h);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.Down).Returns(l);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.North).Returns(v3);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.South).Returns(v1);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.East).Returns(v0);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.West).Returns(v2);
            var id = new BoxIdentifier(unity);
            v2[1] = 0;
            v1[1] = 0;
            Assert.True(id.Start == v1 || id.Start == v2);
            Assert.True(Math.Abs(id.Length - 40) < 1 && Math.Abs(id.Width - 40) < 1);
        }

        [Test]
        public void IgnoresBuildingsThatAreTooSmall()
        {
            float[] centre = { 0, 100, 0 };


            float[] v0 = RotateY(-30, new float[] { 3, 100, 3 });
            float[] v1 = RotateY(-30, new float[] { 3, 100, -3 });
            float[] v2 = RotateY(-30, new float[] { -3, 100, -3 });
            float[] v3 = RotateY(-30, new float[] { -3, 100, 3 });
            float[] h = { 0, 7, 0 };
            float[] l = { 0, 0, 0 };

            unity.GetCentre().Returns(centre);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.Up).Returns(h);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.Down).Returns(l);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.North).Returns(v3);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.South).Returns(v1);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.East).Returns(v0);
            unity.GetClosestPoint(Arg.Any<float[]>(), Directions.West).Returns(v2);
            var id = new BoxIdentifier(unity);
            Assert.True(id.TooSmall);
            Assert.AreEqual(0, id.Length);
            Assert.AreEqual(0, id.Width);

        }


    }
}
