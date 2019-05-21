using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
namespace Drones
{
    using Managers;
    using DataStreamer;
    using Interface;
    using Serializable;
    using UI;
    using Utils;
    using Utils.Extensions;
    using Data;
    using Utils.Jobs;

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

        public uint UID => _Data.UID;

        public static Drone New() => PoolController.Get(ObjectPool.Instance).Get<Drone>(null);

        public static Drone Load(SDrone data)
        {
            var d = PoolController.Get(ObjectPool.Instance).Get<Drone>(null, true);
            d.gameObject.SetActive(true);

            return d.LoadState(data);
        }

        #region IPoolable
        public PoolController PC() => PoolController.Get(ObjectPool.Instance);
        public void Delete() => PC().Release(GetType(), this);

        public void OnRelease()
        {
            StopAllCoroutines();
            InPool = true;
            InfoWindow?.Close.onClick.Invoke();
            var job = GetJob();
            if (job != null)
            {
                job.FailJob();
                AssignJob(null);
            }
            GetBattery()?.Destroy();
            SimManager.AllDrones.Remove(this);
            _Data = null;
            gameObject.SetActive(false);
            transform.SetParent(PC().PoolParent);
            StopCoroutine(PollRoute());
        }

        public void OnGet(Transform parent = null)
        {
            _Data = new DroneData(this);
            Trail.enabled = true;
            SimManager.AllDrones.Add(_Data.UID, this);
            transform.SetParent(parent);
            gameObject.SetActive(true);
            JobManager.AddToQueue(this);
            InPool = false;
            StartCoroutine(PollRoute());
        }

        public bool InPool { get; private set; }
        #endregion

        #region IDataSource
        public bool IsDataStatic { get; } = false;

        public AbstractInfoWindow InfoWindow { get; set; }

        private readonly string[] infoOutput = new string[28];
        private readonly string[] listOutput = new string[4];

        public string[] GetData(Type windowType)
        {
            if (windowType == typeof(DroneWindow))
            {
                infoOutput[0] = Name;
                infoOutput[1] = GetHub().Name;
                infoOutput[2] = _Data.currentWaypoint.ToStringXZ();
                infoOutput[3] = UnitConverter.Convert(Length.m, transform.position.y);
                Battery battery = GetBattery();
                if (battery != null)
                {
                    infoOutput[4] = battery.Charge.ToString("0.000");
                    infoOutput[5] = battery.Capacity.ToString("0.000");
                }
                else
                {
                    for (int i = 4; i < 6; i++) infoOutput[i] = "0.000";
                }
                Job job = GetJob();
                if (job != null)
                {
                    infoOutput[6] = job.Name;
                    infoOutput[7] = job.Pickup.ToStringXZ();
                    infoOutput[8] = job.Dest.ToStringXZ();
                    infoOutput[9] = job.Deadline.ToString();
                    infoOutput[10] = UnitConverter.Convert(Mass.g, job.PackageWeight);
                    infoOutput[11] = "$" + job.Earnings.ToString("0.00");
                    infoOutput[12] = _Data.JobProgress.ToString("0.000");
                } 
                else
                {
                    for (int i = 6; i < 12; i++)
                        infoOutput[i] = "";

                    infoOutput[12] = "0.000";
                }

                infoOutput[13] = _Data.DeliveryCount.ToString();
                infoOutput[14] = UnitConverter.Convert(Mass.kg, _Data.packageWeight);
                infoOutput[15] = UnitConverter.Convert(Length.km, _Data.distanceTravelled);
                float tmp = UnitConverter.ConvertValue(Mass.kg, _Data.packageWeight);
                tmp /= UnitConverter.ConvertValue(Length.km, _Data.distanceTravelled);
                infoOutput[16] = tmp.ToString("0.000") + " " + Mass.kg + "/" + Length.km;
                infoOutput[17] = UnitConverter.Convert(Energy.kWh, _Data.totalEnergy);
                infoOutput[18] = _Data.batterySwaps.ToString();
                infoOutput[19] = _Data.hubHandovers.ToString();
                infoOutput[20] = UnitConverter.Convert(Chronos.min, _Data.audibleDuration);

                //Averages
                infoOutput[21] = UnitConverter.Convert(Mass.kg, _Data.packageWeight / _Data.DeliveryCount);
                infoOutput[22] = UnitConverter.Convert(Length.km, _Data.distanceTravelled / _Data.DeliveryCount);
                infoOutput[23] = UnitConverter.Convert(Chronos.min, _Data.totalDelay / _Data.DeliveryCount);
                infoOutput[24] = UnitConverter.Convert(Energy.kWh, _Data.totalEnergy / _Data.DeliveryCount);
                tmp = _Data.batterySwaps;
                tmp /= _Data.DeliveryCount;
                infoOutput[25] = tmp.ToString();
                tmp = _Data.hubHandovers;
                tmp /= _Data.DeliveryCount;
                infoOutput[26] = tmp.ToString();
                infoOutput[27] = UnitConverter.Convert(Chronos.min, _Data.audibleDuration);

                return infoOutput;
            }
            if (windowType == typeof(DroneListWindow))
            {
                listOutput[0] = Name;
                listOutput[1] = GetHub().Name;
                if (GetJob() != null)
                {
                    listOutput[2] = GetJob().Pickup.ToStringXZ();
                    listOutput[3] = GetJob().Dest.ToStringXZ();
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
            }
            else
            {
                InfoWindow.transform.SetAsLastSibling();
            }

        }
        #endregion

        public string Name => "D" + _Data.UID.ToString("000000");
        public void AssignJob(Job job)
        {
            if (_Data.job != 0 && job == null)
            {
                _Data.job = 0;
            }
            else if (_Data.job == 0 && job != null)
            {
                _Data.job = job.UID;
                job.AssignedDrone = this;
                if (InHub)
                {
                    var d = Vector3.Normalize(GetJob().Pickup - transform.position) * 4;
                    d.y = 0;
                    transform.position += d;
                }
            }
            RouteManager.AddToQueue(this);
        }
        public void AssignBattery(Battery battery)
        {

            if (battery == null)
            {
                _Data.batterySwaps++;
                _Data.battery = 0;
            }
            else
                _Data.battery = battery.UID;

        }
        public void AssignHub(Hub hub)
        {
            if (hub == null)
            {
                _Data.hubHandovers++;
                _Data.hub = 0;
            }
            else
                _Data.hub = hub.UID;
        }
        public void CompleteJob()
        {
            if (_Data.job != 0)
            {
                var job = GetJob();
                _Data.completedJobs.Add(_Data.job, job);
                UpdateDelay(job.Deadline.Timer());
                AssignJob(null);
            }
        }
        public Job GetJob() => (Job)SimManager.AllIncompleteJobs[_Data.job];
        public Battery GetBattery() => SimManager.AllBatteries[_Data.battery];
        public Hub GetHub() => (Hub)SimManager.AllHubs[_Data.hub];
        public void WaitForDeployment() => _Data.isWaiting = true;
        public void Deploy() => _Data.isWaiting = false;
        public void UpdateDelay(float dt) => _Data.totalDelay += dt;
        public void UpdateEnergy(float dE) => _Data.totalEnergy += dE;
        public void UpdateAudible(float dt) => _Data.audibleDuration += dt;
        public MovementInfo GetMovementInfo(MovementInfo info)
        {
            info.speed = _Data.MaxSpeed;
            info.moveType = _Data.movement;
            info.height = _Data.targetAltitude;
            info.waypoint = _Data.currentWaypoint;
            info.isWaiting = _Data.isWaiting ? 1 : 0;

            return info;
        }
        public EnergyInfo GetEnergyInfo(EnergyInfo info)
        {
            info.speed = _Data.MaxSpeed;
            info.moveType = _Data.movement;
            info.pkgXArea = (_Data.job == 0) ? 1 : GetJob().PackageXArea;
            info.pkgWgt = (_Data.job == 0) ? 0 : GetJob().PackageWeight;

            return info;
        }

        #region Fields
        private AudioSensor _Sensor;
        private TrailRenderer _Trail;
        private DroneData _Data;
        #endregion

        #region Drone Properties
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
        public bool InHub => _Data.inHub;
        public DroneMovement Movement => _Data.movement;
        public Vector3 Direction => _Data.Direction;
        public float JobProgress => _Data.JobProgress;
        public SecureSortedSet<uint, IDataSource> JobHistory => _Data.completedJobs;
        public Vector3 Waypoint => _Data.currentWaypoint;
        public Vector3 PreviousPosition
        {
            get => _Data.previousPosition;
            set => _Data.previousPosition = value;
        }

        #endregion

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("IgnoreCollision")) return;

            if (other.gameObject.layer != LayerMask.NameToLayer("Hub") && _Data.collisionOn)
            {
                DroneManager.movementJobHandle.Complete();
                GetHub().DestroyDrone(this, other);
            } 
            else if (other.GetComponent<Hub>() == GetHub())
            {
                _Data.inHub = true;
                _Data.collisionOn = false;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("IgnoreCollision")) return;

            if (other.gameObject.layer == LayerMask.NameToLayer("Hub")
                && other.GetComponent<Hub>() == GetHub())
            {
                _Data.collisionOn = true;
                _Data.inHub = false;
            }
        }

        private void ChangeAltitude(float height)
        {
            if (_Data.movement == DroneMovement.Hover)
            {
                _Data.movement = (transform.position.y > height) ? DroneMovement.Descend : DroneMovement.Ascend;
                _Data.targetAltitude = height;
            }
        }

        private void MoveTo(Vector3 waypoint)
        {
            if (_Data.movement == DroneMovement.Hover)
            {
                _Data.movement = DroneMovement.Horizontal;
                _Data.distanceTravelled += Vector3.Distance(_Data.previousWaypoint, _Data.currentWaypoint);
                _Data.previousWaypoint = _Data.currentWaypoint;
                _Data.currentWaypoint = waypoint;
            }
        }

        private bool ReachedWaypoint()
        {
            Vector3 a = transform.position;
            Vector3 b = _Data.currentWaypoint;
            a.y = b.y = 0;

            return Vector3.Distance(a, b) < 0.1f;
        }

        public void ProcessRoute(SRoute route)
        {
            _Data.frequentRequests = route.frequentRequest;
            NavigateWaypoints(route.waypoints);
        }

        public void NavigateWaypoints(List<SVector3> waypoints)
        {
            _Data.waypoints = new Queue<Vector3>();
            foreach (SVector3 waypoint in waypoints)
            {
                _Data.waypoints.Enqueue(waypoint);
            }
            if (InHub)
            {
                GetHub().ExitingDrones.Enqueue(this);
            }
            _Data.movement = DroneMovement.Hover;
            _Data.state = FlightStatus.PreparingHeight;
            ChangeAltitude(_Data.waypoints.Peek().y);
        }

        void ChangeState()
        {
            Job job = GetJob();
            if (_Data.state == FlightStatus.PreparingHeight)
            {
                if (transform.position.y < 15f)
                {

                    if (job != null && job.Status == JobStatus.Delivering && _Data.isGoingDown != _Data.wasGoingDown)
                    {
                        job.CompleteJob();
                        JobManager.AddToQueue(this);
                    }
                    else if (job != null && job.Status == JobStatus.Pickup && _Data.isGoingDown != _Data.wasGoingDown)
                    {
                        job.StartDelivery();
                    }
                    RouteManager.AddToQueue(this);
                    return;
                }
                _Data.state = FlightStatus.AwaitingWaypoint;

            }

            if (_Data.state != FlightStatus.AwaitingWaypoint && _Data.state != FlightStatus.Delivering) return;

            if (_Data.waypoints.Count > 0)
            {
                _Data.state = FlightStatus.Delivering;
                _Data.currentWaypoint = _Data.waypoints.Dequeue();
                MoveTo(_Data.currentWaypoint);
                return;
            } 

            if (InHub)
            {
                var destination = _Data.currentWaypoint;
                destination.y = GetHub().Position.y;
                if (Vector3.Distance(_Data.currentWaypoint, GetHub().Position) < 0.1f)
                {
                    _Data.state = FlightStatus.Idle;
                    _Data.movement = DroneMovement.Idle;
                    GetHub().OnDroneReturn(this);
                }
                return;
            }

            if (job != null)
            {
                if (job.Status != JobStatus.Pickup && job.Status != JobStatus.Delivering) return;

                Vector3 destination =
                    job.Status == JobStatus.Pickup ? job.Pickup :
                    job.Status == JobStatus.Delivering ? job.Dest :
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
            _Data.wasGoingDown = _Data.isGoingDown;
            _Data.isGoingDown = _Data.movement == DroneMovement.Descend;
            if (_Data.movement == DroneMovement.Ascend && transform.position.y >= _Data.targetAltitude ||
                _Data.movement == DroneMovement.Descend && transform.position.y <= _Data.targetAltitude ||
                _Data.movement == DroneMovement.Horizontal && ReachedWaypoint())
            {
                _Data.movement = DroneMovement.Hover;
            }

            if (_Data.movement == DroneMovement.Hover) ChangeState();

            if (_Data.movement != DroneMovement.Idle && GetBattery().Status == BatteryStatus.Dead) Drop();
        }

        void Drop()
        {
            Trail.enabled = false;
            _Data.movement = DroneMovement.Drop;
            if (AbstractCamera.Followee == gameObject)
                AbstractCamera.ActiveCamera.BreakFollow();
        }

        IEnumerator PollRoute()
        {
            TimeKeeper.Chronos time = TimeKeeper.Chronos.Get();
            var wait1 = new WaitUntil(() => _Data.frequentRequests);
            var wait2 = new WaitUntil(() => time.Timer() > 5);
            while (true)
            {
                yield return wait1;
                if (_Data.movement == DroneMovement.Hover || _Data.movement == DroneMovement.Horizontal)
                    RouteManager.AddToQueue(this);
                yield return wait2;
                time.Now();
            }
        }

        public SDrone Serialize() => new SDrone(_Data, this);

        public StrippedDrone Strip() => new StrippedDrone(_Data, this);

        public Drone LoadState(SDrone data)
        {
            _Data = new DroneData(data, this);
            InPool = false;
            transform.position = data.position;
            StartCoroutine(PollRoute());
            if (_Data.battery != 0)
                GetBattery().AssignedDrone = this;
            if (_Data.job != 0)
            {
                GetJob().AssignedDrone = this;
                RouteManager.AddToQueue(this);
            }
            else
            {
                JobManager.AddToQueue(this);
            }
            if (!InHub)
            {
                transform.SetParent(ActiveDrones);
                RouteManager.AddToQueue(this);
            }
            return this;
        }

    };
}
