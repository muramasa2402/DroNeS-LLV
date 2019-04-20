using System.Collections;
using TMPro;
using UnityEngine;

namespace Drones.UI
{
    using static Drones.Utils.StaticFunc;
    using static Drones.Singletons;
    public class GPS : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _Display;

        public TextMeshProUGUI Display
        {
            get
            {
                if (_Display == null)
                {
                    _Display = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                }
                return _Display;
            }
        }
        void OnEnable()
        {
            StartCoroutine(Locate());
        }

        private void OnDisable()
        {
            StopCoroutine(Locate());
        }

        IEnumerator Locate()
        {
            var wait = new WaitForSeconds(1 / 30f);
            while (true)
            {
                Display.SetText(CoordString(UnityToCoord(RTS.transform.position)));
                yield return wait;
            }
        }
    }
}
