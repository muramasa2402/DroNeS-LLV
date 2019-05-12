using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.UI
{
    public class OpenWindows : MonoBehaviour
    {
        private static OpenWindows Instance;
        private readonly List<AbstractWindow> _Windows = new List<AbstractWindow>();
        void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
             foreach (var window in _Windows)
            {
                window.Close.onClick.Invoke();
            }
        }

        public static void AddToList(AbstractWindow window)
        {
            var type = window.GetType();
            if (type != typeof(ConsoleLog) && type != typeof(NavigationWindow))
            {
                Instance._Windows.Add(window);
            }
        }

    }
}

