using UnityEngine.UI;
using UnityEngine;

namespace Drones.UI
{
    using static Singletons;
    using EventSystem;
    using Utils;
    public abstract class AbstractListElement : MonoBehaviour, IListItemColour
    {
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

        protected virtual void Awake()
        {
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
            SimulationEvent.Invoke(EventType.ListUpdate, new ListUpdate("Element Disabled", Window.Type));
        }
    }
}
