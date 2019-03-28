using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace Drones
{
    using static SceneAttributes;

    public class UIFocus : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        public static int hover;
        public static bool controlling;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CameraControl.Controlling)
            {
                hover++;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (hover > 0) { hover--; }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!controlling) { StartCoroutine(ControlListener()); }
        }


        IEnumerator ControlListener()
        {
            controlling = true;
            do
            {
                yield return null;
            } while (!Input.GetMouseButtonUp(0));
            controlling = false;
            yield break;
        }
    }

}
