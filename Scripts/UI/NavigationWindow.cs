using System.Collections.Generic;
using UnityEngine;

namespace Drones.UI
{
    public class NavigationWindow : Window
    {
        void Start()
        {
            SetUp();
            windowName.text = "Navigation";
            minimizedSize = new Vector2(315, 85);
            maximizedSize = new Vector2(315, 445);
            MaximizeWindow();
            disableOnMinimize = new List<GameObject>
            {
                contentPanel.transform.Find("Minimap").gameObject,
                contentPanel.transform.Find("Altimeter").gameObject
            };
        }
    }

}
