using Drones.Utils;
using System;
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

        public Vector3 origin { get; set; }
        public Vector3 destination { get; set; }
        public JobStatus status { get; set; }
    }
}
