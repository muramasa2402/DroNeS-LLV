using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Serializable
{
    [Serializable]
    public class RouterPayload
    {
        public List<SNoFlyZone> noFlyZones;
        public List<SVector3> dronePositions;
        public List<SVector3> droneDirections;

        public Vector3 origin { get; internal set; }
        public Vector3 destination { get; internal set; }
    }
}
