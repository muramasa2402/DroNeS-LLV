using UnityEngine;
using System.Collections;
using System;

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
        #region IDataSource
        public bool IsDataStatic { get; } = false;

        public SecureSet<ISingleDataSourceReceiver> Connections
        {
            get
            {
                if (_Connections == null)
                {
                    _Connections = new SecureSet<ISingleDataSourceReceiver>
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
                Connections.Add(InfoWindow);
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
        private SecureSet<ISingleDataSourceReceiver> _Connections;

        private SecureSet<IDataSource> _Drones;

        private SecureSet<Drone> _FreeDrones;

        private SecureSet<Battery> _ChargingBatteries;

        private SecureSet<Battery> _Batteries;

        private SecureSet<Battery> _FreeBatteries;
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
            for (int i = 0; i < Drones.Count; i++)
            {
                SimManager.AllDestroyedDrones.Add(new DestroyedDrone((Drone)Drones[i]));
                ((Drone)Drones[i]).SelfRelease();
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
        }
        #endregion

        public Status HubStatus { get; set; } = Status.Green;

        public Vector2 Location
        {
            get
            {
                return transform.position.ToCoordinates();
            }
        }

        public int DroneCount
        {
            get
            {
                return Drones.Count;
            }
        }

        public int FreeDroneCount
        {
            get { return FreeDrones.Count; }
        }

        public int BatteryCount
        {
            get
            {
                return Batteries.Count;
            }
        }

        public int ChargingBatteryCount
        {
            get
            {
                return ChargingBatteries.Count;
            }
        }

        public SecureSet<IDataSource> Drones
        {
            get
            {
                if (_Drones == null)
                {
                    _Drones = new SecureSet<IDataSource>
                    {
                        MemberCondition = (obj) => { return obj is Drone && SimManager.AllDrones.Contains(obj); }
                    };
                    _Drones.ItemAdded += delegate (IDataSource drone)
                    {
                        Batteries.Add(((Drone)drone).AssignedBattery);
                        FreeDrones.Add((Drone)drone);
                    };
                    _Drones.ItemRemoved += delegate (IDataSource drone)
                    {
                        Drone d = (Drone)drone;
                        if (!d.InHub)
                        {
                            Batteries.Remove(d.AssignedBattery);
                        } 
                        FreeDrones.Remove(d);
                    };
                }
                return _Drones;
            }
        }

        private SecureSet<Drone> FreeDrones
        {
            get
            {
                if (_FreeDrones == null)
                {
                    _FreeDrones = new SecureSet<Drone>()
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

        private SecureSet<Battery> ChargingBatteries
        {
            get
            {
                if (_ChargingBatteries == null)
                {
                    _ChargingBatteries = new SecureSet<Battery>
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

        private SecureSet<Battery> FreeBatteries
        {
            get
            {
                if (_FreeBatteries == null)
                {
                    _FreeBatteries = new SecureSet<Battery>
                    {
                        MemberCondition = (Battery obj) => { return Batteries.Contains(obj) && obj.AssignedDrone == null; }
                    };
                }
                return _FreeBatteries;
            }
        }

        private SecureSet<Battery> Batteries
        {
            get
            {
                if (_Batteries == null)
                {
                    _Batteries = new SecureSet<Battery>();
                    _Batteries.ItemRemoved += delegate (Battery bat)
                    {
                        ChargingBatteries.Remove(bat);
                        FreeBatteries.Remove(bat);
                    };
                }
                return _Batteries;
            }
        }

        public void OnDroneReturn(Drone drone)
        {
            FreeDrones.Add(drone);
            ChargingBatteries.Add(drone.AssignedBattery);
        }

        // Called after a drone's battery has been removed (set to null)
        public void OnBatteryUnassign(Battery battery)
        {
            FreeBatteries.Add(battery);
            ChargingBatteries.Add(battery);
        }

        // Called after a drone is assigned to a battery, NOT the other way around
        public void OnBatteryAssign(Battery battery)
        {
            FreeBatteries.Remove(battery);
        }

        public void OnDroneJobAssign(Drone drone)
        {
            if (drone.AssignedJob != null)
            {
                FreeDrones.Remove(drone);
                GetBatteryForDrone(drone);
                StopCharging(drone.AssignedBattery);
            }
        }

        public void StopCharging(Battery battery)
        {
            ChargingBatteries.Remove(battery);
        }

        public float PowerUse
        {
            get
            {
                return ChargingBatteryCount * Battery.ChargeRate * AverageChargingVoltage;
            }
        }

        public float EnergyUse { get; private set; }

        private void GetBatteryForDrone(Drone drone)
        {
            if (drone.AssignedBattery == null)
            {
                if (DroneCount >= BatteryCount)
                {
                    drone.AssignedBattery = new Battery(1.00f, drone);
                    StartCoroutine(drone.AssignedBattery.Operate());
                }
                else
                {
                    drone.AssignedBattery = FreeBatteries.Get();
                }
            }
        }

        public void BuyDrone()
        {
            Drone drone = (Drone)ObjectPool.Get(typeof(Drone));
            GetBatteryForDrone(drone);
            Drones.Add(drone);
        }

        public void SellDrone()
        {
            if (FreeDrones.Count > 0)
            {
                Drone drone = FreeDrones.Get();
                Drones.Remove(drone);
            }
        }

        public void BuyBattery()
        {
            var bat = new Battery(1.00f, null);
            StartCoroutine(bat.Operate());
            Batteries.Add(bat);
        }

        public void SellBattery()
        {
            if (FreeBatteries.Count > 0)
            {
                var bat = FreeBatteries.Get();
                Batteries.Remove(bat);
            }
        }

        public void ChargeBattery(Battery battery)
        {
            if (battery.AssignedDrone.InHub)
            {
                ChargingBatteries.Add(battery);
            }
        }

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