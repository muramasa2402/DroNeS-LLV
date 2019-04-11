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
                if (!_JobQueue.WindowState)
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
                if (!_HubList.WindowState)
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
                if (!_DroneList.WindowState)
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
                if (!_NFZList.WindowState)
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
                JobQueue = (JobQueueWindow)UIObjectPool.Get(WindowType.JobQueue, UICanvas);
                JobQueue.Sources = SimManager.AllIncompleteJobs;
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
                HubList = (HubListWindow)UIObjectPool.Get(WindowType.HubList, UICanvas);
                JobQueue.Sources = SimManager.AllHubs;
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
                DroneList = (DroneListWindow)UIObjectPool.Get(WindowType.DroneList, UICanvas);
                JobQueue.Sources = SimManager.AllDrones;
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
                NFZList = (NoFlyZoneListWindow)UIObjectPool.Get(WindowType.NFZList, UICanvas);
                JobQueue.Sources = SimManager.AllNFZ;
            }
            else
            {
                NFZList.transform.SetAsLastSibling();
            }
        }

    }
}
