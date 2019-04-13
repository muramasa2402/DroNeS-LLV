using UnityEngine.UI;
using UnityEngine;
using System.Collections;

namespace Drones.UI
{
    using Utils.Extensions;
    using Drones.Utils;

    public class DestroyedDroneWindow : AbstractInfoWindow
    {
        [SerializeField]
        private Button _GoToHub;
        [SerializeField]
        private Button _CollidedWith;
        [SerializeField]
        private Button _JobHistory;
        [SerializeField]
        private Button _JobInfo;
        [SerializeField]
        private Button _GoToOrigin;
        [SerializeField]
        private Button _GoToDestination;

        #region Properties
        private Button CollidedWith
        {
            get
            {
                if (_CollidedWith == null)
                {
                    _CollidedWith = ContentPanel.transform.Find("Name").GetComponentInChildren<Button>();
                }
                return _CollidedWith;
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

        public override System.Type DataSourceType { get; } = typeof(DestroyedDrone);

        public override WindowType Type { get; } = WindowType.DestroyedDrone;

        protected override void Awake()
        {
            base.Awake();
            GoToOrigin.onClick.AddListener(delegate
            {
                var position = ((DestroyedDrone)Source).AssignedJob.Origin.ToUnity();
                AbstractCamera.LookHere(position);
            });

            GoToDestination.onClick.AddListener(delegate
            {
                var position = ((DestroyedDrone)Source).AssignedJob.Destination.ToUnity();
                AbstractCamera.LookHere(position);
            });
            GoToHub.onClick.AddListener(delegate
            {
                var position = ((DestroyedDrone)Source).AssignedHub.transform.position;
                AbstractCamera.LookHere(position);
            });

            CollidedWith.onClick.AddListener(delegate
            {
                ((DestroyedDrone)Source).CollidedWith?.OpenInfoWindow();
            });

            JobInfo.onClick.AddListener(OpenJobWindow);

            JobHistory.onClick.AddListener(OpenJobHistoryWindow);
        }

        void OpenJobWindow()
        {
            ((DestroyedDrone)Source).AssignedJob.OpenInfoWindow();
        }

        void OpenJobHistoryWindow()
        {
            var jhw = (JobHistoryWindow)UIObjectPool.Get(WindowType.JobHistory, Singletons.UICanvas);
            jhw.Sources = ((DestroyedDrone)Source).CompletedJobs;
            jhw.Opener = OpenJobHistoryWindow;
            jhw.CreatorEvent = JobHistory.onClick;
            JobHistory.onClick.RemoveAllListeners();
            JobHistory.onClick.AddListener(jhw.transform.SetAsLastSibling);
        }



    }

}
