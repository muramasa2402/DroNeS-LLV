using System.Collections.Generic;
using Drones.Utils;
using UnityEngine;

namespace Drones.UI
{
    public class NavigationWindow : AbstractWindow
    {
        public override WindowType Type { get; } = WindowType.Navigation;

        protected override void Start()
        {
            WindowName.text = "Navigation";
            MinimizedSize = new Vector2(315, 85);
            MaximizedSize = new Vector2(315, 445);
            MaximizeWindow();
            DisableOnMinimize = new List<GameObject>
            {
                ContentPanel.transform.Find("Minimap").gameObject,
                ContentPanel.transform.Find("Altimeter").gameObject
            };
            base.Start();
        }
    }

}
