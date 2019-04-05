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
#pragma warning disable IDE1006 // Naming Styles
        private event StatusChangeAlert _StatusChange;
#pragma warning restore IDE1006 // Naming Styles
        public event StatusChangeAlert StatusChange 
        {
            add
            {
                if (_StatusChange == null || !_StatusChange.GetInvocationList().Contains(value))
                {
                    _StatusChange += value;
                }
            }
            remove
            {
                _StatusChange -= value;
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
                        { Status.Active, transform.Find("Active").gameObject },
                        { Status.SemiActive, transform.Find("SemiActive").gameObject },
                        { Status.Inactive, transform.Find("Inactive").gameObject }
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
