using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Drones;

namespace Tests.Unity
{
    public class CameraDirectionalMovementTest : IPrebuildSetup
    {
        private GameObject container;
        private Camera cam;
        private RTSCameraComponent script;
        [SetUp]
        public void Setup()
        {
            container = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            container.name = "RTSCamera";
            script = container.AddComponent<RTSCameraComponent>();
            GameObject cameraObject = new GameObject
            {
                name = "Camera"
            };
            cam = cameraObject.AddComponent<Camera>();
            cameraObject.transform.SetParent(container.transform);
            container.transform.Rotate(Vector3.right * 60);
        }

        [UnityTest]
        public IEnumerator CameraMovesForwardHorizontallyWithPositiveVerticalInput()
        {
            Vector3 original = container.transform.position;
            Vector3 expectedDirection = Vector3.Cross(cam.transform.right, Vector3.up).normalized;
            script.MoveLongitudinal(1);
            Vector3 newPos = container.transform.position;
            Vector3 actualDirection = (newPos - original).normalized;

            Assert.AreEqual(expectedDirection, actualDirection);
            yield break;
        }

        [UnityTest]
        public IEnumerator CameraMovesBackwardHorizontallyWithNegativeVerticalInput()
        {
            Vector3 original = container.transform.position;
            Vector3 expectedDirection = Vector3.Cross(Vector3.up, cam.transform.right).normalized;
            script.MoveLongitudinal(-1);
            Vector3 newPos = container.transform.position;
            Vector3 actualDirection = (newPos - original).normalized;

            Assert.AreEqual(expectedDirection, actualDirection);
            yield break;
        }

        [UnityTest]
        public IEnumerator CameraMovesRightWithPositiveHorizontalInput()
        {
            Vector3 original = container.transform.position;
            Vector3 expectedDirection = cam.transform.right;
            script.MoveLateral(1);
            Vector3 newPos = container.transform.position;
            Vector3 actualDirection = (newPos - original).normalized;

            Assert.AreEqual(expectedDirection, actualDirection);
            yield break;
        }

        [UnityTest]
        public IEnumerator CameraMovesLeftWithNegativeHorizontalInput()
        {
            Vector3 original = container.transform.position;
            Vector3 expectedDirection = -cam.transform.right;
            script.MoveLateral(-1);
            Vector3 newPos = container.transform.position;
            Vector3 actualDirection = (newPos - original).normalized;

            Assert.AreEqual(expectedDirection, actualDirection);
            yield break;
        }

        [UnityTest]
        public IEnumerator CameraMovesForwardWithPositiveZoomInput()
        {
            Vector3 original = container.transform.position;
            Vector3 expectedDirection = cam.transform.forward;
            script.Zoom(1);
            Vector3 newPos = container.transform.position;
            Vector3 actualDirection = (newPos - original).normalized;

            Assert.AreEqual(expectedDirection, actualDirection);
            yield break;
        }

        [UnityTest]
        public IEnumerator CameraMovesBackwardWithNegativeZoomInput()
        {
            Vector3 original = container.transform.position;
            Vector3 expectedDirection = -cam.transform.forward;
            script.Zoom(-1);
            Vector3 newPos = container.transform.position;
            Vector3 actualDirection = (newPos - original).normalized;

            Assert.AreEqual(expectedDirection, actualDirection);
            yield break;
        }


    }

}
