using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Drones.UI
{
    using static Singletons;
    using EventSystem;
    using Utils.Extensions;
    using Utils;

    public class ConsoleLog : AbstractWindow
    {
        public int consoleSize = 20;

        private GameObject _ScrollBar;
        private GameObject _Viewport;
        private Transform _ElementsParent;
        private ScrollRect _ScrollRect;
        private HashSet<EventType> _Ignored;
        protected GameObject ElementTemplate
        {
            get
            {
                return UIPool.PeekTemplate(ListElement.Console).gameObject;
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
                    _Viewport = ElementsParent.parent.gameObject;
                }
                return _Viewport;
            }
        }

        protected Transform ElementsParent
        {
            get
            {
                if (_ElementsParent == null)
                {
                    _ElementsParent = ContentPanel.transform.FindChildWithTag("ListElementParent");
                }
                return _ElementsParent;
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

        protected override void Awake()
        {
            MinimizeButton.GetComponent<Button>().onClick.AddListener(MinimizeWindow);
            MaximizeButton.GetComponent<Button>().onClick.AddListener(MaximizeWindow);

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
            var wait = new WaitUntil(() => !UIPool.Initializing);
            yield return wait;
            MinimizeWindow();
        }

        private void WriteToConsole(IEvent iEvent)
        {
            if (!iEvent.ToConsole) { return; }

            Button button;
            if (ElementsParent.childCount >= consoleSize)
            {
                button = ElementsParent.GetChild(0).GetComponent<Button>();
                button.onClick.RemoveAllListeners();
            } 
            else
            {
                button = UIPool.Get(ListElement.Console, ElementsParent).gameObject.GetComponent<Button>();
            }

            button.onClick.AddListener(delegate 
            { 
                transform.SetAsLastSibling(); 
                ExecuteButton(iEvent);
            });

            button.name = iEvent.ID;
            button.transform.SetParent(ElementsParent);
            button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(iEvent.Message);
            SimulationEvent.Invoke(EventType.ListUpdate, new ListUpdate("Logged to Console", Type));
        }

        private void ExecuteButton(IEvent iEvent)
        {
            if (iEvent.Target != null)
            {
                var target = iEvent.Target;
                var position = new Vector3(target[0], 0, target[2]);
                Functions.LookHere(position);
            }

            if (iEvent.Window != WindowType.Null)
            {
                //TODO Open Window (Should be Info Window only)
            }
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