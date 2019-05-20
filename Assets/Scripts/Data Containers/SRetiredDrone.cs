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
        public uint assignedJob;
        public STime destroyed;
        public SVector3 waypoint;
        public SVector3 location;
        public List<uint> completedJobs;
        public string otherDroneName;
        public uint otherUID;
        public float charge;

    }
}
