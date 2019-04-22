﻿using System.Collections;
using UnityEngine;

namespace Drones.UI
{
    using Drones.Utils;
    public class Clock : MonoBehaviour
    {
        [SerializeField]
        DataField _Display;

        private DataField Display
        {
            get
            {
                if (_Display == null)
                {
                    _Display = GetComponent<DataField>();
                }
                return _Display;
            }
        }
        public static Clock Instance { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        private IEnumerator Start()
        {
            var time = TimeKeeper.Chronos.Get();
            Display.SetText(time.Now().ToString());
            var wait = new WaitForSeconds(0.1f);
            while (true)
            {
                Display.SetText(time.Now().ToStringLong());
                yield return wait;
            }
        }


    }
}
