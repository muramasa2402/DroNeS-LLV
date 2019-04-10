using UnityEngine;
using System.Linq;

namespace Drones
{
    using DataStreamer;
    using Drones.UI;
    using Drones.Utils;
    using Drones.Utils.Extensions;
    public delegate void JobStatusAlert(Drone drone);

    public class Drone : MonoBehaviour, IDronesObject, IDataSource
    {
        #region Fields
        private event JobStatusAlert JobAssignmentChange;
        private Job _AssignedJob;
        private Hub _AssignedHub;
        private Battery _AssignedBattery;
        private bool _InHub = true;
        private SecureSet<IDataSource> _CompletedJobs;
        private SecureSet<ISingleDataSourceReceiver> _Connections;
        #endregion

        #region IDataSource
        public AbstractInfoWindow InfoWindow { get; set; }

        public SecureSet<ISingleDataSourceReceiver> Connections
        {
            get
            {
                if (_Connections == null)
                {
                    _Connections = new SecureSet<ISingleDataSourceReceiver>
                    {
                        MemberCondition = (ISingleDataSourceReceiver obj) => obj is ListTuple || obj is DroneWindow
                    };
                }
                return _Connections;
            }
        }

        public int TotalConnections
        {
            get
            {
                return Connections.Count;
            }
        }

        public string[] GetData(WindowType windowType)
        {
            return new string[1];
        }

        public void OpenInfoWindow()
        {
            if (InfoWindow == null)
            {
                InfoWindow = (DroneWindow)UIObjectPool.Get(WindowType.Drone, Singletons.UICanvas);
                InfoWindow.Source = this;
                Connections.Add(InfoWindow);
            }
            else
            {
                InfoWindow.transform.SetAsLastSibling();
            }

        }
        #endregion

        #region IDronesObject
        public string Name { get; set; }

        public Job AssignedJob
        {
            get
            {
                return _AssignedJob;
            }
            set
            {
                bool failed = JobProgress < 99.9f;
                _AssignedJob = value;
                JobAssignmentChange?.Invoke(this);
                if (_AssignedJob == null && !failed)
                {
                    JobStatus = JobStatus.Completed;
                }
                else if (_AssignedJob == null && !failed)
                {
                    JobStatus = JobStatus.Failed;
                    FailedJobs++;
                }
            }
        }

        public Hub AssignedHub
        {
            get
            {
                return _AssignedHub;
            }
            set
            {
                if (_AssignedHub != null)
                {
                    _AssignedHub.Drones.Remove(this);
                    JobChange -= _AssignedHub.OnDroneJobAssignmentChange;
                }
                _AssignedHub = value;
                if (_AssignedHub != null)
                {
                    _AssignedHub.Drones.Add(this);
                    JobChange += _AssignedHub.OnDroneJobAssignmentChange;
                }
            }
        }

        public Drone AssignedDrone { get { return this; } }
        #endregion

        #region Properties
        public event JobStatusAlert JobChange
        {
            add
            {
                if (JobAssignmentChange == null || !JobAssignmentChange.GetInvocationList().Contains(value))
                {
                    JobAssignmentChange += value;
                }
            }
            remove
            {
                JobAssignmentChange -= value;
            }
        }

        public bool InHub
        {
            get
            {
                return _InHub;
            }
            set
            {
                if (HubDistance < 1.0f)
                {
                    _InHub = value;
                }
            }
        }

        public bool IsIdle
        {
            get
            {
                return JobStatus == JobStatus.Completed && InHub;
            }
        }

        // Red: Delayed, yellow in progress, green completed and currently idle
        public JobStatus JobStatus { get; private set; }  

        public DroneMovement Movement { get; private set; }

        public Battery AssignedBattery
        {
            get
            {
                return _AssignedBattery;
            }
            set
            {
                if (InHub && value.AssignedHub == AssignedHub)
                {
                    _AssignedBattery = value;
                }
            }
        }

        public SecureSet<IDataSource> CompletedJobs
        {
            get
            {
                if (_CompletedJobs == null)
                {
                    _CompletedJobs = new SecureSet<IDataSource>
                    {
                        MemberCondition = (IDataSource obj) => { return obj is Job; }
                    };
                }
                return _CompletedJobs;
            }
        }

        public Vector2 Position 
        {
            get
            {
                return transform.position.ToCoordinates();
            }
        }

        public float HubDistance 
        {
            get
            {
                if (AssignedHub != null)
                {
                    var d = Vector3.Distance(transform.position, AssignedHub.transform.position);
                    return StaticFunc.UnityToMetre(d);
                }
                return float.NaN;
            }
        }

        public int JobProgress
        {
            get
            {
                if (AssignedJob != null)
                {
                    float a = StaticFunc.CoordDistance(Position, AssignedJob.Destination);
                    float b = StaticFunc.CoordDistance(AssignedJob.Origin, AssignedJob.Destination);
                    return (int)Mathf.Clamp(100 * a / b, 0, 100);
                }
                return 0;
            }
        }

        public int FailedJobs { get; private set; } = 0;
        #endregion

        private void OnDisable()
        {
            Connections.Clear();
        }

        private void OnReachingOrigin()
        {
            JobStatus = JobStatus.InProgress;
        }

        private void OnRecevingJob()
        {
            JobStatus = JobStatus.PickUp;
            AssignedHub.IdleDrones.Remove(this);
        }

        private void OnReachingHub()
        {
            if (IsIdle)
            {
                AssignedHub.IdleDrones.Add(this);
            }
        }

        public override bool Equals(object other)
        {
            return other is Drone && GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            return GetInstanceID();
        }
    };
}
