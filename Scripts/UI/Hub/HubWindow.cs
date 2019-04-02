using UnityEngine.UI;

namespace Drones.UI
{
    using Drones.DataStreamer;
    using Utils;
    using Utils.Extensions;
    public class HubWindow : AbstractInfoWindow
    {
        private Button _GoToLocation;
        private Button _ShowDroneList;

        public Button GoToLocation
        {
            get
            {
                if (_GoToLocation == null)
                {
                    _GoToLocation = transform.Find("Goto Button").GetComponent<Button>();
                }
                return _GoToLocation;
            }
        }

        public Button ShowDroneList
        {
            get
            {
                if (_ShowDroneList == null)
                {
                    _ShowDroneList = transform.Find("List Button").GetComponent<Button>();
                }
                return _ShowDroneList;
            }
        }

        public override System.Type DataSourceType { get; } = typeof(Hub);

        public override WindowType Type { get; } = WindowType.Hub;

        protected override void Start()
        {
            base.Start();

            GoToLocation.onClick.AddListener(delegate
            {
                var position = ((Job)Source).Origin.ToUnity();
                position.y = 0;
                Functions.LookHere(position);
                Functions.HighlightPosition(position);
            });

            //TODO Show Drone List
        }

    }

}
