using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

namespace Drones.UI
{
    using Utils;
    using Interface;

    public abstract class AbstractListElement : MonoBehaviour, IListElement, IPoolable
    {
        #region Statics
        private readonly static Dictionary<WindowType, ListElement> _WindowToList =
            new Dictionary<WindowType, ListElement>
            {
                {WindowType.Console, ListElement.Console},
                {WindowType.DroneList, ListElement.DroneList},
                {WindowType.HubList, ListElement.HubList},
                {WindowType.JobHistory, ListElement.JobHistory},
                {WindowType.JobQueue, ListElement.JobQueue},
                {WindowType.NFZList, ListElement.NFZList}
            };
        public static Color ListItemOdd { get; } = new Color
        {
            r = 180f / 255f,
            b = 180f / 255f,
            g = 180f / 255f,
            a = 1
        };

        public static Color ListItemEven { get; } = new Color
        {
            r = 223f / 255f,
            b = 223f / 255f,
            g = 223f / 255f,
            a = 1
        };
        #endregion

        #region Fields
        private AbstractWindow _Window;
        private Image _ItemImage;
        #endregion

        #region Properties
        public ListElement Type => _WindowToList[Window.Type];

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
        #endregion

        #region IPoolable
        public virtual void OnGet(Transform parent)
        {
            gameObject.SetActive(true);
            transform.SetParent(parent, false);
        }
        public virtual void OnRelease()
        {
            gameObject.SetActive(false);
            transform.SetParent(UIObjectPool.PoolContainer, false);
        }
        public virtual void SelfRelease()
        {
            UIObjectPool.Release(Type, this);
        }
        #endregion

        #region IListElementColour
        public Color Odd { get; } = ListItemOdd;

        public Color Even { get; } = ListItemEven;

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

        public void SetColor()
        {
            ItemImage.color = (transform.GetSiblingIndex() % 2 == 1) ? Odd : Even;
        }

        public void OnListChange()
        {
            SetColor();
        }
        #endregion
    }
}
