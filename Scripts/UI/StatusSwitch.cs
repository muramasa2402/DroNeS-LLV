using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace Drones.UI
{
    using Drones.Utils;

    public class StatusSwitch : DataField
    {
        public delegate void StatusChangeAlert(Status s);


        // For window use
        private event StatusChangeAlert Change;
        public event StatusChangeAlert StatusChange 
        {
            add
            {
                if (Change == null || !Change.GetInvocationList().Contains(value))
                {
                    Change += value;
                }
            }
            remove
            {
                Change -= value;
            }
        }

        private Dictionary<Status, GameObject> _StatusIcons;

        public Dictionary<Status, GameObject> StatusIcons
        {
            get
            {
                if (_StatusIcons == null)
                {
                    _StatusIcons = new Dictionary<Status, GameObject>
                    {
                        { Status.Green, transform.Find("Green").gameObject },
                        { Status.Yellow, transform.Find("Yellow").gameObject },
                        { Status.Red, transform.Find("Red").gameObject }
                    };
                }
                return _StatusIcons;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            color = new Color(1, 1, 1, 0);
            fontSize = 1;
        }

        public override void SetField(string v)
        {
            SetText(v);

            foreach(var key in StatusIcons.Keys)
            {
                StatusIcons[key].SetActive(key == (Status) Enum.Parse(typeof(Status), v));
            }
        }

    }
}
