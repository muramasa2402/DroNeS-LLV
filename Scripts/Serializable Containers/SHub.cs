using System;
using System.Collections.Generic;

namespace Drones.Serializable
{
    [Serializable]
    public class SHub
    {
        public uint count;
        public List<uint> batteries;
        public List<uint> freeBatteries;
        public List<uint> chargingBatteries;
        public List<uint> drones;
        public List<uint> freeDrones;
        public SVector3 position;
        public List<uint> exitingDrones;
        public uint uid;
        public float energy;

    }
}
