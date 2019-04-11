using System.Collections.Generic;
using Mapbox.Utils;
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
        #region IDataSource
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
        readonly string[] infoOutput = new string[6];
        readonly string[] listOutput = new string[4];

        public string[] GetData(WindowType windowType)
        {
            if (windowType == WindowType.Hub)
            {

                infoOutput[0] = Name;
                infoOutput[1] = StaticFunc.CoordString(Location);
                infoOutput[2] = DroneCount.ToString();
                infoOutput[3] = BatteryCount.ToString();
                infoOutput[4] = PowerUse.ToString("0.00");
                infoOutput[5] = EnergyUse.ToString("0.00");
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
        public string Name { get; set; }

        public Job AssignedJob { get; set; }

        public Hub AssignedHub { get; set; }

        public Drone AssignedDrone { get; } = null;
        #endregion

        #region Fields
        public SecureSet<ISingleDataSourceReceiver> _Connections;

        private SecureSet<IDataSource> _Drones;

        private SecureSet<Drone> _StoredDrones;

        private SecureSet<Battery> _ChargingBatteries;

        private SecureSet<Battery> _Batteries;
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
                ((Drone)Drones[i]).SelfRelease();
            }
            SimManager.AllHubs.Remove(this);
            gameObject.SetActive(false);
            transform.SetParent(ObjectPool.PoolContainer);
        }

        public void OnGet(Transform parent = null)
        {
            transform.SetParent(parent);
            gameObject.SetActive(true);
        }
        #endregion

        void OnEnable()
        {
            EnergyUse = 0;
            StartCoroutine(IntegrateForEnergy());
        }

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
                }
                return _Drones;
            }
        }

        private SecureSet<Drone> StoredDrones
        {
            get
            {
                if (_StoredDrones == null)
                {
                    _StoredDrones = new SecureSet<Drone>()
                    {

                        MemberCondition = (obj) => { return Drones.Contains(obj); }
                    };
                }
                return _StoredDrones;
            }
        }

        public SecureSet<Battery> ChargingBatteries
        {
            get
            {
                if (_ChargingBatteries == null)
                {
                    _ChargingBatteries = new SecureSet<Battery>
                    {
                        MemberCondition = (Battery obj) => { return Batteries.Contains(obj); }
                    };
                }
                return _ChargingBatteries;
            }
        }

        private SecureSet<Battery> Batteries
        {
            get
            {
                if (_Batteries == null)
                {
                    _Batteries = new SecureSet<Battery>();
                }
                return _Batteries;
            }
        }

        public void ReturnDrone(Drone drone)
        {
            if (drone.IsFree)
            {
                StoredDrones.Add(drone);
            }
        }

        public void StopCharge(Battery battery)
        {
            ChargingBatteries.Remove(battery);
        }

        public float PowerUse
        {
            get
            {
                return ChargingBatteryCount * Battery.ChargeRate / 1000 * AverageChargingVoltage;
            }
        }

        public float EnergyUse { get; private set; }

        public void AddDrone()
        {
            Drone drone = (Drone)ObjectPool.Get(typeof(Drone));
            Drones.Add(drone);
            StoredDrones.Add(drone);
        }

        public void RemoveDrone()
        {
            if (StoredDrones.Count > 0)
            {
                Drone drone = StoredDrones.Get();
                Drones.Remove(drone);
            }
        }

        public void AddBattery()
        {
            Batteries.Add(new Battery());
            ChargingBatteries.Add(new Battery());
        }

        public void RemoveBattery()
        {
            Batteries.Get();
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
            return GetInstanceID();
        }
    };
}