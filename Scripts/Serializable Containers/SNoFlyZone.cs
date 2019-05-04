using System.Collections;
using System.Collections.Generic;
using System;

namespace Drones.Serializable
{
    [Serializable]
    public class SNoFlyZone
    {
        public uint uid;
        public uint count;
        public uint droneEntry;
        public uint hubEntry;
        public SVector2 cPos;
        public SVector3 position;
        public SVector3 orientation;
        public SVector3 size;

    }
}
