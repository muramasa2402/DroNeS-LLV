using Drones.Objects;
using Drones.Utils.Interfaces;
using UnityEngine;
using Utils;

namespace Drones.Data
{
    using Utils;
    using static Managers.SimManager;
    public class RetiredDroneData : IData
    {
        public bool IsDataStatic { get; set; } = true;

        public uint UID { get; }
        public uint otherUID;
        public uint job;
        public SecureSortedSet<uint, IDataSource> completedJobs;
        public bool isDroneCollision;
        public string hub;
        public string otherDrone;
        public float packageWorth;
        public TimeKeeper.Chronos destroyedTime;
        public Vector3 collisionLocation;
        public Vector3 waypoint;
        public float batteryCharge;

        public RetiredDroneData(Drone drone, Collider other)
        {
            UID = drone.UID;
            var j = drone.GetJob();
            job = j?.UID ?? 0;
            hub = drone.GetHub()?.Name;
            completedJobs = drone.JobHistory;
            batteryCharge = drone.GetBattery().Charge;
            if (other.CompareTag("Drone"))
            {
                var collidee = other.GetComponent<Drone>();
                isDroneCollision = true;
                otherDrone = collidee.Name;
                otherUID = collidee.UID;
            }
            waypoint = drone.Waypoint;
            destroyedTime = TimeKeeper.Chronos.Get();
            collisionLocation = drone.transform.position;
            packageWorth = j?.Loss ?? 0;
        }

        public RetiredDroneData(Drone drone)
        {
            isDroneCollision = false;
            UID = drone.UID;
            var j = drone.GetJob();
            job = j?.UID ?? 0;
            hub = drone.GetHub()?.Name;
            completedJobs = drone.JobHistory;
            batteryCharge = drone.GetBattery()?.Charge ?? 0;
            waypoint = drone.Waypoint;
            destroyedTime = TimeKeeper.Chronos.Get();
            collisionLocation = drone.transform.position;
            packageWorth = j?.Loss ?? 0;
            otherDrone = "Retired";
        }

    }
}
