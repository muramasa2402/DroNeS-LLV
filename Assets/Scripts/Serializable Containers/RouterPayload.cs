using System;
using System.Collections.Generic;
using Drones.Utils;

namespace Drones.Serializable
{
    [Serializable]
    public class RouterPayload
    {
        public SVector3 origin;
        public SVector3 destination;
        public bool onJob;
        public JobStatus status;
        public List<StaticObstacle> noFlyZones;
        public List<uint> drone;
        public List<SVector3> dronePositions;
        public List<SVector3> droneDirections;

    }

}
