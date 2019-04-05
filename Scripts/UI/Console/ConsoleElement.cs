namespace Drones.UI
{

    public class ConsoleElement : AbstractListElement
    {
        private DataField _Message;

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
