using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Drones
{
    using Utils;
    using Utils.Extensions;
    using DataStreamer;
    using Drones.UI;
    using Drones.Interface;

    public class Hub : MonoBehaviour, IDronesObject, IDataSource, IPoolable
    {
        public static float AverageChargingVoltage { get; } = 4;
        public static uint _Count;
        private static readonly float _DeploymentPeriod = 0.5f;

        #region IDataSource
        public bool IsDataStatic { get; } = false;

        public SecureSortedSet<int, ISingleDataSourceReceiver> Connections
        {
            get
            {
                if (_Connections == null)
                {
                    _Connections = new SecureSortedSet<int, ISingleDataSourceReceiver>((x, y) => (x.OpenTime <= y.OpenTime) ? -1 : 1)
                    {
                        MemberCondition = (ISingleDataSourceReceiver obj) => obj is ListTuple || obj is HubWindow
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
        private readonly string[] infoOutput = new string[6];
        private readonly string[] listOutput = new string[4];

        public string[] GetData(WindowType windowType)
        {
            if (windowType == WindowType.Hub)
            {
                infoOutput[0] = Name;
                infoOutput[1] = StaticFunc.CoordString(Location);
                infoOutput[2] = DroneCount.ToString();
                infoOutput[3] = BatteryCount.ToString();
                infoOutput[4] = UnitConverter.Convert(Power.kW, PowerUse);
                infoOutput[5] = UnitConverter.Convert(Energy.kWh, EnergyUse);
                return infoOutput;
            } 
            if (windowType == WindowType.HubList)
            {
                listOutput[0] = Name;
                listOutput[1] = DroneCount.ToString();
                listOutput[2] = BatteryCount.ToString();
                listOutput[3] = StaticFunc.CoordString(Location);
                return listOutput;
            }
            throw new ArgumentException("Wrong Window Type Supplied");
        }

        public AbstractInfoWindow InfoWindow { get; set; }

        public void OpenInfoWindow()
        {
            if (InfoWindow == null)
            {
                InfoWindow = (HubWindow)UIObjectPool.Get(WindowType.Hub, Singletons.UICanvas);
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
        public uint UID { get; private set; }

        public string Name { get; set; }

        public Job AssignedJob { get; set; }

        public Hub AssignedHub { get; set; }

        public Drone AssignedDrone { get; } = null;
        #endregion

        #region Fields
        private SecureSortedSet<int, ISingleDataSourceReceiver> _Connections;

        private SecureSortedSet<uint, IDataSource> _Drones;

        private SecureSortedSet<uint, Drone> _FreeDrones;

        private SecureSortedSet<uint, Battery> _ChargingBatteries;

        private SecureSortedSet<uint, Battery> _Batteries;

        private SecureSortedSet<uint, Battery> _FreeBatteries;

        private Queue<Drone> _ExitingDrones;
        #endregion

        #region IPoolable
        public void SelfRelease()
        {
            ObjectPool.Release(this);
        }

        public void OnRelease()
        {
            InfoWindow?.Close.onClick.Invoke();
            StopAllCoroutines();
            Connections.Clear();
            foreach (Drone drone in Drones.Values)
            {
                DestroyDrone(drone);
            }
            Drones.Clear();
            SimManager.AllHubs.Remove(this);
            gameObject.SetActive(false);
            transform.SetParent(ObjectPool.PoolContainer);
        }

        public void OnGet(Transform parent = null)
        {
            UID = _Count++;
            Name = "H" + UID.ToString("000000");
            transform.SetParent(parent);
            gameObject.SetActive(true);
            EnergyUse = 0;
            StartCoroutine(IntegrateForEnergy());
            StartCoroutine(DeployDrone());
        }
        #endregion

        public Status HubStatus { get; set; } = Status.Green;

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

        public Vector2 Location => transform.position.ToCoordinates();

        public int DroneCount => Drones.Count;

        public int FreeDroneCount => FreeDrones.Count;

        public int BatteryCount => Batteries.Count;

        public int ChargingBatteryCount => ChargingBatteries.Count;

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
                    _FreeDrones = new SecureSortedSet<uint, Drone>()
                    {
                        MemberCondition = (drone) => { return Drones.Contains(drone) && drone.AssignedJob == null; }
                    };
                    _FreeDrones.ItemRemoved += (drone) =>
                    {
                        StopCharging(drone.AssignedBattery);
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
                    };
                    _ChargingBatteries.ItemRemoved += delegate (Battery bat)
                    {
                        bat.Status = BatteryStatus.Idle;
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
                        bat.AssignedHub = this;
                    };
                    _Batteries.ItemRemoved += delegate (Battery bat)
                    {
                        ChargingBatteries.Remove(bat);
                        FreeBatteries.Remove(bat);
                    };
                }
                return _Batteries;
            }
        }

        public float PowerUse => ChargingBatteryCount * Battery.ChargeRate * AverageChargingVoltage;

        public float EnergyUse { get; private set; }

        IEnumerator IntegrateForEnergy()
        {
            var wait = new WaitForSeconds(1 / 30f);
            TimeKeeper.Chronos prev = TimeKeeper.Chronos.Get();
            float dt;
            while (true)
            {
                dt = prev.Timer();

                EnergyUse += PowerUse * dt;
                prev.Now();

                yield return wait;
            }
        }

        IEnumerator DeployDrone()
        {
            var time = TimeKeeper.Chronos.Get();
            WaitUntil _DroneReady = new WaitUntil(() => time.Timer() > _DeploymentPeriod);
            while (true)
            {
                if (ExitingDrones.Count > 0)
                {
                    ExitingDrones.Dequeue().IsWaiting = false;
                }
                yield return _DroneReady;
                time.Now();
            }
        }

        #region Drone/Battery Interface
        public void OnDroneReturn(Drone drone)
        {
            if (drone != null && FreeDrones.Add(drone.UID, drone))
            {
                ChargingBatteries.Add(drone.AssignedBattery.UID, drone.AssignedBattery);
            }
        }

        public void RemoveBatteryFromDrone(Drone drone)
        {
            if (drone.AssignedHub == this && FreeBatteries.Add(drone.AssignedBattery.UID, drone.AssignedBattery))
            {
                ChargingBatteries.Add(drone.AssignedBattery.UID, drone.AssignedBattery);
            }
        }

        public void ReassignDrone(Drone drone, Hub hub)
        {
            drone.AssignedHub = hub;
            Drones.Remove(drone);
            Batteries.Remove(drone.AssignedBattery);
            hub.Drones.Add(drone.UID, drone);
            hub.Batteries.Add(drone.AssignedBattery.UID, drone.AssignedBattery);
        }

        public void OnDroneJobAssign(Drone drone)
        {
            if (drone.AssignedJob != null)
            {
                FreeDrones.Remove(drone);
                GetBatteryForDrone(drone);
                StopCharging(drone.AssignedBattery);

            } 
            else
            {
                FreeDrones.Add(drone.UID, drone);
            }
        }

        public void ChargeBattery(Battery battery)
        {
            if (battery.AssignedDrone.InHub)
            {
                ChargingBatteries.Add(battery.UID, battery);
            }
        }

        public void StopCharging(Battery battery)
        {
            ChargingBatteries.Remove(battery);
        }

        private void GetBatteryForDrone(Drone drone)
        {
            if (drone.AssignedBattery == null)
            {
                if (DroneCount >= BatteryCount)
                {
                    drone.AssignedBattery = BuyBattery();
                }
                else
                {
                    drone.AssignedBattery = FreeBatteries.GetMax(true);
                }
            }
        }

        public void DestroyDrone(Drone drone, Collider other)
        {
            var dd = new DestroyedDrone(drone, other);
            SimManager.AllDestroyedDrones.Add(dd.UID, dd);
            Drones.Remove(drone);
            DestroyBattery(drone.AssignedBattery);
            drone.SelfRelease();
        }

        public void DestroyDrone(Drone drone)
        {
            var dd = new DestroyedDrone(drone);
            SimManager.AllDestroyedDrones.Add(dd.UID, dd);
            Drones.Remove(drone);
            DestroyBattery(drone.AssignedBattery);
            drone.SelfRelease();
        }

        public Drone BuyDrone()
        {
            Drone drone = (Drone)ObjectPool.Get(typeof(Drone));
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
                var dd = new DestroyedDrone(drone);
                SimManager.AllDestroyedDrones.Add(dd.UID, dd);
                Drones.Remove(drone);
                drone.SelfRelease();
            }
        }

        public void DestroyBattery(Battery battery) => Batteries.Remove(battery);

        public Battery BuyBattery(Drone drone = null)
        {
            var bat = new Battery(1.00f, drone, this);
            StartCoroutine(bat.Operate());
            Batteries.Add(bat.UID, bat);
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

        public override bool Equals(object other)
        {
            return other is Hub && GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    };
}