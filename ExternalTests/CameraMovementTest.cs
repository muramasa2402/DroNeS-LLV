using NUnit.Framework;
using NSubstitute;
using System;

namespace Tests.External
{
    [TestFixture]
    public class CameraMovementTest
    {
        private static RTSCameraController GetController(ICameraMovement movement)
        {
            return new RTSCameraController(movement);
        }
        private static ICameraMovement GetMovementMock()
        {
            return Substitute.For<ICameraMovement>();
        }
        static Random rnd = new Random();

        [Test]
        public void CameraControllerExectutesLongitudinalMovementCommandWithTheCorrectSpeedMultiplier()
        {
            var movement = GetMovementMock();
            var controller = GetController(movement);
            float speed = rnd.Next(int.MinValue, int.MaxValue);
            controller.MoveSpeed = speed;
            Assert.AreEqual(speed, controller.MoveSpeed);
            float input = rnd.Next(int.MinValue, int.MaxValue);
            controller.MoveLongitudinal(input);
            movement.Received().MoveLongitudinal(Arg.Is(input * controller.MoveSpeed));
        }

        [Test]
        public void CameraControllerExectutesLateralMovementCommandWithTheCorrectSpeedMultiplier()
        {
            var movement = GetMovementMock();
            var controller = GetController(movement);
            float speed = rnd.Next(int.MinValue, int.MaxValue);
            controller.MoveSpeed = speed;
            Assert.AreEqual(speed, controller.MoveSpeed);
            float input = rnd.Next(int.MinValue, int.MaxValue);
            controller.MoveLateral(input);
            movement.Received().MoveLateral(Arg.Is(input * controller.MoveSpeed));
        }

        [Test]
        public void CameraControllerExectutesZoomCommandWithTheCorrectSpeedMultiplier()
        {
            var movement = GetMovementMock();
            var controller = GetController(movement);
            float speed = rnd.Next(int.MinValue, int.MaxValue);
            controller.ZoomSpeed = speed;
            Assert.AreEqual(speed, controller.ZoomSpeed);
            float input = rnd.Next(int.MinValue, int.MaxValue);
            controller.Zoom(input);
            movement.Received().Zoom(Arg.Is(input * controller.ZoomSpeed));
        }

        [Test]
        public void CameraControllerExecutesPitchWithSpecifiedMouseSensitivityWithoutInvertedAxis()
        {
            var movement = GetMovementMock();
            var controller = GetController(movement);
            float input = rnd.Next(int.MinValue, int.MaxValue);
            float sensitive = rnd.Next(int.MinValue, int.MaxValue);
            controller.MouseSensitivity = sensitive;
            Assert.AreEqual(sensitive, controller.MouseSensitivity);
            Assert.False(controller.InvertYAxis);
            controller.Pitch(input);
            movement.Received().Pitch(Arg.Is(-input * controller.MouseSensitivity));
        }

        [Test]
        public void CameraControllerExecutesPitchWithSpecifiedMouseSensitivityWithInvertedAxis()
        {
            var movement = GetMovementMock();
            var controller = GetController(movement);
            float input = rnd.Next(int.MinValue, int.MaxValue);
            float sensitive = rnd.Next(int.MinValue, int.MaxValue);
            controller.MouseSensitivity = sensitive;
            Assert.AreEqual(sensitive, controller.MouseSensitivity);
            controller.InvertYAxis = true;
            Assert.True(controller.InvertYAxis);
            controller.Pitch(input);
            movement.Received().Pitch(Arg.Is(input * controller.MouseSensitivity));
        }

        [Test]
        public void CameraControllerExecutesYawMotionWithSpecifiedMouseSensitivity()
        {
            var movement = GetMovementMock();
            var controller = GetController(movement);
            float input = rnd.Next(int.MinValue, int.MaxValue);
            float sensitive = rnd.Next(int.MinValue, int.MaxValue);
            controller.MouseSensitivity = sensitive;
            Assert.AreEqual(sensitive, controller.MouseSensitivity);
            controller.Yaw(input);
            movement.Received().Yaw(Arg.Is(input * controller.MouseSensitivity));
        }

        [Test]
        public void CameraControllerExecutesRotationWithSpecifiedRotationSpeed()
        {
            var movement = GetMovementMock();
            var controller = GetController(movement);
            float input = rnd.Next(int.MinValue, int.MaxValue);
            float speed = rnd.Next(int.MinValue, int.MaxValue);
            controller.RotationSpeed = speed;
            Assert.AreEqual(speed, controller.RotationSpeed);
            controller.Rotate(input);
            movement.Received().Rotate(Arg.Is(input * controller.RotationSpeed));
        }

        [Test]
        public void CameraControllerLimitsTheVerticalHeight()
        {
            var movement = GetMovementMock();
            var controller = GetController(movement);
            float lowerBound = rnd.Next(int.MinValue, int.MaxValue / 2);
            float upperBound = rnd.Next(int.MaxValue / 2, int.MaxValue);
            controller.Floor = lowerBound;
            controller.Ceiling = upperBound;
            Assert.AreEqual(lowerBound, controller.Floor);
            Assert.AreEqual(upperBound, controller.Ceiling);
            controller.ClampVertical();
            movement.Received().ClampVertical(Arg.Is(controller.Floor), Arg.Is(controller.Ceiling));
        }

        [Test]
        public void CameraControllerLimitsThePitchAngle()
        {
            var movement = GetMovementMock();
            var controller = GetController(movement);
            float lowerBound = rnd.Next(0, 90);
            float upperBound = rnd.Next(0, 90);
            controller.LowerPitch = lowerBound;
            controller.UpperPitch = upperBound;
            Assert.AreEqual(lowerBound, controller.LowerPitch);
            Assert.AreEqual(upperBound, controller.UpperPitch);
            controller.ClampPitch();
            movement.Received().ClampPitch(Arg.Is(controller.LowerPitch), Arg.Is(controller.UpperPitch));
        }

        [Test]
        public void CameraPitchBoundsCorrectsTo90DegreesIfObtuseAnglesAssigned()
        {
            var movement = GetMovementMock();
            var controller = GetController(movement);
            float obtuseBound = rnd.Next(91, int.MaxValue);
            controller.UpperPitch = obtuseBound;
            controller.LowerPitch = obtuseBound;
            Assert.AreEqual(90, controller.UpperPitch);
            Assert.AreEqual(90, controller.LowerPitch);
        }

        [Test]
        public void CameraPitchBoundsCorrectsTo0DegreesIfNegativeValuesAssigned()
        {
            var movement = GetMovementMock();
            var controller = GetController(movement);
            float negativeBound = rnd.Next(int.MinValue, -1);
            controller.UpperPitch = negativeBound;
            controller.LowerPitch = negativeBound;
            Assert.AreEqual(0, controller.UpperPitch);
        }
    }

}