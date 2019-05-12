using UnityEngine.UI;
using UnityEngine;

namespace Drones.UI
{
    using Drones.Utils;

    public class DroneWindow : AbstractInfoWindow
    {
        public static DroneWindow New() => PoolController.Get(WindowPool.Instance).Get<DroneWindow>(null);

        [SerializeField]
        private Button _FollowDrone;
        [SerializeField]
        private Button _GoToHub;
        [SerializeField]
        private Button _JobHistory;
        [SerializeField]
        private Button _JobInfo;
        [SerializeField]
        private Button _GoToOrigin;
        [SerializeField]
        private Button _GoToDestination;

        #region Properties
        private Button FollowDrone
        {
            get
            {
                if (_FollowDrone == null)
                {
                    _FollowDrone = ContentPanel.transform.Find("Name").GetComponentInChildren<Button>();
                }
                return _FollowDrone;
            }
        }
        private Button GoToHub
        {
            get
            {
                if (_GoToHub == null)
                {
                    _GoToHub = ContentPanel.transform.Find("Hub").GetComponentInChildren<Button>();
                }
                return _GoToHub;
            }
        }
        private Button JobHistory
        {
            get
            {
                if (_JobHistory == null)
                {
                    _JobHistory = ContentPanel.transform.Find("History Button").GetComponent<Button>();
                }
                return _JobHistory;
            }
        }
        private Button JobInfo
        {
            get
            {
                if (_JobInfo == null)
                {
                    _JobInfo = ContentPanel.transform.Find("MoreInfo Button").GetComponent<Button>();
                }
                return _JobInfo;
            }
        }
        private Button GoToOrigin
        {
            get
            {
                if (_GoToOrigin == null)
                {
                    _GoToOrigin = ContentPanel.transform.Find("Origin").GetComponentInChildren<Button>();
                }
                return _GoToOrigin;
            }
        }
        private Button GoToDestination
        {
            get
            {
                if (_GoToDestination == null)
                {
                    _GoToDestination = ContentPanel.transform.Find("Dest.").GetComponentInChildren<Button>();
                }
                return _GoToDestination;
            }
        }
        #endregion

        protected override Vector2 MaximizedSize { get; } = new Vector2(450, 650);

        protected override void Awake()
        {
            base.Awake();
            GoToOrigin.onClick.AddListener(delegate
            {
                var position = ((Drone)Source).AssignedJob.Pickup;
                AbstractCamera.LookHere(position);
            });

            GoToDestination.onClick.AddListener(delegate
            {
                var position = ((Drone)Source).AssignedJob.Dest;
                AbstractCamera.LookHere(position);
            });
            GoToHub.onClick.AddListener(delegate
            {
                var position = ((Drone)Source).AssignedHub.transform.position;
                AbstractCamera.LookHere(position);
            });

            FollowDrone.onClick.AddListener(delegate
            {
                AbstractCamera.Followee = ((Drone)Source).gameObject;
            });

            JobInfo.onClick.AddListener(OpenJobWindow);
            JobHistory.onClick.AddListener(OpenJobHistoryWindow);
        }

        void OpenJobWindow() => ((Drone)Source).AssignedJob?.OpenInfoWindow();

        void OpenJobHistoryWindow()
        {
            var jhw = JobHistoryWindow.New();
            jhw.WindowName.SetText(((Drone)Source).Name);
            jhw.Sources = ((Drone)Source).CompletedJobs;
            jhw.Opener = OpenJobHistoryWindow;
            jhw.CreatorEvent = JobHistory.onClick;
            JobHistory.onClick.RemoveAllListeners();
            JobHistory.onClick.AddListener(jhw.transform.SetAsLastSibling);
        }


    }

}
