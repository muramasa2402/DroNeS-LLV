using UnityEngine;
using UnityEngine.UI;

namespace Drones.UI
{
    using Drones.Utils;
    public class ConsoleElement : AbstractListElement
    {
        public static ConsoleElement New(Transform parent) => (ConsoleElement)PoolController.Get(ListElementPool.Instance).Get(typeof(ConsoleLog), parent);

        [SerializeField]
        private DataField _Message;
        [SerializeField]
        private Button _Link;

        public Button Link
        {
            get
            {
                if (_Link == null)
                {
                    _Link = transform.GetChild(0).GetComponent<Button>();
                }
                return _Link;
            }
        }

        public DataField Message
        {
            get
            {
                if (_Message == null)
                {
                    _Message = gameObject.GetComponentInChildren<DataField>();
                }
                return _Message;
            }

        }
    }
}
