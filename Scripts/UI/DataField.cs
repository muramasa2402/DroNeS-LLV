using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Drones.UI
{
    public class DataField : TextMeshProUGUI, IPointerEnterHandler
    {
        string Value
        {
            get
            {
                return text;
            }
        }

        [SerializeField]
        private string _ToolTip;
        private WaitForSecondsRealtime wait = new WaitForSecondsRealtime(2);

        public virtual void SetField(string v)
        {
            SetText(v);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            StartCoroutine(ShowToolTip());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopCoroutine(ShowToolTip());
        }

        private IEnumerator ShowToolTip()
        {
            yield return wait;
            //TODO Create Panel
            string output = Value + "\n" + _ToolTip;
        }
    }

}