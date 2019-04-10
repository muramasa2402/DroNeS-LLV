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
                    _Window = AbstractWindow.GetWindow(transform).transform;
                }
                return _Window;
            }
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
            Controlling = false;
            if (hover > 0) { hover--; }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!AbstractCamera.Controlling)
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
            var wait = new WaitUntil(() => Input.GetMouseButtonUp(0));
            Controlling = true;
            yield return wait;
            Controlling = false;
            yield break;
        }

    }

}
