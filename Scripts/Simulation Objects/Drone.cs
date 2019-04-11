using UnityEngine;
using System.Linq;

namespace Drones
{
    using System;
    using DataStreamer;
    using Drones.Interface;
    using Drones.UI;
    using Drones.Utils;
    using Drones.Utils.Extensions;

    public class Drone : MonoBehaviour, IDronesObject, IDataSource, IPoolable
    {
        #region Fields
        private event Action<Drone> JobAssignmentChange;
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
                _AssignedJob = value;
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
                }
                _AssignedHub = value;
                if (_AssignedHub != null)
                {
                    _AssignedHub.Drones.Add(this);
                }
            }
        }

        public Drone AssignedDrone { get { return this; } }
        #endregion

        #region Properties
        public event Action<Drone> JobChange
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

        public bool IsFree
        {
            get
            {
                return JobStatus != JobStatus.InProgress && JobStatus != JobStatus.PickUp;
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
                    return StaticFunc.UnityToMetre(Vector3.Distance(transform.position, AssignedHub.transform.position));
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

        #region IPoolable
        public void SelfRelease()
        {
            ObjectPool.Release(this);
        }

        public void OnRelease()
        {
            StopAllCoroutines();
            InfoWindow?.Close.onClick.Invoke();
            if (AssignedJob != null)
            {
                AssignedJob.FailJob();
                AssignedJob = null;
            }
            if (AssignedBattery != null)
            {
                AssignedHub.ChargingBatteries.Add(AssignedBattery);
                AssignedBattery = null;
            }
            SimManager.AllDrones.Remove(this);
            AssignedHub = null;

            CompletedJobs.Clear();
            Connections.Clear();
            gameObject.SetActive(false);
            transform.SetParent(ObjectPool.PoolContainer);
        }

        public void OnGet(Transform parent = null)
        {
            FailedJobs = 0;
            SimManager.AllDrones.Add(this);
            transform.SetParent(parent);
            gameObject.SetActive(true);
        }
        #endregion

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
