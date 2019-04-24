using System.Collections;
using System.Collections.Generic;
using System;

namespace Drones.Serializable
{
    [Serializable]
    public class SRetiredDrone
    {
        public uint uid;
        public bool isDroneCollision;
        public string hub;
        public float packageworth;
        public STime destroyed;
        public SVector2 waypoint;
        public SVector2 location;
        public List<uint> completedJobs;
        public string otherDroneName;
        public uint otherUID;
        public float charge;

    }
}
