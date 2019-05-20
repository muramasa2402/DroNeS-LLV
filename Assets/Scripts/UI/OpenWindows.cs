using System.Collections;
using System.Collections.Generic;
using Drones.Utils.Extensions;
using UnityEngine;

namespace Drones.UI
{
    public class OpenWindows : MonoBehaviour
    {
        private static OpenWindows Instance;

        public static Transform Transform => Instance?.transform;

        private readonly List<AbstractWindow> _Windows = new List<AbstractWindow>();

        private NavigationWindow _Navigation;

        public static NavigationWindow Navigation
        {
            get
            {
                if (Instance._Navigation == null)
                {
                    for (int i = 0; i < Transform.childCount && Instance._Navigation == null; i++)
                    {
                        Instance._Navigation = Transform.GetChild(i).GetComponent<NavigationWindow>();
                    }
                }
                return Instance._Navigation;
            }
        }

        void Awake()
        {
            Instance = this;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {

            while (_Windows != null && _Windows.Count > 0)
            {
                if (_Windows.Count <= 0) break;

                _Windows[0].Close.onClick.Invoke();
            }

            Instance = null;
        }

        public static void AddToList(AbstractWindow window)
        {
            var type = window.GetType();
            if (type != typeof(ConsoleLog) && type != typeof(NavigationWindow))
            {
                Instance._Windows.Add(window);
                window.transform.SetParent(Instance.transform, false);
                window.transform.ToRect().anchoredPosition = Vector2.one * -25 * Instance._Windows.Count;

            }
        }

        public static AbstractWindow GetTop()
        {
            if (Instance._Windows.Count == 0) return null;

            Instance._Windows.Sort((x, y) => y.transform.GetSiblingIndex() - x.transform.GetSiblingIndex());
            return Instance._Windows[0];
        }

        public static void Remove(AbstractWindow window)
        {
            if (Instance._Windows.Count == 0) return;
            Instance._Windows.Remove(window);

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

