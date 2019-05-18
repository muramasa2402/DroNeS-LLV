using System;
using System.Collections.Generic;
using Drones.Utils;
using Newtonsoft.Json;

namespace Drones.Serializable
{
    [Serializable]
    public class RouterPayload
    {
        public uint requester;
        public SVector3 origin;
        public SVector3 destination;
        public bool onJob;
        public JobStatus status;
        public List<StaticObstacle> noFlyZones;
        public Dictionary<uint, int> drone;
        public List<SVector3> dronePositions;
        public List<SVector3> droneDirections;

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }

    }

}
