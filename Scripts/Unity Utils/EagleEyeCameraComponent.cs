using System.Collections;

using UnityEngine;

namespace Drones.UI
{
    using Drones.Interface;
    using static Singletons;
    public class EagleEyeCameraComponent : AbstractCamera
    {

        void OnEnable()
        {
            var v = RTS.transform.position;
            ActiveCamera = this;
            v.y = 200;
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

            if (UIFocus.hover == 0 && Input.GetMouseButton(0) && !UIFocus.Controlling)
            {
                if (!Controlling)
                {
                    StartCoroutine(ControlListener());
                }
                Controller.Roll(Input.GetAxis("Mouse X"));
                Controller.ClampPitch();
            }

            if (!_Following && Followee != null) { StartCoroutine(FollowObject()); }
        }

        protected override IEnumerator FollowObject()
        {
            _Following = true;
            while (!Input.GetKeyDown(KeyCode.Escape))
            {
                var v = Followee.transform.position;
                v.y = 200;
                transform.position = v;
                yield return null;
            }
            _Following = false;
            yield break;
        }

        #region ICameraMovement
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
