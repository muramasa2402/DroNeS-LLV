using UnityEngine.UI;
using UnityEngine;
using System.Collections;

namespace Drones.UI
{
    using Utils.Extensions;
    using Drones.Utils;

    public class DroneWindow : AbstractInfoWindow
    {
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
        [SerializeField]
        private StatusSwitch _StatusSwitch;

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
        private StatusSwitch StatusSwitch
        {
            get
            {
                if (_StatusSwitch == null)
                {
                    _StatusSwitch = ContentPanel.transform.GetComponentInChildren<StatusSwitch>();
                }
                return _StatusSwitch;
            }
        }
        #endregion

        public override System.Type DataSourceType { get; } = typeof(Drone);

        protected override void OnDisable()
        {
            base.OnDisable();
            FollowDrone.onClick.RemoveAllListeners();
            GoToHub.onClick.RemoveAllListeners();
            JobHistory.onClick.RemoveAllListeners();
            JobInfo.onClick.RemoveAllListeners();
            GoToOrigin.onClick.RemoveAllListeners();
            GoToDestination.onClick.RemoveAllListeners();
        }

        public override WindowType Type { get; } = WindowType.Drone;

        protected override void Awake()
        {
            base.Awake();
            GoToOrigin.onClick.AddListener(delegate
            {
                var position = ((Drone)Source).AssignedJob.Origin.ToUnity();
                AbstractCamera.LookHere(position);
            });

            GoToDestination.onClick.AddListener(delegate
            {
                var position = ((Drone)Source).AssignedJob.Destination.ToUnity();
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

        void OpenJobWindow() 
        {
            ((Drone)Source).AssignedJob.OpenInfoWindow();
        }

        void OpenJobHistoryWindow()
        {
            var jhw = (JobHistoryWindow)UIObjectPool.Get(WindowType.JobHistory, Singletons.UICanvas);
            jhw.Sources = ((Drone)Source).CompletedJobs;
            jhw.Opener = OpenJobHistoryWindow;
            jhw.CreatorEvent = JobHistory.onClick;
            JobHistory.onClick.RemoveAllListeners();
            JobHistory.onClick.AddListener(jhw.transform.SetAsLastSibling);
        }



    }

}
