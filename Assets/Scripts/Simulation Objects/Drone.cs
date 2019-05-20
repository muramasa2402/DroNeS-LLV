using UnityEngine;
using System;
using System.Collections.Generic;

namespace Drones
{
    using Managers;
    using DataStreamer;
    using Interface;
    using Serializable;
    using UI;
    using Utils;
    using Utils.Extensions;
    using System.Collections;

    public class Drone : MonoBehaviour, IDataSource, IPoolable
    {
        private static Transform _ActiveDrones;
        public static Transform ActiveDrones
        {
            get
            {
                if (_ActiveDrones == null)
                {
                    _ActiveDrones = new GameObject
                    {
                        name = "ActiveDrones"
                    }.transform;
                    DontDestroyOnLoad(_ActiveDrones.gameObject);

                }
                return _ActiveDrones;
            }
        }
        public override string ToString() => Name;

        public const float DroneAndBatteryMass = 22.5f;

        public static void Reset() => _Count = 0;

        private static uint _Count;

        public static Drone New() => PoolController.Get(ObjectPool.Instance).Get<Drone>(null);

        public static Drone Load(SDrone data)
        {
            var d = PoolController.Get(ObjectPool.Instance).Get<Drone>(null, true);
            d.gameObject.SetActive(true);
            d.LoadState(data);
            d.LoadAssignments(data);

            return d;
        }

        #region IPoolable
        public PoolController PC() => PoolController.Get(ObjectPool.Instance);
        public void Delete() => PC().Release(GetType(), this);

        public void OnRelease()
        {
            StopAllCoroutines();
            InPool = true;
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
            transform.SetParent(PC().PoolParent);
            StopCoroutine(PollRoute());
        }

        public void OnGet(Transform parent = null)
        {
            UID = ++_Count;
            Trail.enabled = true;
            Name = "D" + UID.ToString("000000");
            SimManager.AllDrones.Add(UID, this);
            transform.SetParent(parent);
            gameObject.SetActive(true);
            Movement = DroneMovement.Idle;
            _state = FlightStatus.Idle;
            CollisionOn = false;
            JobManager.AddToQueue(this);
            InPool = false;
            PreviousPosition = transform.position;
            IsWaiting = true;
            StartCoroutine(PollRoute());
        }

        public bool InPool { get; private set; }
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
                    _Connections = new SecureSortedSet<int, ISingleDataSourceReceiver>
                    {
                        MemberCondition = (ISingleDataSourceReceiver obj) => obj is ObjectTuple || obj is DroneWindow
                    };
                }
                return _Connections;
            }
        }

        private readonly string[] infoOutput = new string[28];
        private readonly string[] listOutput = new string[4];

        public string[] GetData(Type windowType)
        {
            if (windowType == typeof(DroneWindow))
            {
                infoOutput[0] = Name;
                infoOutput[1] = AssignedHub.Name;
                infoOutput[2] = Waypoint.ToStringXZ();
                infoOutput[3] = UnitConverter.Convert(Length.m, transform.position.y);
                if (AssignedBattery != null)
                {
                    infoOutput[4] = AssignedBattery.Charge.ToString("0.000");
                    infoOutput[5] = AssignedBattery.Capacity.ToString("0.000");
                }
                else
                {
                    for (int i = 4; i < 6; i++) infoOutput[i] = "0.000";
                }

                if (AssignedJob != null)
                {
                    infoOutput[6] = AssignedJob.Name;
                    infoOutput[7] = AssignedJob.Pickup.ToStringXZ();
                    infoOutput[8] = AssignedJob.Dest.ToStringXZ();
                    infoOutput[9] = AssignedJob.Deadline.ToString();
                    infoOutput[10] = UnitConverter.Convert(Mass.g, AssignedJob.PackageWeight);
                    infoOutput[11] = "$" + AssignedJob.Earnings.ToString("0.00");
                    infoOutput[12] = JobProgress.ToString("0.000");
                } 
                else
                {
                    for (int i = 6; i < 12; i++)
                        infoOutput[i] = "";

                    infoOutput[12] = "0.000";
                }

                infoOutput[13] = DeliveryCount.ToString();
                infoOutput[14] = UnitConverter.Convert(Mass.kg, PackageWeight);
                infoOutput[15] = UnitConverter.Convert(Length.km, DistanceTravelled);
                float tmp = UnitConverter.ConvertValue(Mass.kg, PackageWeight);
                tmp /= UnitConverter.ConvertValue(Length.km, DistanceTravelled);
                infoOutput[16] = tmp.ToString("0.000") + " " + Mass.kg + "/" + Length.km;
                infoOutput[17] = UnitConverter.Convert(Energy.kWh, TotalEnergy);
                infoOutput[18] = BatterySwaps.ToString();
                infoOutput[19] = HubHandovers.ToString();
                infoOutput[20] = UnitConverter.Convert(Chronos.min, AudibleDuration);

                //Averages
                infoOutput[21] = UnitConverter.Convert(Mass.kg, PackageWeight / DeliveryCount);
                infoOutput[22] = UnitConverter.Convert(Length.km, DistanceTravelled / DeliveryCount);
                infoOutput[23] = UnitConverter.Convert(Chronos.min, TotalDelay);
                infoOutput[24] = UnitConverter.Convert(Energy.kWh, TotalEnergy / DeliveryCount);
                tmp = BatterySwaps;
                tmp /= DeliveryCount;
                infoOutput[25] = tmp.ToString();
                tmp = HubHandovers;
                tmp /= DeliveryCount;
                infoOutput[26] = tmp.ToString();
                infoOutput[27] = UnitConverter.Convert(Chronos.min, AudibleDuration);

                return infoOutput;
            }
            if (windowType == typeof(DroneListWindow))
            {
                listOutput[0] = Name;
                listOutput[1] = AssignedHub.Name;
                if (AssignedJob != null)
                {
                    listOutput[2] = AssignedJob.Pickup.ToStringXZ();
                    listOutput[3] = AssignedJob.Dest.ToStringXZ();
                }
                return listOutput;
            }
            throw new ArgumentException("Wrong Window Type Supplied!");
        }

        public void OpenInfoWindow()
        {
            if (InfoWindow == null)
            {
                InfoWindow = DroneWindow.New();
                InfoWindow.Source = this;
                Connections.Add(InfoWindow.UID, InfoWindow);
            }
            else
            {
                InfoWindow.transform.SetAsLastSibling();
            }

        }
        #endregion

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
                    OnJobAssign();
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
                    HubHandovers++;
                }
            }
        }

        #region Fields
        private Job _AssignedJob;
        private Hub _AssignedHub;
        private AudioSensor _Sensor;
        private SecureSortedSet<uint, IDataSource> _CompletedJobs;
        private SecureSortedSet<int, ISingleDataSourceReceiver> _Connections;
        private FlightStatus _state = FlightStatus.Idle;
        private Queue<Vector3> _waypoints = new Queue<Vector3>();
        private Battery _AssignedBattery;
        private Vector3 _PreviousWaypoint;
        private TrailRenderer _Trail;
        public static float minAlt = 150;
        public static float maxAlt = 480;
        #endregion

        #region Drone Properties
        public uint DeliveryCount { get; private set; }
        public float PackageWeight { get; private set; }
        public float DistanceTravelled { get; private set; }
        public uint BatterySwaps { get; private set; }
        public uint HubHandovers { get; private set; }
        public float TotalDelay { get; private set; }
        public float AudibleDuration { get; private set; }
        public float TotalEnergy { get; set; }

        public TrailRenderer Trail
        {
            get
            {
                if (_Trail == null)
                {
                    _Trail = GetComponent<TrailRenderer>();
                }
                return _Trail;
            }
        }
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

        public Vector3 PreviousPosition { get; set; }
        public SecureSortedSet<uint, IDataSource> CompletedJobs
        {
            get
            {
                if (_CompletedJobs == null)
                {
                    _CompletedJobs = new SecureSortedSet<uint, IDataSource>
                        ((x, y) => (((Job)x).CompletedOn >= ((Job)y).CompletedOn) ? -1 : 1)
                    {
                        MemberCondition = (IDataSource obj) => { return obj is Job; }
                    };
                    _CompletedJobs.ItemAdded += (obj) => SimManager.AllCompleteJobs.Add(obj.UID, obj);
                    _CompletedJobs.ItemAdded += (obj) => DeliveryCount++;
                    _CompletedJobs.ItemAdded += (obj) => PackageWeight += ((Job)obj).PackageWeight;
                }
                return _CompletedJobs;
            }
        }

        public Vector3 Position => transform.position;

        public float HubDistance
        {
            get
            {
                return (AssignedHub != null) ? Vector3.Distance(transform.position, AssignedHub.transform.position) : float.NaN;
            }
        }

        public float JobProgress
        {
            get
            {
                if (AssignedJob != null)
                {
                    if (AssignedJob.Status == JobStatus.Delivering)
                    {
                        float a = Vector3.Distance(Position, AssignedJob.Pickup);
                        float b = Vector3.Distance(AssignedJob.Pickup, AssignedJob.Dest);
                        return Mathf.Clamp(a / b, 0, 1);
                    }
                }
                return 0;
            }
        }

        public DroneMovement Movement { get; private set; } = DroneMovement.Idle;

        public Battery AssignedBattery
        {
            get => _AssignedBattery;

            set
            {
                _AssignedBattery = value;
                if (_AssignedBattery != null) BatterySwaps++;
            }
        }

        public bool CollisionOn { get; private set; }

        public bool IsWaiting { get; set; }

        public float TargetAltitude { get; private set; }

        public float MaxSpeed { get; private set; } = 22f;

        public Vector3 Waypoint { get; private set; }

        public SVector3 Direction
        {
            get
            {
                return Vector3.Normalize(PreviousPosition - transform.position);
            }
        }

        public bool FrequentRequests { get; private set; }
        #endregion

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("IgnoreCollision")) return;

            if (other.gameObject.layer != LayerMask.NameToLayer("Hub") && CollisionOn)
            {
                DroneManager.movementJobHandle.Complete();
                AssignedHub.DestroyDrone(this, other);
            } 
            else if (other.GetComponent<Hub>() == AssignedHub)
            {
                InHub = true;
                CollisionOn = false;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("IgnoreCollision")) return;

            if (other.gameObject.layer == LayerMask.NameToLayer("Hub")
                && other.GetComponent<Hub>() == AssignedHub)
            {
                CollisionOn = true;
                InHub = false;
                transform.SetParent(ActiveDrones);
            }
        }

        private void ChangeAltitude(float height)
        {
            if (Movement == DroneMovement.Hover)
            {
                Movement = (transform.position.y > height) ?
                    DroneMovement.Descend : DroneMovement.Ascend;
                TargetAltitude = height;
            }
        }

        private void MoveTo(Vector3 waypoint)
        {
            if (Movement == DroneMovement.Hover)
            {
                Movement = DroneMovement.Horizontal;
                DistanceTravelled += Vector3.Distance(_PreviousWaypoint, Waypoint);
                _PreviousWaypoint = Waypoint;
                Waypoint = waypoint;
            }
        }

        private bool ReachedWaypoint()
        {
            Vector3 a = transform.position;
            Vector3 b = Waypoint;
            a.y = b.y = 0;

            return Vector3.Distance(a, b) < 0.1f;
        }

        public void ProcessRoute(SRoute route)
        {
            FrequentRequests = route.frequentRequest;
            NavigateWaypoints(route.waypoints);
            Debug.Log(route.waypoints.Count);
            Debug.Log((Vector3)route.waypoints[0]);
        }

        public void NavigateWaypoints(List<SVector3> waypoints)
        {
            _waypoints = new Queue<Vector3>();
            foreach (SVector3 waypoint in waypoints)
            {
                _waypoints.Enqueue(waypoint);
            }
            if (InHub)
            {
                AssignedHub.ExitingDrones.Enqueue(this);
            }
            Movement = DroneMovement.Hover;
            _state = FlightStatus.PreparingHeight;
            ChangeAltitude(_waypoints.Peek().y);
        }

        public void UpdateDelay(float dt) => TotalDelay += dt;

        public void UpdateAudible(float dt) => AudibleDuration += dt;
        private bool _wasGoingDown;
        private bool _isGoingDown;
        void ChangeState()
        {
            if (_state == FlightStatus.PreparingHeight)
            {
                if (transform.position.y < 15f)
                {
                    if (AssignedJob != null && AssignedJob.Status == JobStatus.Delivering && _isGoingDown != _wasGoingDown)
                    {
                        AssignedJob.CompleteJob();
                        JobManager.AddToQueue(this);
                    }
                    else if (AssignedJob != null && AssignedJob.Status == JobStatus.Pickup && _isGoingDown != _wasGoingDown)
                    {
                        AssignedJob.StartDelivery();
                    }
                    RouteManager.AddToQueue(this);
                    return;
                }
                _state = FlightStatus.AwaitingWaypoint;

            }

            if (_state != FlightStatus.AwaitingWaypoint && _state != FlightStatus.Delivering) return;

            if (_waypoints.Count > 0)
            {
                _state = FlightStatus.Delivering;
                Waypoint = _waypoints.Dequeue();
                MoveTo(Waypoint);
                return;
            } 

            if (InHub)
            {
                var destination = Waypoint;
                destination.y = AssignedHub.Position.y;
                if (Vector3.Distance(Waypoint, AssignedHub.Position) < 0.1f)
                {
                    _state = FlightStatus.Idle;
                    Movement = DroneMovement.Idle;
                    AssignedHub.OnDroneReturn(this);
                }
                return;
            }

            if (AssignedJob != null)
            {
                if (AssignedJob.Status != JobStatus.Pickup && AssignedJob.Status != JobStatus.Delivering) return;

                Vector3 destination =
                    AssignedJob.Status == JobStatus.Pickup ? AssignedJob.Pickup :
                    AssignedJob.Status == JobStatus.Delivering ? AssignedJob.Dest :
                    Vector3.zero;

                destination.y = transform.position.y;
                if (Vector3.Distance(transform.position, destination) < 0.1f)
                {
                    destination.y = 10;
                    NavigateWaypoints(new List<SVector3> { destination });
                }
                else
                {
                    RouteManager.AddToQueue(this);
                }
            }
        }

        void LateUpdate()
        {
            _wasGoingDown = _isGoingDown;
            _isGoingDown = Movement == DroneMovement.Descend;
            if (Movement == DroneMovement.Ascend && transform.position.y >= TargetAltitude ||
                Movement == DroneMovement.Descend && transform.position.y <= TargetAltitude ||
                Movement == DroneMovement.Horizontal && ReachedWaypoint())
            {
                Movement = DroneMovement.Hover;
            }

            if (Movement == DroneMovement.Hover) ChangeState();

            if (Movement != DroneMovement.Idle && AssignedBattery.Status == BatteryStatus.Dead) Drop();
        }

        void Drop()
        {
            Trail.enabled = false;
            Movement = DroneMovement.Drop;
            if (AbstractCamera.Followee == gameObject) 
            {
                AbstractCamera.ActiveCamera.BreakFollow();
            }
        }

        IEnumerator PollRoute()
        {
            TimeKeeper.Chronos time = TimeKeeper.Chronos.Get();
            var wait1 = new WaitUntil(() => FrequentRequests);
            var wait2 = new WaitUntil(() => time.Timer() > 5);
            while (true)
            {
                yield return wait1;
                if (Movement == DroneMovement.Hover || Movement == DroneMovement.Horizontal)
                    RouteManager.AddToQueue(this);
                yield return wait2;
                time.Now();
            }
        }

        public void OnJobAssign()
        {
            if (AssignedJob != null)
            {
                AssignedJob.AssignedDrone = this;
                if (InHub)
                {
                    var d = Vector3.Normalize(AssignedJob.Pickup - Position) * 4;
                    d.y = 0;
                    transform.position += d;
                }
                RouteManager.AddToQueue(this);
            }
        }

        public SDrone Serialize()
        {
            var output = new SDrone
            {
                count = _Count,
                uid = UID,
                totalDeliveryCount = DeliveryCount,
                totalBatterySwaps = BatterySwaps,
                totalHubHandovers = HubHandovers,
                collisionOn = CollisionOn,
                isWaiting = IsWaiting,
                inHub = InHub,
                name = Name,
                movement = Movement,
                status = _state,
                totalDelay = TotalDelay,
                totalAudibleDuration = AudibleDuration,
                totalPackageWeight = PackageWeight,
                totalDistanceTravelled = DistanceTravelled,
                totalEnergy = TotalEnergy,
                targetAltitude = TargetAltitude,
                waypointsQueue = new List<SVector3>(),
                completedJobs = new List<uint>(),
                maxSpeed = MaxSpeed,
                position = transform.position,
                previousWaypoint = _PreviousWaypoint,
                waypoint = Waypoint,
                job = (AssignedJob == null) ? 0 : AssignedJob.UID,
                hub = (AssignedHub == null) ? 0 : AssignedHub.UID,
                hubPosition = (AssignedHub == null) ? 10000 * Vector3.one : AssignedHub.transform.position,
                battery = (AssignedBattery == null) ? 0 : AssignedBattery.UID,
                charge = (AssignedBattery == null) ? 0 : AssignedBattery.Charge
            };

            foreach (var point in _waypoints)
                output.waypointsQueue.Add(point);
            foreach (Job job in CompletedJobs.Values)
                output.completedJobs.Add(job.UID);

            return output;
        }

        public StrippedDrone Strip()
        {
            var output = new StrippedDrone
            {
                uid = UID,
                isWaiting = IsWaiting,
                inHub = InHub,
                movement = Movement,
                status = _state,
                targetAltitude = TargetAltitude,
                waypointsQueue = new List<SVector3>(),
                maxSpeed = MaxSpeed,
                position = transform.position,
                previousWaypoint = _PreviousWaypoint,
                waypoint = Waypoint,
                job = (AssignedJob == null) ? 0 : AssignedJob.UID,
                hub = (AssignedHub == null) ? 0 : AssignedHub.UID,
                hubPosition = (AssignedHub == null) ? 10000 * Vector3.one : AssignedHub.transform.position,
                battery = (AssignedBattery == null) ? 0 : AssignedBattery.UID,
                charge = (AssignedBattery == null) ? 0 : AssignedBattery.Charge
            };

            foreach (var point in _waypoints)
                output.waypointsQueue.Add(point);

            return output;
        }

        public Drone LoadState(SDrone data)
        {
            _Count = data.count;
            UID = data.uid;
            InPool = false;
            StartCoroutine(PollRoute());
            DeliveryCount = data.totalDeliveryCount;
            BatterySwaps = data.totalBatterySwaps;
            HubHandovers = data.totalHubHandovers;
            CollisionOn = data.collisionOn;
            IsWaiting = data.isWaiting;
            Name = data.name;
            Movement = data.movement;
            _state = data.status;
            TotalDelay = data.totalDelay;
            AudibleDuration = data.totalAudibleDuration;
            PackageWeight = data.totalPackageWeight;
            DistanceTravelled = data.totalDistanceTravelled;
            TotalEnergy = data.totalEnergy;
            TargetAltitude = data.targetAltitude;
            PreviousPosition = transform.position;
            _waypoints = new Queue<Vector3>();
            foreach (Vector3 point in data.waypointsQueue)
            {
                _waypoints.Enqueue(point);
            }
            Waypoint = data.waypoint;
            _PreviousWaypoint = data.previousWaypoint;
            transform.position = data.position;
            return this;
        }

        public Drone LoadAssignments(SDrone data)
        {
            _AssignedBattery = SimManager.AllBatteries[data.battery];
            _AssignedJob = (Job)SimManager.AllIncompleteJobs[data.job];
            if (CompletedJobs == null) { }
            foreach (uint id in data.completedJobs)
                _CompletedJobs.Add(id, SimManager.AllCompleteJobs[id]);
            DeliveryCount = data.totalDeliveryCount;
            PackageWeight = data.totalPackageWeight;

            if (_AssignedBattery != null)
                _AssignedBattery.AssignedDrone = this;
            if (_AssignedJob != null)
            {
                _AssignedJob.AssignedDrone = this;
                RouteManager.AddToQueue(this);
            }
            if (AssignedJob == null && !InHub)
            {
                transform.SetParent(ActiveDrones);
                JobManager.AddToQueue(this);
                RouteManager.AddToQueue(this);
            }
            return this;
        }

    };
}
