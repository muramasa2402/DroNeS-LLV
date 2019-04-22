using System.Collections;

using UnityEngine;

namespace Drones.UI
{
    using static Singletons;
    public class EagleEyeCameraComponent : AbstractCamera
    {
        private static float _DefaultSize;
        private readonly float _DefaultHeight = 600;
        private void Awake()
        {
            _DefaultSize = CameraComponent.orthographicSize;
        }

        void OnEnable()
        {
            CameraComponent.orthographicSize = _DefaultSize;
            var v = RTS.transform.position;
            ActiveCamera = this;
            v.y = _DefaultHeight;
            transform.position = v;
            if (Followee != null)
            {
                StartCoroutine(FollowObject());
            }
        }

        private void Update()
        {
            Controller.MoveLongitudinal(Input.GetAxis("Vertical") * SpeedScale);
            Controller.MoveLateral(Input.GetAxis("Horizontal") * SpeedScale);
            Controller.Rotate(Input.GetAxis("Rotate"));

            if (UIFocus.hover == 0 && !UIFocus.Controlling)
            {
                Controller.Zoom(Input.GetAxis("Mouse ScrollWheel"));
                if (Input.GetMouseButton(0))
                {
                    if (!Controlling)
                    {
                        StartCoroutine(ControlListener());
                    }
                    Controller.Roll(Input.GetAxis("Mouse X"));
                    Controller.ClampPitch();
                }
            }

            if (!_Following && Followee != null) { StartCoroutine(FollowObject()); }
        }

        protected override IEnumerator FollowObject()
        {
            var wait = new WaitForEndOfFrame();
            _Following = true;
            while (!Input.GetKeyDown(KeyCode.Escape))
            {
                var v = Followee.transform.position;
                v.y = _DefaultHeight;
                transform.position = v;
                yield return wait;
                v = Followee.transform.position;
                v.y = _DefaultHeight;
                transform.position = v;
                yield return null;

            }
            _Following = false;
            Followee = null;
            yield break;
        }

        #region ICameraMovement
        public override void Zoom(float input)
        {
            CameraComponent.orthographicSize -= Controller.ZoomSpeed * 10 * input;
            CameraComponent.orthographicSize = Mathf.Clamp(CameraComponent.orthographicSize, 0, 4000f);
        }

        public override void Roll(float input)
        {
            transform.Rotate(0, input, 0, Space.World);
        }

        public override void Rotate(float input)
        {
            Roll(input);
        }
        #endregion

    }
}
