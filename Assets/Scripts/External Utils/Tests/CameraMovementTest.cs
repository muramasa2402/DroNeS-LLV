using NUnit.Framework;
using NSubstitute;
using System;
using Drones.Interface;
using Drones;

namespace Tests.External
{
    [TestFixture]
    public class CameraMovementTest
    {
        static readonly Random rnd = new Random();
        private static readonly ICameraMovement movement = Substitute.For<ICameraMovement>();
        private static readonly float speed = rnd.Next(0, 100);
        private static readonly float input = rnd.Next(-1, 1);
        private static readonly float sensitive = rnd.Next(0, 100);

        [Test]
        public void LongitudinalMovementExecutedWithTheCorrectSpeedMultiplier()
        {
            CameraController controller = new CameraController(movement)
            {
                MoveSpeed = speed
            };
            Assert.AreEqual(speed, controller.MoveSpeed);
            controller.MoveLongitudinal(input);

            movement.Received().MoveLongitudinal(Arg.Is(input * controller.MoveSpeed));
        }

        [Test]
        public void LateralMovementExecutedWithTheCorrectSpeedMultiplier()
        {
            CameraController controller = new CameraController(movement)
            {
                MoveSpeed = speed
            };
            Assert.AreEqual(speed, controller.MoveSpeed);
            controller.MoveLateral(input);
            movement.Received().MoveLateral(Arg.Is(input * controller.MoveSpeed));
        }

        [Test]
        public void ZoomExecutedWithTheCorrectSpeedMultiplier()
        {
            CameraController controller = new CameraController(movement)
            {
                ZoomSpeed = speed
            };
            Assert.AreEqual(speed, controller.ZoomSpeed);
            controller.Zoom(input);
            movement.Received().Zoom(Arg.Is(input * controller.ZoomSpeed));
        }

        [Test]
        public void InvertedPitchExecutedWithSpecifiedMouseSensitivity()
        {
            CameraController controller = new CameraController(movement)
            {
                MouseSensitivity = sensitive
            };
            Assert.AreEqual(sensitive, controller.MouseSensitivity);
            controller.InvertYAxis = false;
            Assert.False(controller.InvertYAxis);
            controller.Pitch(input);
            movement.Received().Pitch(Arg.Is(-input * controller.MouseSensitivity));
        }

        [Test]
        public void NormalPitchExecutedWithSpecifiedMouseSensitivity()
        {
            CameraController controller = new CameraController(movement)
            {
                MouseSensitivity = sensitive
            };
            Assert.AreEqual(sensitive, controller.MouseSensitivity);
            controller.InvertYAxis = true;
            Assert.True(controller.InvertYAxis);
            controller.Pitch(input);
            movement.Received().Pitch(Arg.Is(input * controller.MouseSensitivity));
        }

        [Test]
        public void YawMotionExecutedWithSpecifiedMouseSensitivity()
        {
            CameraController controller = new CameraController(movement)
            {
                MouseSensitivity = sensitive
            };
            Assert.AreEqual(sensitive, controller.MouseSensitivity);
            controller.Yaw(input);
            movement.Received().Yaw(Arg.Is(input * controller.MouseSensitivity));
        }

        [Test]
        public void RotationExecutedWithSpecifiedRotationSpeed()
        {
            CameraController controller = new CameraController(movement)
            {
                RotationSpeed = speed
            };
            Assert.AreEqual(speed, controller.RotationSpeed);
            controller.Rotate(input);
            movement.Received().Rotate(Arg.Is(input * controller.RotationSpeed));
        }

        [Test]
        public void CameraControllerLimitsTheVerticalHeight()
        {
            float lowerBound = rnd.Next(0, int.MaxValue / 2);
            float upperBound = rnd.Next(int.MaxValue / 2, int.MaxValue);
            CameraController controller = new CameraController(movement)
            {
                Ceiling = upperBound,
                Floor = lowerBound
            };
            Assert.AreEqual(lowerBound, controller.Floor);
            Assert.AreEqual(upperBound, controller.Ceiling);
            controller.ClampVertical();
            movement.Received().ClampVertical(Arg.Is(controller.Floor), Arg.Is(controller.Ceiling));
        }

        [Test]
        public void CameraControllerLimitsThePitchAngle()
        {
            float lowerBound = 45;
            float upperBound = 45;
            CameraController controller = new CameraController(movement)
            {
                LowerPitch = lowerBound,
                UpperPitch = upperBound
            };
            Assert.AreEqual(lowerBound, controller.LowerPitch);
            Assert.AreEqual(upperBound, controller.UpperPitch);
            controller.ClampPitch();
            movement.Received().ClampPitch(Arg.Is(controller.LowerPitch), Arg.Is(controller.UpperPitch));
        }

        [Test]
        public void CameraPitchBoundsCannotExceed90Degrees()
        {
            float obtuseUpperBound = rnd.Next(91, int.MaxValue);
            float obtuseLowerBound = rnd.Next(int.MinValue, -91);
            CameraController controller = new CameraController(movement)
            {
                UpperPitch = obtuseUpperBound
            };
            Assert.AreEqual(90, controller.UpperPitch);
            controller = new CameraController(movement)
            {
                LowerPitch = obtuseUpperBound
            };
            Assert.AreEqual(90, controller.LowerPitch);


            controller = new CameraController(movement)
            {
                UpperPitch = obtuseLowerBound
            };
            Assert.AreEqual(-90, controller.UpperPitch);
            controller = new CameraController(movement)
            {
                LowerPitch = obtuseLowerBound
            };
            Assert.AreEqual(-90, controller.LowerPitch);
        }
    
        [Test]
        public void ControllerPorpertiesAreAlwaysPositive()
        {
            float neg = -1;
            CameraController controller = new CameraController(movement)
            {
                MoveSpeed = neg,
                RotationSpeed = neg,
                MouseSensitivity = neg,
                ZoomSpeed = neg,
                SpeedToHeightGradient = neg,
                Floor = neg,
                Ceiling = neg
            };
            Assert.False(controller.MoveSpeed < 0);
            Assert.False(controller.RotationSpeed < 0);
            Assert.False(controller.MouseSensitivity < 0);
            Assert.False(controller.ZoomSpeed < 0);
            Assert.False(controller.SpeedToHeightGradient < 0);
            Assert.False(controller.Floor < 0);
            Assert.False(controller.Ceiling < 0);
        }

        [Test]
        public void FloorIsAlwaysGreaterOrEqualsToCeiling()
        {
            CameraController controller = new CameraController(movement);
            controller.Floor = controller.Ceiling + 1;
            Assert.False(controller.Floor > controller.Ceiling);
            controller.Floor = 50;
            controller.Ceiling = controller.Floor - 1;
            Assert.False(controller.Floor > controller.Ceiling);
        }
    }

}