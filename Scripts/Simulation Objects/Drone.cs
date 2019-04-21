using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Drones
{

    using DataStreamer;
    using Drones.Interface;
    using Drones.UI;
    using Drones.Utils;
    using Drones.Utils.Extensions;
    public class Drone : MonoBehaviour, IDronesObject, IDataSource, IPoolable
    {
        private static uint _Count;

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
            SimManager.AllDrones.Add(UID, this);
            transform.SetParent(parent);
            gameObject.SetActive(true);
            Movement = DroneMovement.Idle;
            CollisionOn = false;
        }
        #endregion

        #region IDataSource
        public bool IsDataStatic { get; } = false;

        public AbstractInfoWindow InfoWindow { get; set; }

        public SecureSortedSet<int, ISingleDataSourceReceiver> Connections
        {
            get
            {
                if (_Connections == null)
                {
                    _Connections = new SecureSortedSet<int, ISingleDataSourceReceiver>((x, y) => (x.OpenTime <= y.OpenTime) ? -1 : 1)
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

        private readonly string[] infoOutput = new string[28];
        private readonly string[] listOutput = new string[4];

        public string[] GetData(WindowType windowType)
        {
            if (windowType == WindowType.Drone)
            {
                infoOutput[0] = Name;
                infoOutput[1] = AssignedHub.Name;
                infoOutput[2] = StaticFunc.CoordString(Waypoint.ToCoordinates());
                infoOutput[3] = UnitConverter.Convert(Length.m, StaticFunc.UnityToMetre(transform.position.y));
                if (AssignedBattery != null)
                {
                    infoOutput[4] = AssignedBattery.Charge.ToString("0.000");
                    infoOutput[5] = AssignedBattery.Capacity.ToString("0.000");
                }
                else
                {
                    for (int i = 4; i < 6; i++)
                    {
                        infoOutput[i] = "0.000";
                    }
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
                else
                {
                    for (int i = 6; i < 12; i++)
                    {
                        infoOutput[i] = "";
                    }
                    infoOutput[12] = "0.000";
                }

                //TODO Statistics
                infoOutput[13] = _DeliveryCount.ToString();
                infoOutput[14] = UnitConverter.Convert(Mass.kg, _PackageWeight);
                infoOutput[15] = UnitConverter.Convert(Length.km, _DistanceTravelled);
                float tmp = UnitConverter.ConvertValue(Mass.kg, _PackageWeight);
                tmp /= UnitConverter.ConvertValue(Length.km, _DistanceTravelled);
                infoOutput[16] = tmp.ToString("0.000") + " " + Mass.kg + "/" + Length.km;
                infoOutput[17] = UnitConverter.Convert(Energy.kWh, _Energy);
                infoOutput[18] = _BatterySwaps.ToString();
                infoOutput[19] = _HubHandovers.ToString();
                infoOutput[20] = UnitConverter.Convert(Chronos.min, AudibleDuration);

                //Averages
                infoOutput[21] = UnitConverter.Convert(Mass.kg, _PackageWeight / _DeliveryCount);
                infoOutput[22] = UnitConverter.Convert(Length.km, _DistanceTravelled / _DeliveryCount);
                infoOutput[23] = UnitConverter.Convert(Chronos.min, _TotalDelay);
                infoOutput[24] = UnitConverter.Convert(Energy.kWh, _Energy / _DeliveryCount);
                tmp = _BatterySwaps;
                tmp /= _DeliveryCount;
                infoOutput[25] = tmp.ToString();
                tmp = _HubHandovers;
                tmp /= _DeliveryCount;
                infoOutput[26] = tmp.ToString();
                infoOutput[27] = UnitConverter.Convert(Chronos.min, AudibleDuration);

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
                Connections.Add(InfoWindow.UID, InfoWindow);
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
                if ((_AssignedJob == null && value != null) || (_AssignedJob != null && value == null))
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
                if (value != null)
                {
                    _AssignedHub = value;
                    _HubHandovers++;
                }
            }
        }

        public Drone AssignedDrone => this;
        #endregion

        #region Fields
        private Job _AssignedJob;
        private Hub _AssignedHub;
        private AudioSensor _Sensor;
        private SecureSortedSet<uint, IDataSource> _CompletedJobs;
        private SecureSortedSet<int, ISingleDataSourceReceiver> _Connections;
        private FlightStatus _state = FlightStatus.Idle;
        private Queue<Vector3> _waypoints;
        private Battery _AssignedBattery;
        // Statistics
        private uint _DeliveryCount;
        private float _PackageWeight;
        private float _DistanceTravelled;
        private float _Energy;
        private uint _BatterySwaps;
        private uint _HubHandovers;
        private float _TotalDelay;
        #endregion

        #region Drone Properties
        private float AudibleDuration => Sensor.TotalTime;

        public AudioSensor Sensor
        {
            get
            {
                if (_Sensor == null)
                {
                    _Sensor = GetComponentInChildren<AudioSensor>();
                }
                return _Sensor;
            }
        }

        public bool InHub { get; private set; }

        public bool IsFree => AssignedJob == null;

        public SecureSortedSet<uint, IDataSource> CompletedJobs
        {
            get
            {
                if (_CompletedJobs == null)
                {
                    _CompletedJobs = new SecureSortedSet<uint, IDataSource>
                        ((x, y) => (((Job)x).CompletedAt >= ((Job)y).CompletedAt) ? -1 : 1)
                    {
                        MemberCondition = (IDataSource obj) => { return obj is Job; }
                    };
                    _CompletedJobs.ItemAdded += (obj) => SimManager.AllCompleteJobs.Add(obj.UID, obj);
                    _CompletedJobs.ItemAdded += (obj) => _DeliveryCount++;
                    _CompletedJobs.ItemAdded += (obj) => _PackageWeight += ((Job)obj).PackageWeight;
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

        public DroneMovement Movement { get; private set; } = DroneMovement.Idle;

        public int FailedJobs { get; private set; } = 0;

        public Battery AssignedBattery
        {
            get
            {
                return _AssignedBattery;
            }
            set
            {
                _AssignedBattery = value;
                if (_AssignedBattery != null) _BatterySwaps++;
            }
        }

        public bool CollisionOn { get; private set; }

        public bool IsWaiting { get; set; }

        public float Height { get; private set; } = 110;

        public float MaxSpeed { get; private set; } = 15f;

        private Vector3 PreviousWaypoint { get; set; }

        public Vector3 Waypoint { get; private set; }
        #endregion

        public void OnTriggerEnter(Collider other)
        {
            Hub hub = other.GetComponent<Hub>();

            if (hub != null && hub == AssignedHub)
            {
                Debug.Log("Collision Off");
                InHub = true;
                IsWaiting = true;
                CollisionOn = false;
            }
            if (CollisionOn && other.gameObject.layer != 10)
            {
                Debug.Log("Collided");
                DroneManager.movementHandle.Complete();
                AssignedHub.DestroyDrone(this, other);

            }
        }

        public void OnTriggerExit(Collider other)
        {
            Hub hub = other.GetComponent<Hub>();
            if (hub != null)
            {
                Debug.Log("Collision On");
                CollisionOn = true;
                InHub = false;
            }
        }

        private void Ascend(float y)
        {
            if (Movement == DroneMovement.Hover)
            {
                Movement = DroneMovement.Ascend;
                Height = y;
            }
            else Debug.Log("Cannot ascend during unfinished move commmand.");
        }

        private void Descend(float y)
        {
            if (Movement == DroneMovement.Hover)
            {
                Movement = DroneMovement.Descend;
                Height = y;
            }
            else Debug.Log("Cannot descend during unfinished move commmand.");
        }

        private void MoveTo(Vector3 waypoint)
        {
            if (Movement == DroneMovement.Hover)
            {
                Movement = DroneMovement.Horizontal;
                _DistanceTravelled += Vector3.Distance(PreviousWaypoint, Waypoint);
                PreviousWaypoint = Waypoint;
                Waypoint = waypoint;
            }
            else Debug.Log("Cannot move during unfinished move commmand.");
        }

        private bool ReachedWaypoint()
        {
            Vector3 a = transform.position;
            Vector3 b = Waypoint;
            a.y = b.y = 0;

            return Vector3.Distance(a, b) < 0.1f;
        }

        public void NavigateWaypoints(List<Vector3> waypoints)
        {
            _waypoints = new Queue<Vector3>(waypoints);
            float height = _waypoints.Peek().y;

            Movement = DroneMovement.Hover;
            _state = FlightStatus.PreparingHeight;
            if (transform.position.y > height)
            {
                Descend(height);
            }
            else
            {
                Ascend(height);
            }
        }

        public void UpdateEnergy(float dE) => _Energy += dE;

        public void UpdateDelay(float dt) => _TotalDelay += dt;

        void ChangeState()
        {
            if (_state == FlightStatus.PreparingHeight)
            {
                _state = FlightStatus.AwatingWaypoint;
            }

            if (_state == FlightStatus.AwatingWaypoint)
            {
                if (_waypoints.Count > 0)
                {
                    Waypoint = _waypoints.Dequeue();
                    MoveTo(Waypoint);
                }
                else
                {
                    //_state = InHub ? FlightStatus.Idle : FlightStatus.AwatingWaypoint;
                    //Movement = InHub ? DroneMovement.Idle : DroneMovement.Hover;
                    _state = FlightStatus.Idle;
                    Movement = DroneMovement.Idle;
                }
            }
        }

        void LateUpdate()
        {
            if (Movement == DroneMovement.Ascend && transform.position.y >= Height)
            {
                Movement = DroneMovement.Hover;
            }
            else if (Movement == DroneMovement.Descend && transform.position.y <= Height)
            {
                Movement = DroneMovement.Hover;
            }
            else if (Movement == DroneMovement.Horizontal && ReachedWaypoint())
            {
                Movement = DroneMovement.Hover;
            }

            if (Movement == DroneMovement.Hover)
            {
                ChangeState();
            }
        }
    };
}
