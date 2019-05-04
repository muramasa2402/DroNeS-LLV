using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Drones.UI
{
    using static Singletons;
    using EventSystem;
    using Utils.Extensions;
    using Utils;
    using Interface;

    public class ConsoleLog : AbstractWindow, IListWindow
    {
        public int consoleSize = 200;

        private static ConsoleLog _Instance;
        private GameObject _ScrollBar;
        private GameObject _Viewport;
        private ListTupleContainer _TupleContainer;
        private ScrollRect _ScrollRect;
        private HashSet<EventType> _Ignored;
        private event ListChangeHandler ContentChanged;

        public static ConsoleLog Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = Instantiate(UIObjectPool.GetTemplate(WindowType.Console)).GetComponent<ConsoleLog>();
                    _Instance.transform.SetParent(UICanvas, false);
                }
                return _Instance;
            }
            private set
            {
                _Instance = value;
            }
        }

        protected GameObject ElementTemplate
        {
            get
            {
                return UIObjectPool.GetTemplate(ListElement.Console);
            }
        }

        protected GameObject ScrollBar
        {
            get
            {
                if (_ScrollBar == null)
                {
                    _ScrollBar = ContentPanel.transform.Find("Scroll Bar").gameObject;
                }
                return _ScrollBar;
            }
        }

        protected GameObject Viewport
        {
            get
            {
                if (_Viewport == null)
                {
                    _Viewport = TupleContainer.transform.parent.gameObject;
                }
                return _Viewport;
            }
        }

        protected ScrollRect ScrollRect
        {
            get
            {
                if (_ScrollRect == null)
                {
                    _ScrollRect = ContentPanel.GetComponent<ScrollRect>();
                }
                return _ScrollRect;
            }
        }

        protected HashSet<EventType> Ignored
        {
            get
            {
                if (_Ignored == null)
                {
                    _Ignored = new HashSet<EventType>();
                }
                return _Ignored;
            }
        }

        protected override Vector2 MaximizedSize { get; } = new Vector2(900, 300);

        protected override Vector2 MinimizedSize
        {
            get
            {
                var top = -Viewport.transform.ToRect().offsetMax.y;
                var bot = Viewport.transform.ToRect().offsetMin.y;
                var size = transform.ToRect().sizeDelta;
                size.y = ElementTemplate.transform.ToRect().sizeDelta.y + top + bot;
                return size;
            }
        }

        protected override Button MaximizeButton
        {
            get
            {
                if (_MaximizeButton == null)
                {
                    _MaximizeButton = transform.Find("Minimize Button").GetComponent<Button>();
                }
                return _MaximizeButton;
            }
        }

        protected override Button MinimizeButton
        {
            get
            {
                if (_MinimizeButton == null)
                {
                    _MinimizeButton = transform.Find("Minimize Button").GetComponent<Button>();
                }
                return _MinimizeButton;
            }
        }

        public override WindowType Type { get; } = WindowType.Console;

        public event ListChangeHandler ListChanged
        {
            add
            {
                if (ContentChanged == null || !ContentChanged.GetInvocationList().Contains(value))
                {
                    ContentChanged += value;
                }
            }
            remove
            {
                ContentChanged -= value;
            }
        }

        public ListTupleContainer TupleContainer
        {
            get
            {
                if (_TupleContainer == null)
                {
                    _TupleContainer = ContentPanel.GetComponentInChildren<ListTupleContainer>();
                }
                return _TupleContainer;
            }
        }

        public ListElement TupleType { get; } = ListElement.Console;

        protected override void Awake()
        {
            Instance = this;
            MinimizeButton.onClick.AddListener(MinimizeWindow);
            MaximizeButton.onClick.AddListener(MaximizeWindow);

            foreach (EventType type in Enum.GetValues(typeof(EventType)))
            {
                if (!Ignored.Contains(type))
                {
                    SimulationEvent.RegisterListener(type, WriteToConsole);
                }
            }
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => UIObjectPool.Initialized);
            MinimizeWindow();
        }

        private void OnEnable()
        {
            StartCoroutine(Start());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void WriteToConsole(IEvent iEvent)
        {
            Button button;
            ConsoleElement element = null;
            if (TupleContainer.transform.childCount >= consoleSize)
            {
                button = TupleContainer.transform.GetChild(0).GetComponent<Button>();
                element = TupleContainer.transform.GetChild(0).GetComponent<ConsoleElement>();
                button.onClick.RemoveAllListeners();
            } 
            else
            {
                element = (ConsoleElement) UIObjectPool.Get(TupleType, TupleContainer.transform);
                TupleContainer.AdjustDimensions();
                button = element.Link;
                ListChanged += element.OnListChange;
            }

            button.onClick.AddListener(delegate 
            { 
                transform.SetAsLastSibling(); 
                ExecuteButton(iEvent);
            });

            button.name = iEvent.ID;
            button.transform.SetParent(_TupleContainer.transform);
            element.Message.SetText(iEvent.Message);
            ContentChanged.Invoke();
        }

        private void ExecuteButton(IEvent iEvent)
        {
            if (iEvent.Target != null)
            {
                var target = iEvent.Target;
                AbstractCamera.LookHere(new Vector3(target[0], target[1], target[2]));
            }

            iEvent.OpenWindow();
        }

        protected override void MinimizeWindow()
        {
            ScrollRect.ScrollToBottom();
            ScrollRect.vertical = false;
            ScrollBar.SetActive(false);
            transform.ToRect().sizeDelta = MinimizedSize;
            MaximizeButton.gameObject.SetActive(true);
            MinimizeButton.gameObject.SetActive(false);
            Viewport.transform.ToRect().offsetMax += Vector2.right * MaximizeButton.transform.ToRect().offsetMax.y;
        }

        protected override void MaximizeWindow()
        {
            ScrollRect.vertical = true;
            ScrollBar.SetActive(true);
            transform.ToRect().sizeDelta = MaximizedSize;
            MaximizeButton.gameObject.SetActive(false);
            MinimizeButton.gameObject.SetActive(true);
            Viewport.transform.ToRect().offsetMax -= Vector2.right * MaximizeButton.transform.ToRect().offsetMax.y;
        }

    }

}