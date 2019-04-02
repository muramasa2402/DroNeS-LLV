using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace Drones.UI
{
    using static Singletons;

    public class UIFocus : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
    {
        public static int hover;
        private Transform _Window;
        public static bool Controlling { get; protected set; }

        protected Transform Window
        {
            get
            {
                if (_Window == null)
                {
                    _Window = UI.AbstractWindow.GetWindow(transform).transform;
                }
                return _Window;
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!CameraControl.Controlling)
            {
                hover++;
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (hover > 0) { hover--; }
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (!Controlling) { StartCoroutine(ControlListener()); }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                transform.SetAsLastSibling();
            }
        }

        public static IEnumerator ControlListener()
        {
            Controlling = true;
            yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
            Controlling = false;
            yield break;
        }

    }

}
