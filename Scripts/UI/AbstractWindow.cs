using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace Drones.UI
{
    using Drones.DataStreamer;
    using Drones.Utils;
    using Drones.Utils.Extensions;

    public abstract class AbstractWindow : MonoBehaviour
    {
        public abstract WindowType Type { get; }

        [SerializeField]
        private Vector2 _MaximizedSize;
        [SerializeField]
        private Vector2 _MinimizedSize;
        [SerializeField]
        protected TextMeshProUGUI _WindowName;
        [SerializeField]
        protected GameObject _ContentPanel;
        [SerializeField]
        protected List<GameObject> _DisableOnMinimize;
        protected Transform _Decoration;
        [SerializeField]
        protected Button _Close;
        [SerializeField]
        protected Button _MinimizeButton;
        [SerializeField]
        protected Button _MaximizeButton;

        public List<GameObject> DisableOnMinimize
        {
            get
            {
                return _DisableOnMinimize;
            }
            protected set
            {
                _DisableOnMinimize = value;
            }
        }

        public Vector2 MaximizedSize
        {
            get
            {
                return _MaximizedSize;
            }
            protected set
            {
                _MaximizedSize = value;
            }
        }

        public Vector2 MinimizedSize
        {
            get
            {
                return _MinimizedSize;
            }
            protected set
            {
                _MinimizedSize = value;
            }
        }

        public virtual Transform Decoration
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

        public virtual TextMeshProUGUI WindowName
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

        public virtual Button MinimizeButton
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

        public virtual Button MaximizeButton
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

        public virtual GameObject ContentPanel
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

        protected virtual void Start()
        {
            MinimizeButton.onClick.AddListener(MinimizeWindow);
            MaximizeButton.onClick.AddListener(MaximizeWindow);
            Close.onClick.AddListener(delegate { Destroy(gameObject); });
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

        public static AbstractWindow GetWindow(Transform current)
        {
            if (current.tag != "Window" && current != null)
            {
                return GetWindow(current.parent);
            }
            if (current == null)
            {
                return null;
            }
            return current.GetComponent<AbstractWindow>();
        }
    }
}