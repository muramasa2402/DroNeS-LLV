using System.Collections.Generic;
using Drones.Utils;
using UnityEngine;

namespace Drones.UI
{
    public class NavigationWindow : AbstractWindow
    {
        public override WindowType Type { get; } = WindowType.Navigation;
        protected override Vector2 MinimizedSize { get; } = new Vector2(315, 85);
        protected override Vector2 MaximizedSize { get; } = new Vector2(315, 445);

        protected override void Awake()
        {
            SetName("Navigation");
            MaximizeWindow();

            DisableOnMinimize = new List<GameObject>
            {
                ContentPanel.transform.Find("Minimap").gameObject,
                ContentPanel.transform.Find("Altimeter").gameObject
            };

            base.Awake();
        }
    }

}
