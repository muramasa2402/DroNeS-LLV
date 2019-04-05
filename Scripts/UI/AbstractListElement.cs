using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

namespace Drones.UI
{
    using static Singletons;
    using EventSystem;
    using Utils;

    public abstract class AbstractListElement : MonoBehaviour, IListItemColour
    {
        private readonly static Dictionary<WindowType, ListElement> _WindowToList =
            new Dictionary<WindowType, ListElement>
            {
                {WindowType.Console, ListElement.Console},
                {WindowType.DroneList, ListElement.DroneList},
                {WindowType.HubList, ListElement.HubList},
                {WindowType.JobHistory, ListElement.JobHistory},
                {WindowType.JobQueue, ListElement.JobQueue},
            };

        private AbstractWindow _Window;
        public AbstractWindow Window
        {
            get
            {
                if (_Window == null)
                {
                    _Window = AbstractWindow.GetWindow(transform.parent);
                }
                return _Window;
            }
        }

        public Color Odd { get; } = ListItemOdd;
        public Color Even { get; } = ListItemEven;

        private Image _ItemImage;
        public Image ItemImage
        {
            get
            {
                if (_ItemImage == null)
                {
                    _ItemImage = gameObject.GetComponent<Image>();
                }
                return _ItemImage;
            }

        }
        public ListElement Type
        {
            get
            {
                return _WindowToList[Window.Type];
            }
        }

        protected virtual void Awake()
        {
            //TODO maybe remove this from SimulationEvent and put it in the window itself
            SimulationEvent.RegisterListener(EventType.ListUpdate, OnListChange);
        }

        public void SetColor()
        {
            ItemImage.color = (transform.GetSiblingIndex() % 2 == 1) ? Odd : Even;
        }

        public void OnListChange(IEvent e)
        {
            SetColor();
        }

        protected virtual void OnDisable()
        {
            if (Window != null)
            {
                SimulationEvent.Invoke(EventType.ListUpdate, new ListUpdate("Element Disabled", Window.Type));
            }
        }

        public override bool Equals(object other)
        {
            return other is AbstractListElement && GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            return GetInstanceID();
        }
    }
}
