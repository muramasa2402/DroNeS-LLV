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
        private static float _ChargeTarget = 1;
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
        private float _Charge = 0.5f * DesignHealth;
        private float _Health = DesignHealth;
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

        public float Charge 
        { 
            get
            {
                return _Charge / _Health;
            }
        } 

        public float Health 
        { 
            get
            {
                return _Health / DesignHealth;
            }
        } 

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

        public Battery SetCharge(float f) { _Charge = f; return this; }

        public Battery SetHealth(float f) { _Health = f; return this; }

        public IEnumerator Operate()
        {
            TimeKeeper.Chronos prev = TimeKeeper.Chronos.Get();
            var wait = new WaitForSeconds(1 / 30f);

            float dt;
            float dc;
            yield return wait;

            while (true)
            {
                dt = prev.Timer(); // time in s
                prev.Now(); // get current time

                switch (Status)
                {
                    case BatteryStatus.Discharge:
                        dc = DischargeRate[AssignedDrone.Movement] * dt;
                        if (_Charge > 0) { _CumulativeDischarge += -dc; }
                        break;
                    case BatteryStatus.Idle:
                        dc = DischargeRate[DroneMovement.Idle] * dt;
                        if (_Charge > 0) { _CumulativeDischarge += -dc; }
                        break;
                    default:
                        dc = ChargeRate * dt;
                        if (_Charge < _Health) { _CumulativeCharge += dc; }
                        if (Mathf.Abs(ChargeTarget * _Health - _Charge) < Constants.EPSILON)
                        {
                            Status = BatteryStatus.Idle;
                            AssignedHub.StopCharge(this);
                        }
                        break;
                }
                _Charge += dc;
                _Charge = Mathf.Clamp(_Charge, 0, _Health);
                if ((int)(_CumulativeDischarge / _Health) > Cycles && (int)(_CumulativeCharge / _Health) > Cycles)
                {
                    Cycles++;
                    SetHealth();
                }

                yield return wait;
            }
        }

        private void SetHealth()
        {
            float x = Cycles / DesignCycles;

            _Health = (-0.7199f * Mathf.Pow(x, 3) + 0.7894f * Mathf.Pow(x, 2) - 0.3007f * x + 1) * DesignHealth;
        }

    }


}