using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones
{

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
        #endregion

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
                        {DroneMovement.Hover, -60f},
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
        private Hub _AssignedHub;
        #endregion

        #region Properties
        public string Name { get; }

        public BatteryStatus Status { get; set; } = BatteryStatus.Idle;

        public float CumulativeDischarge { get; private set; }

        public float CumulativeCharge { get; private set; }

        public float AbsoluteCharge { get; private set; }

        public float AbsoluteCapacity { get; private set; }

        public float Charge => AbsoluteCharge / AbsoluteCapacity;

        public float Capacity => AbsoluteCapacity / DesignCapacity;

        public int Cycles { get; private set; } = 0;

        public bool IsOperating { get; set; } = true;
        #endregion

        #region IDronesObject
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

        public Drone AssignedDrone { get; set; }
        #endregion

        public IEnumerator Operate()
        {
            TimeKeeper.Chronos prev = TimeKeeper.Chronos.Get();

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

                dt = prev.Timer(); // time in s
                prev.Now(); // get current time

                switch (Status)
                {
                    case BatteryStatus.Discharge:
                        dQ = DischargeRate[AssignedDrone.Movement] * dt;
                        if (AbsoluteCharge > 0) { CumulativeDischarge += -dQ; }
                        break;
                    case BatteryStatus.Idle:
                        dQ = DischargeRate[DroneMovement.Idle] * dt;
                        if (AbsoluteCharge > 0) { CumulativeDischarge += -dQ; }
                        break;
                    default:
                        dQ = ChargeRate * dt;
                        if (AbsoluteCharge < AbsoluteCapacity) { CumulativeCharge += dQ; }
                        if (Mathf.Abs(ChargeTarget * AbsoluteCapacity - AbsoluteCharge) < Constants.EPSILON)
                        {
                            AssignedHub.StopCharging(this);
                        }
                        break;
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

            yield break;
        }

        private void SetCap()
        {
            float x = Cycles / DesignCycles;

            AbsoluteCapacity = (-0.7199f * Mathf.Pow(x, 3) + 0.7894f * Mathf.Pow(x, 2) - 0.3007f * x + 1) * DesignCapacity;
        }

        /* Unused */
        public void UpdateData(LiPo lipo)
        {
            AbsoluteCharge = lipo.charge;
            AbsoluteCapacity = lipo.capacity;
            Cycles = lipo.cycles;
            CumulativeCharge = lipo.totalCharge;
            CumulativeDischarge = lipo.totalDischarge;
            Status = lipo.status;
        }

    }

    public struct LiPo 
    {
        public static int BatteryOperating;
        public const int DesignCycles = 500;
        public const float DesignCapacity = 144000; // 144000 Coulombs = 40000 mAh
        public const float ChargeRate = 0.5f * DesignCapacity;
        public const float IdleDischargeRate = -0.003f;

        public LiPo(Battery battery)
        {
            charge = battery.AbsoluteCharge;
            capacity = battery.AbsoluteCapacity;
            cycles = battery.Cycles;
            dischargeRate = Battery.DischargeRate[DroneMovement.Idle];
            totalCharge = battery.CumulativeCharge;
            totalDischarge = battery.CumulativeDischarge;
            status = battery.Status;
        }

        public float charge;
        public float capacity;
        public int cycles;
        public float dischargeRate;
        public float totalDischarge;
        public float totalCharge;
        public BatteryStatus status;
    }

    public struct LiPoOperation : IJobParallelFor
    {
        public NativeArray<LiPo> AllLiPos;
        public float deltaTime;

        public void Execute(int index)
        {
            AllLiPos[index] = Operate(AllLiPos[index]);
        }

        public LiPo Operate(LiPo battery)
        {
            float dQ;
            switch (battery.status)
            {
                case BatteryStatus.Discharge:
                    dQ = battery.dischargeRate * deltaTime;
                    battery.totalDischarge += (battery.charge > 0) ? -dQ : 0;
                    break;
                case BatteryStatus.Idle:
                    dQ = LiPo.IdleDischargeRate * deltaTime;
                    battery.totalDischarge += (battery.charge > 0) ? -dQ : 0;
                    break;
                default:
                    dQ = LiPo.ChargeRate * deltaTime;
                    battery.totalCharge += (battery.charge < battery.capacity) ? dQ : 0;

                    if (Mathf.Abs(battery.capacity - battery.charge) < Constants.EPSILON)
                    {
                        battery.status = BatteryStatus.Idle;
                    }
                    break;
            }
            battery.charge += dQ;
            battery.charge = Mathf.Clamp(battery.charge, 0, battery.capacity);
            if ((int)(battery.totalCharge / battery.capacity) > battery.cycles && (int)(battery.totalDischarge / battery.capacity) > battery.cycles)
            {
                battery.cycles++;
                battery.capacity = SetCap(battery.cycles);
            }
            return battery;
        }

        private float SetCap(int cycles)
        {
            float x = cycles / LiPo.DesignCycles;

            return (-0.7199f * Mathf.Pow(x, 3) + 0.7894f * Mathf.Pow(x, 2) - 0.3007f * x + 1) * LiPo.DesignCapacity;
        }
    }

}