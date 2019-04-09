using System;
using System.Collections;
using UnityEngine;

namespace Drones.UI.Edit
{
    using static Singletons;
    using Utils;
    public class EditModeSelection: MonoBehaviour
    {
        //TODO make this work in simulating mode
        public static EditModeSelection Selected { get; protected set; }
        protected static readonly WaitUntil _Wait = new WaitUntil(() => Input.GetMouseButton(0));
        private bool _Rotating;
        protected Vector3 _Origin;
        protected Vector3 _CurrPos;
        protected Vector2 _ScreenPos;
        private static Camera _Cam;
        public static Camera Cam
        {
            get
            {
                if (_Cam == null)
                {
                    _Cam = EagleEye.GetComponent<Camera>();
                }
                return _Cam;
            }
        }

        protected virtual void OnMouseEnter()
        {
            if (GameManager.SimStatus == SimulationStatus.EditMode && Selected == this)
            {
                StopCoroutine(WaitForDeselect());
            }
        }

        protected virtual void OnMouseUpAsButton()
        {
            // TODO Enable only in EditMode
            if (GameManager.SimStatus != SimulationStatus.EditMode && !Input.GetMouseButtonUp(0)) { return; }

            if (Selected != null && Selected != this)
            {
                Deselect();
                SelectEdit();
            }
            else if (Selected == null)
            {
                SelectEdit();
            }

        }

        protected virtual void OnMouseDown()
        {
            // TODO Enable only in EditMode
            if (GameManager.SimStatus == SimulationStatus.EditMode && Selected == this)
            {
                StartCoroutine(UIFocus.ControlListener());
                _ScreenPos = Input.mousePosition;
                _Origin = Cam.ScreenToWorldPoint(new Vector3(_ScreenPos.x, _ScreenPos.y, Cam.nearClipPlane));
                _Origin.y = 0;
            }
        }

        protected virtual void OnMouseExit()
        {
            // TODO Enable only in EditMode
            if (GameManager.SimStatus == SimulationStatus.EditMode && Selected == this)
            {
                StartCoroutine(WaitForDeselect());
            }
        }

        protected virtual void OnMouseDrag()
        {
            // TODO Enable only in EditMode
            if (GameManager.SimStatus == SimulationStatus.EditMode && Selected == this)
            {
                _ScreenPos = Input.mousePosition;
                _CurrPos = Cam.ScreenToWorldPoint(new Vector3(_ScreenPos.x, _ScreenPos.y, Cam.nearClipPlane));
                _CurrPos.y = 0;
                transform.position += _CurrPos - _Origin;
                _Origin = _CurrPos;
            }
        }

        protected virtual void OnMouseOver()
        {
            // TODO Enable only in EditMode
            if (GameManager.SimStatus == SimulationStatus.EditMode && !_Rotating && Input.GetMouseButtonDown(1))
            {
                _ScreenPos = Input.mousePosition;
                _Origin = Cam.ScreenToWorldPoint(new Vector3(_ScreenPos.x, _ScreenPos.y, Cam.nearClipPlane));
                _Origin.y = 0;
                StartCoroutine(Rotation());
            }
        }

        private void OnDisable()
        {
            StopCoroutine(WaitForDeselect());
        }

        private IEnumerator Rotation()
        {
            _Rotating = true;
            while (!Input.GetMouseButtonUp(1))
            {
                _ScreenPos = Input.mousePosition;
                _CurrPos = Cam.ScreenToWorldPoint(new Vector3(_ScreenPos.x, _ScreenPos.y, Cam.nearClipPlane));
                _CurrPos.y = 0;
                Vector3 t = _CurrPos - transform.position;
                Vector3 c = _Origin - transform.position;

                float angle = Vector3.SignedAngle(c, t, Vector3.up);
                transform.rotation *= Quaternion.AngleAxis(angle, Vector3.up);
                _Origin = _CurrPos;
                yield return null;
            }
            _Rotating = false;
            yield break;
        }

        protected void SelectEdit()
        {
            Selected = this;
            var pivots = GetComponentsInChildren<Pivot>(true);
            for (int i = 0; i < pivots.Length; i++)
            {
                pivots[i].gameObject.SetActive(true);
            }
        }

        public static void Deselect()
        {
            if (Pivot.Operating) { return; }

            for (int i = 0; Selected != null && i < Selected.transform.childCount; i++)
            {
                Selected.transform.GetChild(i).gameObject.SetActive(false); 
                //May need to check for other children
            }
            Selected = null;
        }

        public IEnumerator WaitForDeselect()
        {
            yield return _Wait;

            _ScreenPos = Input.mousePosition;
            Vector3 origin = Cam.ScreenToWorldPoint(new Vector3(_ScreenPos.x, _ScreenPos.y, Cam.nearClipPlane));
            origin.y = 800;

            if (!Physics.Raycast(new Ray(origin, Vector3.down), out RaycastHit info, 800, 1 << 14)
            || (info.transform.parent != Selected && info.transform != Selected))
            {
                Deselect();
            }

            yield break;
        }
    }
}
