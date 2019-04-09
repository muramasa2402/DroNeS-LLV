using UnityEngine;
using UnityEngine.UI;

namespace Drones.UI
{

    public class ConsoleElement : AbstractListElement
    {
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
