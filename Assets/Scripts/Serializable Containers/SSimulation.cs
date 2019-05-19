using System;
using System.Collections.Generic;

namespace Drones.Serializable
{
    [Serializable]
    public class SSimulation
    {
        public long timestamp = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        public float revenue;
        public float delay;
        public float audible;
        public float energy;
        public List<SDrone> drones;
        public List<SRetiredDrone> retiredDrones;
        public List<SBattery> batteries;
        public List<SHub> hubs;
        public List<SJob> completedJobs;
        public List<SJob> incompleteJobs;
        public List<SNoFlyZone> noFlyZones;
        public STime currentTime;
        public uint requester; //TODO remove
    }

}