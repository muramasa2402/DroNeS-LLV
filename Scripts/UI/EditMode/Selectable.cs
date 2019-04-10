using System;
using System.Collections;
using UnityEngine;

namespace Drones.UI
{
    using static Singletons;
    using Utils;
    using Drones.DataStreamer;
    using Drones.Interface;

    public class Selectable: MonoBehaviour
    {
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
        public static Selectable Selected { get; private set; }
        public static bool DeleteMode { get; set; }

        private static readonly WaitUntil _Wait = new WaitUntil(() => Input.GetMouseButton(0));
        private static Camera _Cam;

        private const float _ClickDelta = 0.35f;

        private bool _Rotating;
        private Vector3 _Origin;
        private Vector3 _CurrPos;
        private Vector2 _ScreenPos;
        [SerializeField]
        private bool _Editable;
        [SerializeField]
        private IDataSource _ThisSource;

        private float _ClickTime;
        private bool _FirstClick;
        private IDataSource ThisSource
        {
            get
            {
                if (_ThisSource == null)
                {
                    _ThisSource = GetComponent<IDataSource>();
                }
                return _ThisSource;
            }
        }

        private bool Editable
        {
            get
            {
                return GameManager.SimStatus == SimulationStatus.EditMode && _Editable;
            }
            set
            {
                _Editable = value;
            }
        }

        #region Mouse Interactions
        private void OnMouseEnter()
        {
            if (Editable && Selected == this)
            {
                StopCoroutine(WaitForDeselect());
            }
        }

        private void OnMouseUpAsButton()
        {
            if (Editable)
            {
                if (Selected != null && Selected != this)
                {
                    Deselect();
                    Select();
                }
                else if (Selected == null)
                {
                    Select();
                }
                else if (DeleteMode && Selected == this)
                {
                    ((IPoolable)ThisSource).SelfRelease();
                    DeleteMode = false;
                }
            }

            if (_FirstClick && Time.unscaledTime - _ClickTime > _ClickDelta)
            {
                _FirstClick = false;
            }

            if (_FirstClick && Time.unscaledTime - _ClickTime <= _ClickDelta)
            {
                ThisSource.OpenInfoWindow();
            }
            else
            {
                _FirstClick = true;
                _ClickTime = Time.unscaledTime;
            }
        }

        private void OnMouseDown()
        {
            if (Editable && Selected == this)
            {
                StartCoroutine(UIFocus.ControlListener());
                _ScreenPos = Input.mousePosition;
                _Origin = Cam.ScreenToWorldPoint(new Vector3(_ScreenPos.x, _ScreenPos.y, Cam.nearClipPlane));
                _Origin.y = 0;
            }
        }

        private void OnMouseExit()
        {
            if (Editable && Selected == this)
            {
                StartCoroutine(WaitForDeselect());
            }
        }

        private void OnMouseDrag()
        {
            if (Editable && Selected == this)
            {
                _ScreenPos = Input.mousePosition;
                _CurrPos = Cam.ScreenToWorldPoint(new Vector3(_ScreenPos.x, _ScreenPos.y, Cam.nearClipPlane));
                _CurrPos.y = 0;
                transform.position += _CurrPos - _Origin;
                _Origin = _CurrPos;
            }
        }

        private void OnMouseOver()
        {
            if (!_Rotating && Editable && Input.GetMouseButtonDown(1))
            {
                _ScreenPos = Input.mousePosition;
                _Origin = Cam.ScreenToWorldPoint(new Vector3(_ScreenPos.x, _ScreenPos.y, Cam.nearClipPlane));
                _Origin.y = 0;
                StartCoroutine(Rotation());
            }
        }
        #endregion

        private void OnDisable()
        {
            if (Selected == this)
            {
                Deselect();
            }
            StopAllCoroutines();
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

        private void Select()
        {
            Selected = this;
            // Highlight?
            if (transform.childCount > 3)
            {
                var pivots = GetComponentsInChildren<Pivot>(true);
                for (int i = 0; i < pivots.Length; i++)
                {
                    pivots[i].gameObject.SetActive(true);
                }
            }
        }

        public static void Deselect()
        {
            if (Pivot.Operating) { return; } // If in the middle of resizing don't deselect

            for (int i = 0; Selected != null && i < Selected.transform.childCount; i++)
            {
                Selected.transform.GetChild(i).gameObject.SetActive(false); 
                //May need to check for other children
            }
            Selected = null;
        }

        public IEnumerator WaitForDeselect()
        {
            if (Selected == null) { yield break; }

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
