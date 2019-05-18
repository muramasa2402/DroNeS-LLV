using System;

namespace Drones.Serializable
{
    [Serializable]
    public class SBattery
    {
        public uint count;
        public uint uid;
        public float charge;
        public float capacity;
        public int cycles;
        public uint drone;
        public uint hub;
        public float dischargeVolt;
    }

}

