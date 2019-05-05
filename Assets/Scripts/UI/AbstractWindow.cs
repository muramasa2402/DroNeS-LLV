using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

namespace Drones.UI
{
    using Drones.Utils;
    using Drones.Utils.Extensions;
    using Drones.Interface;
    using static Singletons;
    public abstract class AbstractWindow : MonoBehaviour, IPoolable
    {
        public static uint OpenWindowCount { get; private set; }
        public static AbstractWindow GetWindow(Transform current)
        {

            if (current != null)
            {
                AbstractWindow window = current.GetComponent<AbstractWindow>();
                if (window == null)
                {
                    return GetWindow(current.parent);
                }
                return window;
            }
            return null;
        }

        #region Fields
        [SerializeField]
        private TextMeshProUGUI _WindowName;
        [SerializeField]
        private GameObject _ContentPanel;
        [SerializeField]
        private List<GameObject> _DisableOnMinimize;
        private Transform _Decoration;
        [SerializeField]
        protected Button _Close;
        [SerializeField]
        protected Button _MinimizeButton;
        [SerializeField]
        protected Button _MaximizeButton;
        #endregion

        #region IPoolable
        public bool InPool { get; private set; }
        public virtual void OnGet(Transform parent)
        {
            InPool = true;
            OpenWindowCount++;
            IsOpen = true;
            gameObject.SetActive(true);
            transform.SetParent(parent, false);
            transform.ToRect().offsetMax = UIObjectPool.GetTemplate(Type).transform.ToRect().offsetMax;
            transform.ToRect().offsetMin = UIObjectPool.GetTemplate(Type).transform.ToRect().offsetMin;
        }
        public virtual void OnRelease()
        {
            InPool = false;
            OpenWindowCount--;
            IsOpen = false;
            gameObject.SetActive(false);
            transform.SetParent(UIObjectPool.PoolContainer, false);
            Opener = null;
            CreatorEvent = null;
        }

        public virtual void Delete()
        {
            UIObjectPool.Release(Type, this);
        }
        #endregion

        public bool IsOpen { get; protected set; }

        public abstract WindowType Type { get; }

        /* The following properties are used to alter the button used to open this window to avoid duplicate windows */
        // Opener is a delegate
        public UnityAction Opener { get; set; } = null;
        // CreatorEvent is the event that opened this window
        public Button.ButtonClickedEvent CreatorEvent { get; set; } = null;

        #region Protected Properties
        protected List<GameObject> DisableOnMinimize
        {
            get => _DisableOnMinimize;
            set => _DisableOnMinimize = value;
        }

        protected abstract Vector2 MaximizedSize { get; }

        protected abstract Vector2 MinimizedSize { get; }

        protected virtual Transform Decoration
        {
            get
            {
                if (_Decoration == null)
                {
                    _Decoration = transform.Find("Decoration");
                }
                return _Decoration;
            }
        }

        protected virtual TextMeshProUGUI WindowName
        {
            get
            {
                if (_WindowName == null)
                {
                    _WindowName = Decoration.Find("Name").GetComponent<TextMeshProUGUI>();
                }
                return _WindowName;
            }
        }

        public virtual Button Close
        {
            get
            {
                if (_Close == null)
                {
                    _Close = Decoration.Find("Close Button").GetComponent<Button>();
                }
                return _Close;
            }
        }

        protected virtual Button MinimizeButton
        {
            get
            {
                if (_MinimizeButton == null)
                {
                    _MinimizeButton = Decoration.Find("Minimize Button").GetComponent<Button>();
                }
                return _MinimizeButton;
            }
        }

        protected virtual Button MaximizeButton
        {
            get
            {
                if (_MaximizeButton == null)
                {
                    _MaximizeButton = Decoration.Find("Maximize Button").GetComponent<Button>();
                }
                return _MaximizeButton;
            }
        }

        protected virtual GameObject ContentPanel
        {
            get
            {
                if (_ContentPanel == null)
                {
                    _ContentPanel = transform.Find("Contents Panel").gameObject;
                }
                return _ContentPanel;
            }
        }
        #endregion

        protected virtual void Awake()
        {
            MinimizeButton.onClick.AddListener(MinimizeWindow);
            MaximizeButton.onClick.AddListener(MaximizeWindow);

            Close.onClick.AddListener(delegate 
            {
                Delete();
                if (CreatorEvent != null)
                {
                    CreatorEvent.RemoveAllListeners();
                    CreatorEvent.AddListener(Opener);
                }
            });
        }

        protected virtual void MinimizeWindow()
        {
            foreach(GameObject go in DisableOnMinimize)
            {
                go.SetActive(false);
            }
            MaximizeButton.gameObject.SetActive(true);
            MinimizeButton.gameObject.SetActive(false);
            transform.ToRect().sizeDelta = MinimizedSize;
        }

        protected virtual void MaximizeWindow()
        {
            foreach (GameObject go in DisableOnMinimize)
            {
                go.SetActive(true);
            }
            MaximizeButton.gameObject.SetActive(false);
            MinimizeButton.gameObject.SetActive(true);
            transform.ToRect().sizeDelta = MaximizedSize;
        }

        protected void SetName(string name)
        {
            WindowName.SetText(name);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && OpenWindowCount > 2)
            {
                for (int i = UICanvas.childCount - 1; i >= 0; i--)
                {
                    AbstractWindow window = UICanvas.GetChild(i).GetComponent<AbstractInfoWindow>();
                    if (window != null)
                    {
                        window.Close?.onClick.Invoke();
                    }
                    else
                    {
                        window = UICanvas.GetChild(i).GetComponent<AbstractListWindow>();
                        window?.Close?.onClick.Invoke();
                    }

                }
            }
        }

    }
}