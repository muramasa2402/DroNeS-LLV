using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Drones
{
    using Managers;
    using Utils;
    using Utils.Extensions;
    using DataStreamer;
    using UI;
    using Interface;
    using Serializable;
    using Random = UnityEngine.Random;

    public class Hub : MonoBehaviour, IDataSource, IPoolable
    {
        public static uint _Count;
        public static void Reset() => _Count = 0;
        private static readonly float _DeploymentPeriod = 0.5f;
        public static Hub New() => PoolController.Get(ObjectPool.Instance).Get<Hub>(null);

        #region IDataSource
        public bool IsDataStatic { get; } = false;

        public SecureSortedSet<int, ISingleDataSourceReceiver> Connections
        {
            get
            {
                if (_Connections == null)
                {
                    _Connections = new SecureSortedSet<int, ISingleDataSourceReceiver>
                    {
                        MemberCondition = (ISingleDataSourceReceiver obj) => obj is ObjectTuple || obj is HubWindow
                    };
                }
                return _Connections;
            }
        }

        private readonly string[] infoOutput = new string[5];
        private readonly string[] listOutput = new string[4];

        public string[] GetData(Type windowType)
        {
            if (windowType == typeof(HubWindow))
            {
                infoOutput[0] = Name;
                infoOutput[1] = Position.ToStringXZ();
                infoOutput[2] = DroneCount.ToString();
                infoOutput[3] = BatteryCount.ToString();
                infoOutput[4] = UnitConverter.Convert(Energy.kWh, DroneEnergy);
                return infoOutput;
            } 
            if (windowType == typeof(HubListWindow))
            {
                listOutput[0] = Name;
                listOutput[1] = DroneCount.ToString();
                listOutput[2] = BatteryCount.ToString();
                listOutput[3] = Position.ToStringXZ();
                return listOutput;
            }
            throw new ArgumentException("Wrong Window Type Supplied");
        }

        public AbstractInfoWindow InfoWindow { get; set; }

        public void OpenInfoWindow()
        {
            if (InfoWindow == null)
            {
                InfoWindow = HubWindow.New();
                InfoWindow.Source = this;
                Connections.Add(InfoWindow.UID, InfoWindow);
            }
            else
            {
                InfoWindow.transform.SetAsLastSibling();
            }
        }

        #endregion


        public uint UID { get; private set; }

        public string Name { get; private set; }

        #region Fields
        private SecureSortedSet<int, ISingleDataSourceReceiver> _Connections;

        private SecureSortedSet<uint, IDataSource> _Drones;

        private SecureSortedSet<uint, Drone> _FreeDrones;

        private SecureSortedSet<uint, Battery> _ChargingBatteries;

        private SecureSortedSet<uint, Battery> _Batteries;

        private SecureSortedSet<uint, Battery> _FreeBatteries;

        private Queue<Drone> _ExitingDrones;

        [SerializeField]
        private PathClearer _DronePath;

        private int _rCount;
        #endregion

        #region IPoolable
        public PoolController PC() => PoolController.Get(ObjectPool.Instance);
        public bool InPool { get; private set; }

        public void Delete() => PC().Release(GetType(), this);

        public void OnRelease()
        {
            InPool = true;
            InfoWindow?.Close.onClick.Invoke();
            Connections.Clear();
            Drones.ReSort();
            while (Drones.Count > 0)
            {
                DestroyDrone((Drone)Drones.GetMin(false));
            }
            SimManager.AllHubs.Remove(this);
            gameObject.SetActive(false);
            transform.SetParent(PC().PoolParent);
            ExitingDrones.Clear();
        }

        public void OnGet(Transform parent = null)
        {
            InPool = false;
            UID = ++_Count;
            SimManager.AllHubs.Add(UID, this);
            Name = "H" + UID.ToString("000000");
            transform.SetParent(parent);
            gameObject.SetActive(true);
            DroneEnergy = 0;
            StartCoroutine(DeployDrone());
        }
        #endregion

        public Queue<Drone> ExitingDrones
        {
            get
            {
                if (_ExitingDrones == null)
                {
                    _ExitingDrones = new Queue<Drone>();
                }
                return _ExitingDrones;
            }
        }

        public Vector3 Position => transform.position;

        public int DroneCount => Drones.Count;

        public int FreeDroneCount => FreeDrones.Count;

        public int BatteryCount => Batteries.Count;

        public int ChargingBatteryCount => ChargingBatteries.Count;

        public PathClearer DronePath
        {
            get
            {
                if (_DronePath == null)
                {
                    _DronePath = transform.GetComponentInChildren<PathClearer>();
                }
                return _DronePath;
            }
        }

        public SecureSortedSet<uint, IDataSource> Drones
        {
            get
            {
                if (_Drones == null)
                {
                    _Drones = new SecureSortedSet<uint, IDataSource>
                    {
                        MemberCondition = (obj) => { return obj is Drone; }
                    };
                    _Drones.ItemAdded += delegate (IDataSource drone)
                    {
                        ((Drone)drone).AssignedHub = this;
                        SimManager.AllDrones.Add(drone.UID, drone);
                        FreeDrones.Add(drone.UID, (Drone)drone);
                    };
                    _Drones.ItemRemoved += delegate (IDataSource drone)
                    {
                        SimManager.AllDrones.Remove(drone);
                        FreeDrones.Remove((Drone)drone);
                    };
                }
                return _Drones;
            }
        }

        private SecureSortedSet<uint, Drone> FreeDrones
        {
            get
            {
                if (_FreeDrones == null)
                {
                    _FreeDrones = new SecureSortedSet<uint, Drone>
                    {
                        MemberCondition = (drone) => { return Drones.Contains(drone) && drone.AssignedJob == null; }
                    };
                    _FreeDrones.ItemAdded += (drone) =>
                    {
                        drone.transform.SetParent(transform);
                    };
                    _FreeDrones.ItemRemoved += (drone) =>
                    {
                        StopCharging(drone.AssignedBattery);
                        drone.transform.SetParent(null);
                    };
                }
                return _FreeDrones;
            }
        }

        private SecureSortedSet<uint, Battery> ChargingBatteries
        {
            get
            {
                if (_ChargingBatteries == null)
                {
                    _ChargingBatteries = new SecureSortedSet<uint, Battery>((x, y) => (x.Charge <= y.Charge) ? -1 : 1)
                    {
                        MemberCondition = (Battery obj) => { return Batteries.Contains(obj); }
                    };
                    _ChargingBatteries.ItemAdded += delegate (Battery bat)
                    {
                        bat.Status = BatteryStatus.Charge;
                        StartCoroutine(bat.ChargeBattery());
                    };
                    _ChargingBatteries.ItemRemoved += delegate (Battery bat)
                    {
                        bat.Status = BatteryStatus.Idle;
                        StopCoroutine(bat.ChargeBattery());
                    };
                }
                return _ChargingBatteries;
            }
        }

        private SecureSortedSet<uint, Battery> FreeBatteries
        {
            get
            {
                if (_FreeBatteries == null)
                {
                    _FreeBatteries = new SecureSortedSet<uint, Battery>((x, y) => (x.Charge <= y.Charge) ? -1 : 1)
                    {
                        MemberCondition = (Battery obj) => { return Batteries.Contains(obj) && obj.AssignedDrone == null; }
                    };
                }
                return _FreeBatteries;
            }
        }

        private SecureSortedSet<uint, Battery> Batteries
        {
            get
            {
                if (_Batteries == null)
                {
                    _Batteries = new SecureSortedSet<uint, Battery>();
                    _Batteries.ItemAdded += delegate (Battery bat)
                    {
                        SimManager.AllBatteries.Add(bat.UID, bat);
                        bat.AssignedHub = this;
                    };
                    _Batteries.ItemRemoved += delegate (Battery bat)
                    {
                        ChargingBatteries.Remove(bat);
                        FreeBatteries.Remove(bat);
                        SimManager.AllBatteries.Remove(bat);
                    };
                }
                return _Batteries;
            }
        }

        public float DroneEnergy { get; private set; }

        public void UpdateEnergy(float dE) => DroneEnergy += dE;

        private void OnTriggerEnter(Collider other)
        {
            Random.InitState(DateTime.Now.Millisecond);
            if (other.gameObject.layer == 14)
            {
                _rCount++;
                transform.position += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                StartCoroutine(Repulsion(other));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == 14 && _rCount > 0)
            {
                _rCount--;
            }
        }

        public IEnumerator Repulsion(Collider other)
        {
            StopCoroutine(DeployDrone());
            yield return null;
            Vector3 acc;
            Vector3 vel = Vector3.zero;
            float dt = 0;
            float dist;
            do
            {
                transform.position += vel * dt;
                dist = Vector3.Distance(transform.position, other.transform.position);
                float k = 1e8f / Mathf.Pow(dist, 2);
                if (other.CompareTag("NoFlyZone")) k *= 100;
                k = Mathf.Clamp(k, 0, 1e5f);
                acc = k * (transform.position - other.transform.position).normalized;
                acc.y = 0;
                dt = Time.deltaTime;
                vel = acc * dt;
                yield return null;
            } while (_rCount > 0);
            StartCoroutine(Reposition(vel.normalized));
            yield break;
        }

        public IEnumerator Reposition(Vector3 inDirection)
        {
            StopCoroutine(DeployDrone());
            Vector3.Normalize(inDirection);
            while (!DronePath.IsClear) // Building
            {
                transform.position += 5 * DronePath.Direction + inDirection;
                yield return null;
            }
            StartCoroutine(DeployDrone());
            yield break;
        }

        IEnumerator DeployDrone()
        {
            var time = TimeKeeper.Chronos.Get();
            WaitUntil _DroneReady = new WaitUntil(() => time.Timer() > _DeploymentPeriod);
            Drone outgoing = null;
            while (true)
            {

                if (!DronePath.IsClear) yield return null;
                if (ExitingDrones.Count > 0)
                {
                    outgoing = ExitingDrones.Dequeue();
                    if (outgoing.InPool) continue;
                    FreeDrones.Remove(outgoing);
                    GetBatteryForDrone(outgoing);
                    StopCharging(outgoing.AssignedBattery);
                    outgoing.IsWaiting = false;
                }
                yield return _DroneReady;
                time.Now();
            }
        }

        public override string ToString() => Name;

        #region Drone/Battery Interface
        public void OnDroneReturn(Drone drone)
        {
            drone.transform.SetParent(transform);
            if (drone != null && FreeDrones.Add(drone.UID, drone))
            {
                ChargingBatteries.Add(drone.AssignedBattery.UID, drone.AssignedBattery);
            }
            drone.IsWaiting = true;
        }

        private void RemoveBatteryFromDrone(Drone drone)
        {
            if (drone.AssignedHub == this && FreeBatteries.Add(drone.AssignedBattery.UID, drone.AssignedBattery))
            {
                ChargingBatteries.Add(drone.AssignedBattery.UID, drone.AssignedBattery);
                drone.AssignedBattery = null;
            }
        }

        public void ReassignDrone(Drone drone, Hub hub)
        {
            drone.AssignedHub = hub;
            Drones.Remove(drone);
            Batteries.Remove(drone.AssignedBattery);
            hub.Drones.Add(drone.UID, drone);
            hub.Batteries.Add(drone.AssignedBattery.UID, drone.AssignedBattery);
            drone.AssignedBattery.AssignedHub = hub;
        }

        public void StopCharging(Battery battery) => ChargingBatteries.Remove(battery);

        private void GetBatteryForDrone(Drone drone)
        {
            if (drone.AssignedBattery != null) return;

            if (DroneCount >= BatteryCount)
            {
                drone.AssignedBattery = BuyBattery(drone);
            }
            else
            {
                drone.AssignedBattery = FreeBatteries.GetMax(true);
                drone.AssignedBattery.AssignedDrone = drone;
            }
        }

        public void DestroyDrone(Drone drone, Collider other)
        {
            if (drone != null)
            {
                var dd = new RetiredDrone(drone, other);
                SimManager.AllRetiredDrones.Add(dd.UID, dd);
                drone.AssignedJob?.FailJob();
                Drones.Remove(drone);
                drone.AssignedBattery.Destroy();
                drone.Delete();
            }
        }

        public void DestroyDrone(Drone drone)
        {
            if (drone != null)
            {
                var dd = new RetiredDrone(drone);
                SimManager.AllRetiredDrones.Add(dd.UID, dd);
                Drones.Remove(drone);
                drone.AssignedBattery.Destroy();
                drone.Delete();
            }

        }

        public Drone BuyDrone()
        {
            Drone drone = Drone.New();
            drone.transform.position = transform.position;
            GetBatteryForDrone(drone);
            Drones.Add(drone.UID, drone);
            return drone;
        }

        public void SellDrone()
        {
            if (FreeDrones.Count > 0)
            {
                Drone drone = FreeDrones.GetMax(false);
                var dd = new RetiredDrone(drone, true);
                SimManager.AllRetiredDrones.Add(dd.UID, dd);
                Drones.Remove(drone);
                drone.Delete();
            }
        }

        public void DestroyBattery(Battery battery) => Batteries.Remove(battery);

        public Battery BuyBattery(Drone drone = null)
        {
            var bat = new Battery(1f, drone, this);
            Batteries.Add(bat.UID, bat);
            if (drone is null) FreeBatteries.Add(bat.UID, bat);
            return bat;
        }

        public void SellBattery()
        {
            if (FreeBatteries.Count > 0)
            {
                var bat = FreeBatteries.GetMin(true);
                Batteries.Remove(bat);
            }
        }
        #endregion

        public SHub Serialize()
        {
            var data = new SHub
            {
                count = _Count,
                uid = UID,
                batteries = new List<uint>(),
                freeBatteries = new List<uint>(),
                chargingBatteries = new List<uint>(),
                drones = new List<uint>(),
                freeDrones = new List<uint>(),
                position = transform.position,
                exitingDrones = new List<uint>(),
                energy = DroneEnergy
            };
            foreach (var bat in Batteries.Keys)
                data.batteries.Add(bat);
            foreach (var bat in FreeBatteries.Keys)
                data.freeBatteries.Add(bat);
            foreach (var bat in ChargingBatteries.Keys)
                data.chargingBatteries.Add(bat);
            foreach (var d in Drones.Keys)
                data.drones.Add(d);
            foreach (var d in FreeDrones.Keys)
                data.freeDrones.Add(d);
            foreach (var d in ExitingDrones)
                data.exitingDrones.Add(d.UID);

            return data;
        }

        public static Hub Load(SHub data, List<SDrone> sd, List<SBattery> sb) 
        {
            Hub hub = PoolController.Get(ObjectPool.Instance).Get<Hub>(null, true);
            hub.transform.position = data.position;
            hub.transform.SetParent(null);
            hub.gameObject.SetActive(true);
            hub.InPool = false;
            _Count = data.count;
            hub.UID = data.uid;
            hub.Name = "H" + hub.UID.ToString("000000");
            SimManager.AllHubs.Add(hub.UID, hub);
            hub.DroneEnergy = data.energy;
            hub.LoadAssignments(sd, sb);
            foreach (Drone d in hub.Drones.Values)
            {
                if (data.freeDrones.Contains(d.UID))
                    hub.FreeDrones.Add(d.UID, d);
                else
                    hub.FreeDrones.Remove(d.UID);
            }
            foreach (Battery b in hub.Batteries.Values)
            {
                if (data.freeBatteries.Contains(b.UID))
                    hub.FreeBatteries.Add(b.UID, b);
                else
                    hub.FreeBatteries.Remove(b.UID);

                if (data.chargingBatteries.Contains(b.UID))
                    hub.ChargingBatteries.Add(b.UID, b);
                else
                    hub.ChargingBatteries.Remove(b.UID);
            }

            foreach (var id in data.exitingDrones)
            {
                hub.ExitingDrones.Enqueue((Drone)SimManager.AllDrones[id]);
            }
            hub.StartCoroutine(hub.DeployDrone());

            return hub;
        }

        private bool LoadBattery(SBattery data)
        {
            if (data.hub == UID)
            {
                var bat = new Battery(data);
                Batteries.Add(bat.UID, bat);
                return true;
            }
            return false;
        }

        private bool LoadDrone(SDrone data)
        {
            if (data.hub == UID)
            {
                Drone drone = Drone.Load(data);
                Drones.Add(drone.UID, drone);
                if (!data.inHub) { drone.transform.SetParent(null); }
                return true;
            }
            return false;
        }

        private void LoadAssignments(List<SDrone> droneData, List<SBattery> batteryData)
        {
            for (int i = batteryData.Count - 1; i >= 0; i--)
            {
                if (LoadBattery(batteryData[i])) batteryData.RemoveAt(i);
            }
            for (int i = droneData.Count - 1; i >= 0; i--)
            {
                if (LoadDrone(droneData[i])) droneData.RemoveAt(i);
            }
            ExitingDrones.Clear();
        }

    };
}