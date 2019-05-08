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
        public List<Vector3> dronePositions;
        public List<Vector3> droneDirections;

        public Vector3 origin;
        public Vector3 destination;
        public bool onJob;
        public JobStatus status;
    }
}
