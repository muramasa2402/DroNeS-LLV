using UnityEngine.UI;

namespace Drones.UI
{
    using Utils.Extensions;
    using Drones.Utils;

    public class JobWindow : AbstractInfoWindow
    {
        private Button _JobQueue;
        private Button _JobHistory;
        private Button _GoToOrigin;
        private Button _GoToDestination;
        
        public Button JobQueue
        {
            get
            {
                if (_JobQueue == null)
                {
                    _JobQueue = transform.Find("List Button").GetComponent<Button>();
                }
                return _JobQueue;
            }
        }

        public Button JobHistory
        {
            get
            {
                if (_JobHistory == null)
                {
                    _JobHistory = transform.Find("History Button").GetComponent<Button>();
                }
                return _JobHistory;
            }
        }

        public Button GoToOrigin
        {
            get
            {
                if (_GoToOrigin == null)
                {
                    _GoToOrigin = transform.Find("Origin").Find("Goto Button").GetComponent<Button>();
                }
                return _GoToOrigin;
            }
        }

        public Button GoToDestination
        {
            get
            {
                if (_GoToDestination == null)
                {
                    _GoToDestination = transform.Find("Dest.").Find("Goto Button").GetComponent<Button>();
                }
                return _GoToDestination;
            }
        }

        public override System.Type DataSourceType { get; } = typeof(Job);

        public override WindowType Type { get; } = WindowType.Job;

        protected override void Start()
        {
            base.Start();

            GetComponentInChildren<StatusSwitch>().OnStatusChange += ChangeButton;

            GoToOrigin.onClick.AddListener(delegate
            {
                var position = ((Job)Source).Origin.ToUnity();
                position.y = 0;
                Functions.LookHere(position);
                Functions.HighlightPosition(position);
            });

            GoToDestination.onClick.AddListener(delegate
            {
                var position = ((Job)Source).Destination.ToUnity();
                position.y = 0;
                Functions.LookHere(position);
                Functions.HighlightPosition(position);
            });
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            GetComponentInChildren<StatusSwitch>().OnStatusChange -= ChangeButton;
        }

        private void ChangeButton(Status status)
        {
            if (status == Status.Null)
            {
                _JobQueue.gameObject.SetActive(false);
                _JobHistory.gameObject.SetActive(false);
            }
            else if (status == Status.SemiActive)
            {
                _JobQueue.gameObject.SetActive(true);
                _JobHistory.gameObject.SetActive(false);
            }
            else
            {
                _JobQueue.gameObject.SetActive(false);
                _JobHistory.gameObject.SetActive(true);
            }
        }


    }

}
