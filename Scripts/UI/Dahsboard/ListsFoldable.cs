namespace Drones.UI
{
    using Drones.Utils;
    using static Singletons;
    public class ListsFoldable : FoldableMenu
    {
        private JobQueueWindow _JobQueue;
        private HubListWindow _HubList;
        private DroneListWindow _DroneList;
        private NoFlyZoneListWindow _NFZList;

        #region Properties
        public JobQueueWindow JobQueue
        {
            get
            {
                if (!_JobQueue.gameObject.activeSelf)
                {
                    return null;
                }
                return _JobQueue;
            }
            private set
            {
                _JobQueue = value;
            }
        }

        public HubListWindow HubList
        {
            get
            {
                if (!_HubList.gameObject.activeSelf)
                {
                    return null;
                }
                return _HubList;
            }
            private set
            {
                _HubList = value;
            }
        }

        public DroneListWindow DroneList
        {
            get
            {
                if (!_DroneList.gameObject.activeSelf)
                {
                    return null;
                }
                return _DroneList;
            }
            private set
            {
                _DroneList = value;
            }
        }

        public NoFlyZoneListWindow NFZList
        {
            get
            {
                if (!_NFZList.gameObject.activeSelf)
                {
                    return null;
                }
                return _NFZList;
            }
            private set
            {
                _NFZList = value;
            }
        }
        #endregion

        protected override void Start()
        {
            Buttons[0].onClick.AddListener(OpenWholeJobQueue);
            Buttons[1].onClick.AddListener(OpenWholeHubList);
            Buttons[2].onClick.AddListener(OpenWholeDroneList);
            Buttons[3].onClick.AddListener(OpenWholeNFZList);
            base.Start();
        }

        private void OpenWholeJobQueue()
        {
            if (JobQueue == null)
            {
                JobQueue = (JobQueueWindow)UIPool.Get(WindowType.JobQueue, UICanvas);
                // TODO Need global Job Queue
                // JobQueue.Sources = 
            }
            else
            {
                JobQueue.transform.SetAsLastSibling();
            }
        }

        private void OpenWholeHubList()
        {
            if (HubList == null)
            {
                HubList = (HubListWindow)UIPool.Get(WindowType.HubList, UICanvas);
                // TODO Need global Hub list
                // HubList.Sources = 
            }
            else
            {
                HubList.transform.SetAsLastSibling();
            }
        }

        private void OpenWholeDroneList()
        {
            if (DroneList == null)
            {
                DroneList = (DroneListWindow) UIPool.Get(WindowType.DroneList, UICanvas);
                // TODO Need global drone list
                // DroneList.Sources = 
            }
            else
            {
                DroneList.transform.SetAsLastSibling();
            }
        }

        private void OpenWholeNFZList()
        {
            if (NFZList == null)
            {
                NFZList = (NoFlyZoneListWindow)UIPool.Get(WindowType.NFZList, UICanvas);
                // TODO Need global drone list
                // DroneList.Sources = 
            }
            else
            {
                NFZList.transform.SetAsLastSibling();
            }
        }

    }
}
