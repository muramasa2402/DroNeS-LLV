using System;

using UnityEngine;
using System.Collections;
using Drones.Interface;

namespace Drones
{
    using UI;
    using static Singletons;
    public abstract class AbstractCamera : MonoBehaviour, ICameraMovement
    {
        private CameraController _Controller;

        public CameraController Controller
        {
            get
            {
                if (_Controller == null)
                {
                    _Controller = new CameraController(this);
                }
                if (_Controller.Move == null)
                {
                    _Controller.Move = this;
                }
                return _Controller;
            }
        }

        protected float SpeedScale
        {
            get
            {
                return Controller.SpeedToHeightGradient * transform.position.y + 1;
            }
        }

        public static GameObject Followee { get; set; }

        public static bool Controlling { get; private set; }

        public static IEnumerator ControlListener()
        {
            Controlling = true;
            do
            {
                yield return null;
            } while (!Input.GetMouseButtonUp(0));
            Controlling = false;
            yield break;
        }

        protected bool _Following;

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
            _Following = false;
        }

        protected abstract IEnumerator FollowObject();

        private IEnumerator Collide(Collision collision)
        {
            Vector3 previousPosition = transform.position;
            Vector3 motion;
            ContactPoint[] contacts = new ContactPoint[128];
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
            StartCoroutine(Collide(collision));
        }

        private void OnCollisionExit(Collision collision)
        {
            StopCoroutine(Collide(collision));
        }

        #region ICameraMovement Implementation

        public virtual void MoveLongitudinal(float input)
        {
            var positiveDirection = Vector3.Cross(CameraTransform.right, Vector3.up).normalized;

            transform.position += input * positiveDirection * Time.unscaledDeltaTime;
        }

        public virtual void MoveLateral(float input)
        {
            var positiveDirection = CameraTransform.right;

            transform.position += input * positiveDirection * Time.unscaledDeltaTime;
        }

        public virtual void Zoom(float input)
        {
            return;
        }

        public virtual void Pitch(float input)
        {
            return;
        }

        public virtual void Yaw(float input)
        {
            return;
        }

        public virtual void Rotate(float input)
        {
            return;
        }

        public virtual void ClampVertical(float lowerBound, float upperBound)
        {
            return;
        }

        public virtual void ClampPitch(float lowerAngle, float upperAngle)
        {
            return;
        }

        public virtual void MoveVertical(float input)
        {
            return;
        }

        public virtual void Roll(float input)
        {
            return;
        }

        public virtual void ClampLateral(float lowerBound, float upperBound)
        {
            return;
        }

        public virtual void ClampLongitudinal(float lowerBound, float upperBound)
        {
            return;
        }

        public virtual void ClampZoom(float lowerBound, float upperBound)
        {
            return;
        }

        public virtual void ClampYaw(float lowerBound, float upperBound)
        {
            return;
        }

        public virtual void ClampRoll(float lowerBound, float upperBound)
        {
            return;
        }

        public virtual void ClampRotate(float lowerBound, float upperBound)
        {
            return;
        }

        #endregion

    }
}