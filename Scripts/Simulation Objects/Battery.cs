using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones
{
    using Drones.Serializable;
    using Drones.Utils;
    using Unity.Collections;
    using Unity.Jobs;

    [Serializable]
    public class Battery : IDronesObject
    {
        private static uint _Count;

        #region Constructors
        public Battery(Drone drone, Hub hub)
        {
            UID = _Count++;
            Name = "B" + UID.ToString("000000");
            AbsoluteCapacity = DesignCapacity;
            AssignedDrone = drone;
            AssignedHub = hub;
            AbsoluteCharge = 0.5f * AbsoluteCapacity;

        }

        public Battery(float charge, Drone drone, Hub hub)
        {
            UID = _Count++;
            Name = "B" + UID.ToString("000000");
            AbsoluteCapacity = DesignCapacity;
            AssignedDrone = drone;
            AssignedHub = hub;
            AbsoluteCharge = Mathf.Clamp(charge, 0, 1) * AbsoluteCapacity;
        }

        public Battery(SBattery data)
        {
            _Count = data.count;
            UID = data.uid;
            Name = "B" + UID.ToString("000000");
            AbsoluteCapacity = data.capacity;
            AbsoluteCharge = data.charge;
            Cycles = data.cycles;
        }
        #endregion

        #region Statics
        private readonly static WaitForSeconds _Wait = new WaitForSeconds(1 / 30f);
        public static int DesignCycles { get; } = 500;
        public static float DesignCapacity { get; } = 576000f; // 576,000 Coulombs = 160,000 mAh
        public static float ChargeRate { get; } = 0.5f * DesignCapacity;
        private static float _ChargeTarget = 1;
        public static bool IsInfinite { get; set; } = false;
        public static float DischargeVoltage { get; } = 23f;
        public static float ChargeVoltage { get; } = 4f;
        public static float ChargeTarget
        {
            get => _ChargeTarget;
            set
            {
                _ChargeTarget = Mathf.Clamp(value, 0, 1);
            }
        }
        #endregion

        #region Fields
        private Hub _AssignedHub;
        private Drone _AssignedDrone;
        #endregion

        #region Properties
        public string Name { get; private set; }

        public BatteryStatus Status { get; set; } = BatteryStatus.Idle;

        public float CumulativeDischarge { get; private set; }

        public float CumulativeCharge { get; private set; }

        public float AbsoluteCharge { get; private set; }

        public float AbsoluteCapacity { get; private set; }

        public float Charge => AbsoluteCharge / AbsoluteCapacity;

        public float Capacity => AbsoluteCapacity / DesignCapacity;

        public int Cycles { get; private set; } = 0;
        #endregion

        #region IDronesObject
        public uint UID { get; private set; }

        public Job AssignedJob
        {
            get
            {
                if (AssignedDrone == null)
                {
                    return null;
                }
                return AssignedDrone.AssignedJob;
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
                }
            }
        }

        public Drone AssignedDrone
        {
            get => _AssignedDrone;

            set
            {
                _AssignedDrone = value;
                Status = (_AssignedDrone == null) ? BatteryStatus.Idle : BatteryStatus.Discharge;
            }
        }
        #endregion

        public void DischargeBattery(float dE)
        {
            if (!IsInfinite)
            {
                var dQ = dE / DischargeVoltage;
                AbsoluteCharge -= dQ;
                if (AbsoluteCharge > 0.1f) 
                { 
                    CumulativeDischarge += dQ; 
                }
                else
                {
                    Status = BatteryStatus.Dead;
                }
            }
        }

        public IEnumerator ChargeBattery()
        {
            var time = TimeKeeper.Chronos.Get();
            float dt;
            float dQ;
            yield return _Wait;

            while (true)
            {
                if (IsInfinite)
                {
                    AbsoluteCapacity = DesignCapacity;
                    AbsoluteCharge = DesignCapacity;
                    break;
                }

                dt = time.Timer(); // time in s
                time.Now(); // get current time

                dQ = ChargeRate * dt;
                if (AbsoluteCharge < AbsoluteCapacity) { CumulativeCharge += dQ; }

                if (Mathf.Abs(ChargeTarget * AbsoluteCapacity - AbsoluteCharge) < Constants.EPSILON)
                {
                    AssignedHub.StopCharging(this);
                }
                AbsoluteCharge += dQ;
                AbsoluteCharge = Mathf.Clamp(AbsoluteCharge, 0, AbsoluteCapacity);

                if ((int)(CumulativeDischarge / AbsoluteCapacity) > Cycles && (int)(CumulativeCharge / AbsoluteCapacity) > Cycles)
                {
                    Cycles++;
                    SetCap();
                }
                yield return _Wait;
            }
        }

        private void SetCap()
        {
            float x = Cycles / DesignCycles;

            AbsoluteCapacity = (-0.7199f * Mathf.Pow(x, 3) + 0.7894f * Mathf.Pow(x, 2) - 0.3007f * x + 1) * DesignCapacity;
        }

        public SBattery Serialize()
        {
            return new SBattery
            {
                count = _Count,
                uid = UID,
                capacity = AbsoluteCapacity,
                charge = AbsoluteCharge,
                cycles = Cycles,
                drone = (AssignedDrone == null) ? 0 : AssignedDrone.UID,
                hub = (AssignedHub == null) ? 0 : AssignedHub.UID
            };
        }
    }

}