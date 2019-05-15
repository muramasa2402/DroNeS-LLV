﻿using System.Collections;
using TMPro;
using UnityEngine;

namespace Drones.UI
{
    using Drones.Utils.Extensions;
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
            var wait = new WaitForSeconds(1 / 10f);
            while (true)
            {
                yield return new WaitUntil(() => AbstractCamera.ActiveCamera != null);
                Display.SetText(AbstractCamera.ActiveCamera.transform.position.ToStringXZ());
                yield return wait;
            }
        }
    }
}
