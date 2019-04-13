using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones
{

    using Drones.Utils;

    [Serializable]
    public class Battery : IDronesObject
    {
        private static uint _Count;
        public Battery()
        {
            UID = _Count++;
            Name = "B" + UID.ToString("000000");
            _Capacity = DesignCapacity;
            SetCharge(0.5f * _Capacity);
        }

        public Battery(float charge)
        {
            if (charge > 1)
            {
                throw new ArgumentException("Charge must be between 0 and 1");
            }
            UID = _Count++;
            Name = "B" + UID.ToString("000000");
            _Capacity = DesignCapacity;
            SetCharge(charge * _Capacity);
        }

        #region Statics
        private static Dictionary<DroneMovement, float> _DischargeRate;
        private static float _ChargeTarget = 1;
        private readonly static WaitForSeconds _Wait = new WaitForSeconds(1 / 30f);
        public static Dictionary<DroneMovement, float> DischargeRate
        {
            get
            {
                if (_DischargeRate == null)
                {
                    _DischargeRate = new Dictionary<DroneMovement, float>
                    {
                        {DroneMovement.Ascend, -144f},
                        {DroneMovement.Hover, -130f},
                        {DroneMovement.Descend, -30f},
                        {DroneMovement.Horizontal, -72f},
                        {DroneMovement.Idle, -0.003f} // ~ 5% CHARGE LOSS / MONTH
                    };
                }
                return _DischargeRate;
            }
        }
        public static int DesignCycles { get; } = 500;
        public static float DesignCapacity { get; } = 144000; // 144000 Coulombs = 40000 mAh
        public static float ChargeRate { get; } = 0.5f * DesignCapacity;
        public static float ChargeTarget
        {
            get
            {
                return _ChargeTarget;
            }
            set
            {
                _ChargeTarget = Mathf.Clamp(value, 0, 1);
            }
        }
        public static bool IsInfinite { get; set; } = false;
        #endregion

        #region Fields
        private float _CumulativeDischarge;
        private float _CumulativeCharge;
        private float _Charge;
        private float _Capacity;
        #endregion

        #region Properties
        public string Name { get; }

        public BatteryStatus Status { get; set; } = BatteryStatus.Idle;

        public float Charge 
        { 
            get
            {
                return _Charge / _Capacity;
            }
        } 

        public float Capacity 
        { 
            get
            {
                return _Capacity / DesignCapacity;
            }
        } 

        public float Cycles { get; private set; } = 0;

        public bool IsOperating { get; set; } = true;
        #endregion

        public uint UID { get; }

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

        public Hub AssignedHub { get; set; }

        public Drone AssignedDrone { get; set; }

        public Battery SetCharge(float absoluteCharge) { _Charge = absoluteCharge; return this; }

        public Battery SetHealth(float absoluteHealth) { _Capacity = absoluteHealth; return this; }

        public IEnumerator Operate()
        {
            TimeKeeper.Chronos prev = TimeKeeper.Chronos.Get();

            float dt;
            float dQ;
            yield return _Wait;

            while (true)
            {
                dt = prev.Timer(); // time in s
                prev.Now(); // get current time

                switch (Status)
                {
                    case BatteryStatus.Discharge:
                        dQ = DischargeRate[AssignedDrone.Movement] * dt;
                        if (_Charge > 0) { _CumulativeDischarge += -dQ; }
                        break;
                    case BatteryStatus.Idle:
                        dQ = DischargeRate[DroneMovement.Idle] * dt;
                        if (_Charge > 0) { _CumulativeDischarge += -dQ; }
                        break;
                    default:
                        dQ = ChargeRate * dt;
                        if (_Charge < _Capacity) { _CumulativeCharge += dQ; }
                        if (Mathf.Abs(ChargeTarget * _Capacity - _Charge) < Constants.EPSILON)
                        {
                            Status = BatteryStatus.Idle;
                            AssignedHub.StopCharge(this);
                        }
                        break;
                }
                _Charge += dQ;
                _Charge = Mathf.Clamp(_Charge, 0, _Capacity);
                if ((int)(_CumulativeDischarge / _Capacity) > Cycles && (int)(_CumulativeCharge / _Capacity) > Cycles)
                {
                    Cycles++;
                    SetHealth();
                }

                yield return _Wait;
            }
        }

        private void SetHealth()
        {
            float x = Cycles / DesignCycles;

            _Capacity = (-0.7199f * Mathf.Pow(x, 3) + 0.7894f * Mathf.Pow(x, 2) - 0.3007f * x + 1) * DesignCapacity;
        }

    }


}