using System.Collections.Generic;
using Drones.Utils;
using Drones.Utils.Extensions;
using UnityEngine;

namespace Drones.UI
{
    public class NavigationWindow : AbstractWindow
    {
        public override WindowType Type { get; } = WindowType.Navigation;
        protected override Vector2 MinimizedSize { get; } = new Vector2(315, 85);
        protected override Vector2 MaximizedSize { get; } = new Vector2(375, 480);

        [SerializeField]
        private GameObject _FilterPanel;
        [SerializeField]
        private GameObject _GPS;

        public GameObject FilterPanel
        {
            get
            {
                if (_FilterPanel == null)
                {
                    _FilterPanel = ContentPanel.transform.Find("Filter Panel").gameObject;
                }
                return _FilterPanel;
            }
        }

        public GameObject GPS
        {
            get
            {
                if (_GPS == null)
                {
                    _GPS = ContentPanel.transform.Find("GPS").gameObject;
                }
                return _GPS;
            }
        }

        protected override void Awake()
        {
            SetName("Navigation");
            MaximizeWindow();

            DisableOnMinimize = new List<GameObject>
            {
                ContentPanel.transform.Find("Minimap").gameObject,
                ContentPanel.transform.Find("Altimeter").gameObject,
                ContentPanel.transform.Find("Compass").gameObject,
                FilterPanel
            };

            base.Awake();
        }

        protected override void MinimizeWindow()
        {
            base.MinimizeWindow();
            Vector2 v = GPS.transform.ToRect().offsetMax;
            v.x = 0;
            GPS.transform.ToRect().offsetMax = v;
        }

        protected override void MaximizeWindow()
        {
            base.MaximizeWindow();
            Vector2 v = GPS.transform.ToRect().offsetMax;
            v.x = -FilterPanel.transform.ToRect().sizeDelta.x;
            GPS.transform.ToRect().offsetMax = v;
        }
    }

}
