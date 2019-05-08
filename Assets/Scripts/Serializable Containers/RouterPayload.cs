using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Serializable
{
    [Serializable]
    public class RouterPayload
    {
        public List<StaticObstacle> noFlyZones;
        public List<SVector3> dronePositions;
        public List<SVector3> droneDirections;
    }

}
