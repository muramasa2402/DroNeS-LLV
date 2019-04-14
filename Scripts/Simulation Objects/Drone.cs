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
        private static uint _Count;

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
        public bool IsDataStatic { get; } = false;

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

        private readonly string[] infoOutput = new string[29];
        private readonly string[] listOutput = new string[4];

        public string[] GetData(WindowType windowType)
        {
            if (windowType == WindowType.Drone)
            {
                infoOutput[0] = Name;
                infoOutput[1] = AssignedHub.Name;
                infoOutput[2] = StaticFunc.CoordString(Waypoint);
                infoOutput[3] = UnitConverter.Convert(Length.m, StaticFunc.UnityToMetre(transform.position.y));
                if (AssignedBattery != null)
                {
                    infoOutput[4] = AssignedBattery.Charge.ToString("0.000");
                    infoOutput[5] = AssignedBattery.Capacity.ToString("0.000");
                }
                if (AssignedJob != null)
                {
                    infoOutput[6] = AssignedJob.Name;
                    infoOutput[7] = StaticFunc.CoordString(AssignedJob.Origin);
                    infoOutput[8] = StaticFunc.CoordString(AssignedJob.Destination);
                    infoOutput[9] = AssignedJob.Deadline.ToString();
                    infoOutput[10] = UnitConverter.Convert(Mass.g, AssignedJob.PackageWeight);
                    infoOutput[11] = "$" + AssignedJob.ExpectedEarnings.ToString("0.00");
                    infoOutput[12] = JobProgress.ToString("0.000");
                }
                    
                //TODO Statistics

                return infoOutput;
            }
            if (windowType == WindowType.DroneList)
            {
                listOutput[0] = Name;
                listOutput[1] = AssignedHub.Name;
                if (AssignedJob != null)
                {
                    listOutput[2] = StaticFunc.CoordString(AssignedJob.Origin);
                    listOutput[3] = StaticFunc.CoordString(AssignedJob.Destination);
                }
                return listOutput;
            }
            throw new ArgumentException("Wrong Window Type Supplied!");
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
        public string Name { get; private set; }

        public uint UID { get; private set; }

        public Job AssignedJob
        {
            get
            {
                return _AssignedJob;
            }
            set
            {
                if ((_AssignedJob == null && value !=null) || (_AssignedJob != null && value == null))
                {
                    _AssignedJob = value;
                    AssignedHub.OnDroneJobAssign(this);
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
                }
                _AssignedHub = value;
                if (_AssignedHub != null)
                {
                    _AssignedHub.Drones.Add(this);
                }
            }
        }

        public Drone AssignedDrone => this;
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
                if (HubDistance < 5.0f) // 5 m
                {
                    _InHub = value;
                }
            }
        }

        public bool IsFree
        {
            get
            {
                return AssignedJob == null;
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
                if (InHub && (value == null || (_AssignedBattery == null && value.AssignedHub == AssignedHub)))
                {
                    if (_AssignedBattery != null)
                    {
                        _AssignedBattery.AssignedDrone = null;
                        AssignedHub.OnBatteryUnassign(_AssignedBattery);
                    }
                    _AssignedBattery = value;
                    if (_AssignedBattery != null)
                    {
                        _AssignedBattery.AssignedDrone = this;
                        AssignedHub.OnBatteryAssign(_AssignedBattery);
                    }
                } 
                else if (!InHub && value == null)
                {
                    _AssignedBattery = null;
                }
            }
        }

        public Vector2 Waypoint { get; set; }

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

        public float JobProgress
        {
            get
            {
                if (AssignedJob != null)
                {
                    float a = StaticFunc.CoordDistance(Position, AssignedJob.Destination);
                    float b = StaticFunc.CoordDistance(AssignedJob.Origin, AssignedJob.Destination);
                    return Mathf.Clamp(a / b, 0, 1);
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
            AssignedBattery = null;
            SimManager.AllDrones.Remove(this);
            AssignedHub = null;

            CompletedJobs.Clear();
            Connections.Clear();
            gameObject.SetActive(false);
            transform.SetParent(ObjectPool.PoolContainer);
        }

        public void OnGet(Transform parent = null)
        {
            UID = _Count++;
            Name = "D" + UID.ToString("000000");
            FailedJobs = 0;
            SimManager.AllDrones.Add(this);
            transform.SetParent(parent);
            gameObject.SetActive(true);
            Movement = DroneMovement.Idle;
        }
        #endregion

        public override bool Equals(object other)
        {
            return other is Drone && GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    };
}
