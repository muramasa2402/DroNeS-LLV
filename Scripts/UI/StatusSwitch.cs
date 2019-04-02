using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Drones.UI
{

    using Drones.Utils;

    public class StatusSwitch : DataField
    {
        public delegate void StatusChangeAlert(Status s);

        // For window use
        private event StatusChangeAlert _OnStatusChange;
        public event StatusChangeAlert OnStatusChange 
        {
            add
            {
                if (_OnStatusChange == null || !_OnStatusChange.GetInvocationList().Contains(value))
                {
                    _OnStatusChange += value;
                }
            }
            remove
            {
                _OnStatusChange -= value;
            }
        }

        private static Map<string, Status> _StringStatusMap;

        public static Map<string, Status> StringStatusMap
        {
            get
            {
                if (_StringStatusMap == null)
                {
                    _StringStatusMap = new Map<string, Status>()
                    {
                        { "A", Status.Active },
                        { "B", Status.SemiActive },
                        { "C", Status.Inactive },
                        { "-", Status.Null }
                    };
                }
                return _StringStatusMap;
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
                StatusIcons[key].SetActive(key == StringStatusMap.Forward[v]);
            }

        }

        public void SetField(Status s)
        {
            SetText(StringStatusMap.Reverse[s]);

            foreach (var key in StatusIcons.Keys)
            {
                StatusIcons[key].SetActive(key == s);
            }
            _OnStatusChange?.Invoke(s);
        }
    }
}
