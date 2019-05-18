using System.Collections;
using System.Collections.Generic;
using System;
namespace Drones.Serializable
{
    using Utils;
    [Serializable]
    public class StrippedDrone
    {
        public uint uid;
        public bool isWaiting;
        public bool inHub;
        public DroneMovement movement;
        public FlightStatus status;
        public float targetAltitude;
        public float maxSpeed;
        public List<SVector3> waypointsQueue;
        public SVector3 position;
        public SVector3 previousWaypoint;
        public SVector3 waypoint;
        public SVector3 hubPosition;
        public float charge;
        public uint job;
        public uint hub;
        public uint battery;

    }


}
