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
            while (_Windows.Count > 0)
            {
                _Windows[0].Close.onClick.Invoke();
                _Windows.RemoveAt(0);
            }
        }

        public static void AddToList(AbstractWindow window)
        {
            var type = window.GetType();
            if (type != typeof(ConsoleLog) && type != typeof(NavigationWindow))
            {
                Instance._Windows.Add(window);
                window.transform.SetParent(Instance.transform, false);
            }
        }

        public static AbstractWindow GetTop()
        {
            if (Instance._Windows.Count == 0) return null;

            Instance._Windows.Sort((x, y) => x.transform.GetSiblingIndex() - y.transform.GetSiblingIndex());
            return Instance._Windows[0];
        }

        public static void Remove(AbstractWindow window)
        {
            if (Instance._Windows.Count == 0) return;
            int i = Instance._Windows.IndexOf(window);
            if (i >= 0)
            {
                Instance._Windows.RemoveAt(i);
            }

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GetTop()?.Close?.onClick.Invoke();
            }
        }

    }
}

