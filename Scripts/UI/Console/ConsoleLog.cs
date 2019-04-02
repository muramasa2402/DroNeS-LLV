using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
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
        public int consoleHeight = 300;

        private GameObject _ScrollBar;
        private GameObject _Viewport;
        private Transform _ElementsParent;
        private ScrollRect _ScrollRect;
        private GameObject _ButtonTemplate;

        public GameObject ButtonTemplate
        {
            get
            {
                if (_ButtonTemplate == null)
                {
                    _ButtonTemplate = Resources.Load("Prefabs/UI/Windows/Console/ConsoleElement") as GameObject;
                }
                return _ButtonTemplate;
            }
        }

        public GameObject ScrollBar
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

        public override Button MaximizeButton
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

        public override Button MinimizeButton
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

        public GameObject Viewport
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

        public Transform ElementsParent
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

        public ScrollRect ScrollRect
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

        public override WindowType Type { get; } = WindowType.Console;

        public static readonly HashSet<EventType> ignored = new HashSet<EventType>();

        private void Awake()
        {
            MinimizeButton.GetComponent<Button>().onClick.AddListener(MinimizeWindow);
            MaximizeButton.GetComponent<Button>().onClick.AddListener(MaximizeWindow);

            MinimizeWindow();

            foreach (EventType type in Enum.GetValues(typeof(EventType)))
            {
                if (!ignored.Contains(type))
                {
                    SimulationEvent.RegisterListener(type, WriteToConsole);
                }
            }
        }

        private void WriteToConsole(IEvent iEvent)
        {
            if (!iEvent.ToConsole) { return; }

            if (ElementsParent.childCount >= consoleSize)
            {
                Destroy(ElementsParent.GetChild(0).gameObject);
            }

            var button = Instantiate(ButtonTemplate) as GameObject;

            button.GetComponent<Button>().onClick.AddListener(delegate 
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
                Functions.HighlightPosition(position);
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
            var top = -Viewport.transform.ToRect().offsetMax.y;
            var bot = Viewport.transform.ToRect().offsetMin.y;
            var size = transform.ToRect().sizeDelta; 
            size.y = ButtonTemplate.transform.ToRect().sizeDelta.y + top + bot;
            transform.ToRect().sizeDelta = size;
            MaximizeButton.gameObject.SetActive(true);
            MinimizeButton.gameObject.SetActive(false);
            Viewport.transform.ToRect().offsetMax += Vector2.right * MaximizeButton.transform.ToRect().offsetMax.y;
        }

        protected override void MaximizeWindow()
        {
            ScrollRect.vertical = true;
            ScrollBar.SetActive(true);
            var size = transform.ToRect().sizeDelta;
            size.y = consoleHeight;
            transform.ToRect().sizeDelta = size;
            MaximizeButton.gameObject.SetActive(false);
            MinimizeButton.gameObject.SetActive(true);
            Viewport.transform.ToRect().offsetMax -= Vector2.right * MaximizeButton.transform.ToRect().offsetMax.y;
        }

    }

}