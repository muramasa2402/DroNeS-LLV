using System.Collections;
using UnityEngine;
using Utils;
using Utils.Interfaces;

namespace Drones.Utils
{
    public abstract class AbstractCamera : MonoBehaviour, ICameraMovement
    {
        private CameraController _controller;
        private Camera _CameraComponent;
        protected bool Following;
        public static GameObject PositionHighlight;

        #region Statics
        public static GameObject Followee { get; set; }

        public static bool Controlling { get; private set; }

        public static AbstractCamera ActiveCamera { get; protected set; }

        public static Transform CameraTransform => ActiveCamera.transform.GetChild(0);

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

        public static void LookHere(Vector3 position)
        {
            position.y = 0;
            HighlightPosition(position);
            var back = -CameraTransform.forward;
            position += back * ActiveCamera.transform.position.y / back.y;
            ActiveCamera.StopCoroutine(FlyTowards(position));
            ActiveCamera.StartCoroutine(FlyTowards(position));
        }

        private static IEnumerator FlyTowards(Vector3 position)
        {
            var origin = ActiveCamera.transform.position;
            var start = Time.unscaledTime;
            var speed = Vector3.Distance(origin, position) * 2;
            if (speed <= 0.1f) yield break;
            float covered;
            float frac = 0;

            while (frac < 1 - 1e-5f)
            {
                covered = (Time.unscaledTime - start) * speed;
                frac = covered / Vector3.Distance(origin, position);

                ActiveCamera.transform.position = Vector3.Lerp(origin, position, frac);
                yield return null;
            }
        }

        public static void HighlightPosition(Vector3 position)
        {
            if (PositionHighlight != null)
            {
                PositionHighlight.GetComponent<Animation>().Stop();
                PositionHighlight.GetComponent<Animation>().Play();
                PositionHighlight.transform.GetChild(0).GetComponent<Animation>().Stop();
                PositionHighlight.transform.GetChild(0).GetComponent<Animation>().Play();
            }
            else
            {
                PositionHighlight = Instantiate(Singletons.PositionHighlightTemplate);
                PositionHighlight.name = "Current Position";
            }
            PositionHighlight.transform.position = position;
            PositionHighlight.transform.position += Vector3.up * (PositionHighlight.transform.lossyScale.y + 0.5f);
        }
        #endregion

        #region Properties
        public CameraController Controller
        {
            get
            {
                if (_controller == null)
                {
                    _controller = new CameraController(this);
                }
                if (_controller.Move == null)
                {
                    _controller.Move = this;
                }
                return _controller;
            }
        }

        protected float SpeedScale => Controller.SpeedToHeightGradient * transform.position.y + 1;

        public Camera CameraComponent
        {
            get
            {
                if (_CameraComponent == null) _CameraComponent = GetComponentInChildren<Camera>();

                return _CameraComponent;
            }
        }

        public bool IsActive => ActiveCamera == this;
        #endregion

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
            Following = false;
        }

        protected virtual void OnDestroy()
        {
            Following = false;
            Followee = null;
            Controlling = false;
            ActiveCamera = null;
            _controller = null;
        }

        public abstract void BreakFollow();

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
                var num = collision.GetContacts(contacts);
                for (var i = 0; i < num; i++)
                {
                    var dot = Vector3.Dot(motion, contacts[i].normal.normalized);
                    if (dot < 0)
                    {
                        transform.position -= dot * contacts[i].normal.normalized;
                    }
                }
                previousPosition = transform.position;
                yield return null;
            }
        }

        private void OnCollisionEnter(Collision collision) => StartCoroutine(Collide(collision));

        private void OnCollisionExit(Collision collision) => StopCoroutine(Collide(collision));

        #region ICameraMovement Implementation

        public virtual void MoveLongitudinal(float input)
        {
            var positiveDirection = Vector3.Cross(CameraTransform.right, Vector3.up).normalized;

            transform.position += input * Time.unscaledDeltaTime * positiveDirection;
        }

        public virtual void MoveLateral(float input)
        {
            var positiveDirection = CameraTransform.right;

            transform.position += input * Time.unscaledDeltaTime * positiveDirection;
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