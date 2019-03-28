using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Drones.UI
{
    using static SceneAttributes;
    using Struct;
    using Utils.Extensions;
    public class EventLogger : MonoBehaviour
    {
        public int consoleSize = 20;
        public int consoleHeight = 300;

        private GameObject _ButtonTemplate;
        [SerializeField]
        private GameObject _ScrollBar;
        [SerializeField]
        private GameObject _MaximizeButton;
        [SerializeField]
        private GameObject _MinimizeButton;
        [SerializeField]
        private GameObject _ConsolePanel;
        [SerializeField]
        private GameObject _Viewport;
        [SerializeField]
        private Transform _ConsoleElements;
        private ScrollRect _ScrollRect;
        private float _ButtonHeight;

        private float _Separation;

        private void Awake()
        {
            _ButtonTemplate = Resources.Load("Prefabs/UI/Console/ConsoleButton") as GameObject;
            if (_ConsolePanel == null)
            {
                _ConsolePanel = transform.Find("Contents Panel").gameObject;
            }
            if (_ScrollBar == null)
            {
                _ScrollBar = _ConsolePanel.transform.Find("Scroll Bar").gameObject;
            }
            if (_Viewport == null)
            {
                _Viewport = _ConsolePanel.transform.Find("Viewport").gameObject;
            }
            if (_ConsoleElements == null)
            {
                _ConsoleElements = _Viewport.transform.GetChild(0);
            }
            if (_MinimizeButton == null)
            {
                _MinimizeButton = transform.Find("Minimize Button").gameObject;
            }
            if (_MaximizeButton == null)
            {
                _MaximizeButton = transform.Find("Minimize Button").gameObject;
            }
            _ScrollRect = _ConsolePanel.GetComponent<ScrollRect>();
            _Separation = _ConsoleElements.GetComponent<VerticalLayoutGroup>().spacing;
            _ButtonHeight = _ButtonTemplate.transform.ToRect().sizeDelta.y;
        }

        private void Start()
        {
            _MinimizeButton.GetComponent<Button>().onClick.AddListener(MinimizeConsole);
            _MaximizeButton.GetComponent<Button>().onClick.AddListener(MaximizeConsole);
            MinimizeConsole();
        }

        public void LogEvent(SimulationEventInfo simulationEvent)
        {
            Events.Insert(0, simulationEvent);
            WriteToConsole(simulationEvent);
        }

        public void WriteToConsole(SimulationEventInfo simulationEvent)
        {
            if (_ConsoleElements.childCount >= consoleSize)
            {
                DestroyImmediate(_ConsoleElements.GetChild(0).gameObject);
            }

            var button = Instantiate(_ButtonTemplate) as GameObject;
            button.GetComponent<Button>().onClick.AddListener(delegate { ExecuteButton(simulationEvent); });
            button.name = simulationEvent.ID;
            button.transform.SetParent(_ConsoleElements);
            button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = simulationEvent.Message;

            var content = _ConsoleElements.ToRect();
            var y = content.childCount * (_ButtonHeight + _Separation) + _Separation;
            content.sizeDelta = new Vector2(content.sizeDelta.x, y);

            //foreach (Transform message in _ConsoleElements)
            //{
            //    message.localPosition += Vector3.up * rect.sizeDelta.y;
            //}


            //button.transform.localPosition = button.transform.position;
            //button.transform.localPosition += Vector3.up * _Separation;
            //button.transform.localScale = Vector3.one;
            //rect.offsetMin = new Vector2(0, rect.offsetMin.y);
            //rect.offsetMax = new Vector2(0, rect.offsetMax.y);



        }

        static void ExecuteButton(SimulationEventInfo simulationEvent)
        {
            if (simulationEvent.Target != null)
            {
                var target = simulationEvent.Target;
                var position = new Vector3(target[0], 0, target[2]);
                var back = -CamTrans.forward;
                CameraContainer.position = position + back * CameraContainer.position.y/back.y;
                if (CurrentPosition != null)
                {
                    Destroy(CurrentPosition);
                }
                CurrentPosition = Instantiate(Resources.Load("Prefabs/UI/PositionHighlight") as GameObject);
                CurrentPosition.name = "Current Position";
                CurrentPosition.transform.position = position;
                CurrentPosition.transform.position += Vector3.up * CurrentPosition.transform.lossyScale.y;
            }

            if (simulationEvent.WindowID > -1)
            {

            }
        }

        void MinimizeConsole()
        {
            _ScrollRect.ScrollToBottom();
            _ScrollRect.vertical = false;
            _ScrollBar.SetActive(false);
            var size = transform.ToRect().sizeDelta; 
            size.y = _ButtonTemplate.transform.ToRect().sizeDelta.y + 2 * _Separation;
            transform.ToRect().sizeDelta = size;
            _MaximizeButton.SetActive(true);
            _MinimizeButton.SetActive(false);
            _Viewport.transform.ToRect().offsetMax += Vector2.right * _MaximizeButton.transform.ToRect().offsetMax.y;
        }

        void MaximizeConsole()
        {
            _ScrollRect.vertical = true;
            _ScrollBar.SetActive(true);
            var size = transform.ToRect().sizeDelta;
            size.y = consoleHeight;
            transform.ToRect().sizeDelta = size;
            _MaximizeButton.SetActive(false);
            _MinimizeButton.SetActive(true);
            _Viewport.transform.ToRect().offsetMax -= Vector2.right * _MaximizeButton.transform.ToRect().offsetMax.y;
        }

    }

}