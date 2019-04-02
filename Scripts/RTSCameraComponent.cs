using UnityEngine;
using System.Collections;
using Drones.Interface;

namespace Drones
{
    using UI;
    using static Singletons;
    public class RTSCameraComponent : MonoBehaviour, ICameraMovement
    {
        private RTSCameraController _Controller;

        public RTSCameraController Controller 
        {
            get
            {
                if (_Controller == null)
                {
                    _Controller = new RTSCameraController(this);
                }
                if (_Controller.Move == null) 
                { 
                    _Controller.Move = this; 
                }
                return _Controller;
            }
        }

        public GameObject Followee { get; set; }

        public float followedDistance = 3f;

        private void Update()
        {
            float speedScale = Controller.SpeedToHeightGradient * transform.position.y + 1;
            Controller.MoveLongitudinal(Input.GetAxis("Vertical") * speedScale);
            Controller.MoveLateral(Input.GetAxis("Horizontal") * speedScale);
            Controller.Rotate(Input.GetAxis("Rotate"));

            if (UIFocus.hover == 0)
            {
                Controller.Zoom(Input.GetAxis("Mouse ScrollWheel") * speedScale);
                //FPS mouse hold click
                if (Input.GetMouseButton(0) && !UIFocus.Controlling)
                {
                    if (!Controlling) 
                    {
                        StartCoroutine(ControlListener()); 
                    }
                    Controller.Pitch(Input.GetAxis("Mouse Y"));
                    Controller.Yaw(Input.GetAxis("Mouse X"));
                    Controller.ClampPitch();
                }
            }

            // Bounds
            Controller.ClampVertical();

            if (Followee != null && Input.GetKey(KeyCode.Space)) { StartCoroutine(FollowObject()); }
        }

        public bool Controlling { get; private set; }

        IEnumerator ControlListener()
        {
            Controlling = true;
            do
            {
                yield return null;
            } while (!Input.GetMouseButtonUp(0));
            Controlling = false;
            yield break;
        }

        IEnumerator FollowObject()
        {
            while (!Input.GetKeyDown(KeyCode.Escape))
            {
                transform.position = Followee.transform.position - CamTrans.forward * followedDistance;
                yield return null;
            }
            yield break;
        }

        IEnumerator StopCamera(Collision collision)
        {
            Vector3 previousPosition = transform.position;
            Vector3 motion;
            ContactPoint[] contacts = new ContactPoint[32];
            yield return new WaitForEndOfFrame();
            while (true)
            {
                motion = transform.position - previousPosition;
                int num = collision.GetContacts(contacts);
                for (int i = 0; i < num; i++)
                {
                    float dot = Vector3.Dot(motion, contacts[i].normal.normalized);
                    if (dot < 0)
                    {
                        transform.position -= dot * contacts[i].normal.normalized;
                    }
                }
                previousPosition = transform.position;
                yield return null;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            StartCoroutine(StopCamera(collision));
        }

        private void OnCollisionExit(Collision collision)
        {
            StopCoroutine(StopCamera(collision));
        }

        #region ICameraMovement Implementation

        public void MoveLongitudinal(float longitudinalInput)
        {
            var positiveDirection = Vector3.Cross(CamTrans.right, Vector3.up).normalized;

            transform.position += longitudinalInput * positiveDirection * Time.deltaTime;
        }

        public void MoveLateral(float lateralInput)
        {
            var positiveDirection = CamTrans.right;

            transform.position += lateralInput * positiveDirection * Time.deltaTime;
        }

        public void Zoom(float zoomInput)
        {
            Vector3 positiveDirection = CamTrans.forward;
            // Cannot zoom when facing up
            if (positiveDirection.y < 0)
            {
                transform.position += zoomInput * positiveDirection * Time.deltaTime;
            }
        }

        public void Pitch(float pitchInput)
        {
            transform.Rotate(pitchInput, 0, 0);
        }

        public void Yaw(float yawInput)
        {
            transform.Rotate(0, yawInput, 0, Space.World);
        }

        public void Rotate(float rotationInput)
        {
            float scale = (Controller.Floor - transform.position.y) / CamTrans.forward.y;
            Vector3 point = transform.position + CamTrans.forward * scale;
            transform.RotateAround(point, Vector3.up, rotationInput);
        }

        public void ClampVertical(float lowerBound, float upperBound)
        {
            Vector3 position = transform.position;
            position.y = Mathf.Clamp(position.y, lowerBound, upperBound);
            transform.position = position;
        }

        public void ClampPitch(float lowerAngle, float upperAngle)
        {
            Vector3 front = Vector3.Cross(CamTrans.right, Vector3.up).normalized;
            if (CamTrans.forward.y > 0)
            {
                float up = Vector3.Angle(front, CamTrans.forward);
                if (up > upperAngle)
                {
                    transform.rotation *= Quaternion.AngleAxis(up - upperAngle, Vector3.right);
                }
                if (lowerAngle < 0 && up < -lowerAngle)
                {
                    transform.rotation *= Quaternion.AngleAxis(up + lowerAngle, Vector3.right);
                }
            }
            else
            {
                float down = Vector3.Angle(front, CamTrans.forward);
                if (down > lowerAngle)
                {
                    transform.rotation *= Quaternion.AngleAxis(down - lowerAngle, -Vector3.right);
                }
                if (upperAngle < 0 && down < -upperAngle)
                {
                    transform.rotation *= Quaternion.AngleAxis(down + upperAngle, -Vector3.right);
                }
            }
        }

        public void MoveVertical(float verticalInput)
        {
            return;
        }

        public void Roll(float rollInput)
        {
            return;
        }

        public void ClampLateral(float lowerBound, float upperBound)
        {
            return;
        }

        public void ClampLongitudinal(float lowerBound, float upperBound)
        {
            return;
        }

        public void ClampZoom(float lowerBound, float upperBound)
        {
            return;
        }

        public void ClampYaw(float lowerBound, float upperBound)
        {
            return;
        }

        public void ClampRoll(float lowerBound, float upperBound)
        {
            return;
        }

        public void ClampRotate(float lowerBound, float upperBound)
        {
            return;
        }

        #endregion

        /* Double click do something coroutine */
        //IEnumerator ResetView() {
        //    float startTime = Time.time;
        //    yield return new WaitForEndOfFrame();
        //    while ((Time.time - startTime)<delay) {
        //        if (Input.GetMouseButtonDown(0)) {
        //            Vector3 newDir = Vector3.RotateTowards(cam.transform.forward, default_dir, rotationSpeed, 0.0f);
        //            transform.rotation = Quaternion.LookRotation(newDir);
        //            //transform.position = default_pos;
        //            yield break;
        //        }
        //        yield return null;
        //    }
        //    yield break;
        //}

    }
}