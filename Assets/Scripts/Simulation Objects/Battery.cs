using System;
using System.Collections;
using UnityEngine;

namespace Drones
{
    using Drones.Data;
    using Drones.Managers;
    using Drones.Serializable;
    using Drones.Utils;

    [Serializable]
    public class Battery
    {
        private readonly static WaitForSeconds _Wait = new WaitForSeconds(1 / 30f);
        public static bool IsInfinite;


        public Battery(Drone drone, Hub hub)
        {
            _Data = new BatteryData();
            AssignDrone(drone);
            AssignHub(hub);
        }

        public Battery(SBattery data)
        {
            _Data = new BatteryData(data);
        }

        #region Properties
        public string Name => "B" + UID.ToString("000000");

        public BatteryStatus Status => _Data.status;

        public float Charge => _Data.charge / _Data.capacity;

        public float Capacity => _Data.capacity / BatteryData.designCapacity;
        #endregion

        public uint UID => _Data.UID;
        private readonly BatteryData _Data;
        public Hub GetHub() => (Hub)SimManager.AllHubs[_Data.hub];
        public Drone GetDrone() => (Drone)SimManager.AllDrones[_Data.drone];
        public void AssignHub(Hub hub) => _Data.hub = hub.UID;
        public void AssignDrone(Drone drone) => _Data.drone = drone.UID;
        public void SetStatus(BatteryStatus status) => _Data.status = status;
        public void Destroy() => GetHub()?.DestroyBattery(this);

        public void DischargeBattery(float dE)
        {
            if (!IsInfinite)
            {
                // TODO Non constant volage
                var dQ = dE / _Data.dischargeVoltage;
                _Data.charge -= dQ;
                if (_Data.charge > 0.1f) 
                { 
                    _Data.totalDischarge += dQ; 
                }
                else
                {
                    _Data.status = BatteryStatus.Dead;
                }
            }
        }

        public IEnumerator ChargeBattery()
        {
            var time = TimeKeeper.Chronos.Get();
            float dt;
            float dQ;
            yield return _Wait;
            if (IsInfinite) yield break;
            while (true)
            {
                dt = time.Timer(); 
                time.Now(); 
                // TODO Non constant charge rate
                dQ = _Data.chargeRate * dt;
                if (_Data.charge < _Data.capacity) { _Data.totalCharge += dQ; }

                if (Mathf.Abs(BatteryData.chargeTarget * _Data.capacity - _Data.charge) < Constants.EPSILON)
                {
                    GetHub().StopCharging(this);
                }

                _Data.charge += dQ;
                _Data.charge = Mathf.Clamp(_Data.charge, 0, _Data.capacity);

                if ((int)(_Data.totalDischarge / _Data.capacity) > _Data.cycles && (int)(_Data.totalCharge / _Data.capacity) > _Data.cycles)
                {
                    _Data.cycles++;
                    SetCap();
                }
                yield return _Wait;
            }
        }

        private void SetCap()
        {
            float x = _Data.cycles / BatteryData.designCycles;

            _Data.capacity = (-0.7199f * Mathf.Pow(x, 3) + 0.7894f * Mathf.Pow(x, 2) - 0.3007f * x + 1) * BatteryData.designCapacity;
        }

        public SBattery Serialize() => new SBattery(_Data);
    }

}