using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones
{
    using Drones.Utils;
    public class Battery : IDronesObject
    {
        #region Statics
        private static SortedSet<string> _UnusedNameDatabase;
        private static SortedSet<string> _UsedNameDatabase;
        private static Dictionary<DroneMovement, float> _DischargeRate;
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
        public static float DesignHealth { get; } = 144000; // Coulombs = 40000 mAh
        // Max charge rate is 1C which would charge in an hour but, charging not const
        public static float ChargeRate { get; } = 0.5f * DesignHealth; 
        // https://batteryuniversity.com/learn/article/charging_lithium_ion_batteries
        public static SortedSet<string> UnusedNameDatabase
        {
            get
            {
                if (_UnusedNameDatabase == null)
                {
                    _UnusedNameDatabase = new SortedSet<string>();
                    int max = 100001;
                    for (int i = 1; i < max; i++)
                    {
                        string name = "B";
                        var zeros = max.ToString().Length - i.ToString().Length;
                        for (int j = 0; j < zeros; j++)
                        {
                            name += "0";
                        }
                        name += i;
                        _UnusedNameDatabase.Add(name);
                    }
                }
                return _UnusedNameDatabase;
            }
        }
        public static SortedSet<string> UsedNameDatabase
        {
            get
            {
                if (_UsedNameDatabase == null)
                {
                    _UsedNameDatabase = new SortedSet<string>();
                }
                return _UsedNameDatabase;
            }
        }
        #endregion

        #region Fields
        private float _CumulativeDischarge;
        private float _CumulativeCharge;
        private string _Name;
        #endregion

        #region Properties
        public string Name
        {
            get
            {
                if (_Name == null)
                {
                    _UsedNameDatabase.Add(_UnusedNameDatabase.Min);
                    _Name = _UnusedNameDatabase.Min;
                    _UnusedNameDatabase.Remove(_Name);
                }
                return _Name;
            }
            set
            {
                if (!_UsedNameDatabase.Add(value))
                {
                    //TODO Error window "This name has already been taken"
                }
                else
                {
                    if (_Name != null)
                    {
                        _UnusedNameDatabase.Add(_Name);
                    }
                    _UnusedNameDatabase.Remove(value);
                    _Name = value;
                }

            }
        }

        public BatteryStatus Status { get; set; } = BatteryStatus.Idle;

        public float Charge { get; private set; } = 0.5f * DesignHealth;

        public float Health { get; private set; } = DesignHealth;

        public float Cycles { get; private set; } = 0;

        public bool IsOperating { get; set; } = true;
        #endregion

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

        public Battery SetCharge(float f) { Charge = f; return this; }

        public Battery SetHealth(float f) { Health = f; return this; }

        public IEnumerator Operate()
        {
            var wait = new WaitForSeconds(1 / 12f);
            TimeKeeper.Chronos prev = TimeKeeper.Chronos.Get();

            float dt;
            float dc;
            yield return wait;

            while (true)
            {
                dt = prev.Timer(); // time in s

                switch (Status)
                {
                    case BatteryStatus.Discharge:
                        dc = DischargeRate[AssignedDrone.Movement] * dt;
                        if (Charge > 0) { _CumulativeDischarge += -dc; }
                        break;
                    case BatteryStatus.Idle:
                        dc = DischargeRate[DroneMovement.Idle] * dt;
                        if (Charge > 0) { _CumulativeDischarge += -dc; }
                        break;
                    default:
                        dc = ChargeRate * dt;
                        if (Charge < Health) { _CumulativeCharge += dc; }
                        if (Health - Charge < Constants.EPSILON)
                        {
                            Status = BatteryStatus.Idle;
                            AssignedHub.ChargingBatteries.Remove(this);
                            AssignedHub.IdleBatteries.Add(this);
                        }
                        break;
                }

                Charge = Mathf.Clamp(Charge + dc, 0, Health);
                if ((int)(_CumulativeDischarge / Health) > Cycles && (int)(_CumulativeCharge / Health) > Cycles)
                {
                    Cycles++;
                    SetHealth();
                }

                prev.Now();
                yield return wait;
            }
        }

        private void SetHealth()
        {
            float x = Cycles / DesignCycles;

            Health = (-0.7199f * Mathf.Pow(x, 3) + 0.7894f * Mathf.Pow(x, 2) - 0.3007f * x + 1) * DesignHealth;
        }
    }

}