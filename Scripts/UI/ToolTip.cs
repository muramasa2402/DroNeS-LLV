using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Drones.Interface;

namespace Drones.UI
{
    public class ToolTip : UIFocus, IPoolable
    {
        [SerializeField]
        TextMeshProUGUI _Body;
        private TextMeshProUGUI Body
        {
            get
            {
                if (_Body == null)
                {
                    _Body = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                }
                return _Body;
            }
        }

        public void OnGet(Transform parent)
        {

        }

        public void OnRelease()
        {

        }

        public void SelfRelease()
        {

        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
        }
    }
}
