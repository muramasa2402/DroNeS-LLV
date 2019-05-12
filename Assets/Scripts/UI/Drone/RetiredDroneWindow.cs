using UnityEngine.UI;
using UnityEngine;
using System.Collections;

namespace Drones.UI
{
    using Drones.Utils;
    using static Singletons;

    public class RetiredDroneWindow : AbstractInfoWindow
    {
        public static RetiredDroneWindow New() => PoolController.Get(WindowPool.Instance).Get<RetiredDroneWindow>(null);

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

        protected override Vector2 MaximizedSize { get; } = new Vector2(425, 595);

        protected override void Awake()
        {
            base.Awake();
            GoToOrigin.onClick.AddListener(delegate
            {
                var position = ((RetiredDrone)Source).AssignedJob.Pickup;
                AbstractCamera.LookHere(position);
            });

            GoToDestination.onClick.AddListener(delegate
            {
                var position = ((RetiredDrone)Source).AssignedJob.Dest;
                AbstractCamera.LookHere(position);
            });
            CollidedWith.onClick.AddListener(delegate
            {
                ((RetiredDrone)Source).OtherDrone?.OpenInfoWindow();
            });

            JobInfo.onClick.AddListener(OpenJobWindow);

            JobHistory.onClick.AddListener(OpenJobHistoryWindow);
        }

        void OpenJobWindow()
        {
            ((RetiredDrone)Source).AssignedJob.OpenInfoWindow();
        }

        void OpenJobHistoryWindow()
        {
            var jhw = JobHistoryWindow.New();
            jhw.Sources = ((RetiredDrone)Source).CompletedJobs;
            jhw.Opener = OpenJobHistoryWindow;
            jhw.CreatorEvent = JobHistory.onClick;
            JobHistory.onClick.RemoveAllListeners();
            JobHistory.onClick.AddListener(jhw.transform.SetAsLastSibling);
        }

    }

}
