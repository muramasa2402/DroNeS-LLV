using System;
using System.Collections.Generic;
using UnityEngine;

namespace Drones
{
    using EventSystem;
    using UI;
    using Utils;
    using DataStreamer;
    using Utils.Extensions;
    using Serializable;
    using Managers;

    public class RetiredDrone : IDataSource
    {
        public RetiredDrone(Drone drone, Collider other)
        {
            UID = drone.UID;
            Name = drone.Name;
            AssignedJob = drone.AssignedJob;
            HubName = (drone.AssignedHub == null) ? "" : drone.AssignedHub.Name;
            CompletedJobs = drone.CompletedJobs;
            drone.StopCoroutine(drone.AssignedBattery.ChargeBattery());
            BatteryCharge = drone.AssignedBattery.Charge;
            if (other.CompareTag("Drone"))
            {
                var collidee = other.GetComponent<Drone>();
                IsDroneCollision = true;
                OtherDroneName = collidee.Name;
                _OtherUID = collidee.UID;
            }
            Waypoint = drone.Waypoint;
            DestroyedTime = TimeKeeper.Chronos.Get();
            CollisionLocation = drone.Position;
            PackageWorth = (AssignedJob == null) ? 0 : AssignedJob.Loss;
            ConsoleLog.WriteToConsole(new DroneCollision(this));
        }

        public RetiredDrone(Drone drone, bool sold = false)
        {
            IsDroneCollision = false;
            UID = drone.UID;
            Name = drone.Name;
            AssignedJob = drone.AssignedJob;
            HubName = (drone.AssignedHub == null) ? "" : drone.AssignedHub.Name;
            CompletedJobs = drone.CompletedJobs;
            drone.StopCoroutine(drone.AssignedBattery.ChargeBattery());
            BatteryCharge = drone.AssignedBattery.Charge;
            Waypoint = drone.Waypoint;
            DestroyedTime = TimeKeeper.Chronos.Get();
            CollisionLocation = drone.Position;
            PackageWorth = (AssignedJob == null) ? 0 : AssignedJob.Loss;
            if (!sold)
            {
                OtherDroneName = "Environment";
                ConsoleLog.WriteToConsole(new DroneCollision(this));
            }
            else
            {
                OtherDroneName = "Retired";
                ConsoleLog.WriteToConsole(new DroneRetired(this));
            }

        }

        public RetiredDrone(SRetiredDrone data)
        {
            UID = data.uid;
            IsDroneCollision = data.isDroneCollision;
            HubName = data.hub;
            PackageWorth = data.packageworth;
            DestroyedTime = new TimeKeeper.Chronos(data.destroyed);
            CollisionLocation = data.location;
            Waypoint = data.waypoint;
            _OtherUID = data.otherUID;
            OtherDroneName = data.otherDroneName;
            BatteryCharge = data.charge;
            AssignedJob = (Job)SimManager.AllIncompleteJobs[data.assignedJob];
            SimManager.AllRetiredDrones.Add(UID, this);
            CompletedJobs = new SecureSortedSet<uint, IDataSource>((x, y) => (((Job)x).CompletedOn >= ((Job)y).CompletedOn) ? -1 : 1)
            {
                MemberCondition = (IDataSource obj) => { return obj is Job; }
            };
            foreach (uint job in data.completedJobs)
            {
                CompletedJobs.Add(job, SimManager.AllCompleteJobs[job]);
            }
        }

        #region Fields
        private SecureSortedSet<int, ISingleDataSourceReceiver> _Connections;
        #endregion

        public string Name { get; }

        public Job AssignedJob { get; }

        #region IDataSource
        public uint UID { get; }

        public bool IsDataStatic { get; } = true;

        public AbstractInfoWindow InfoWindow { get; set; }

        private readonly string[] infoOutput = new string[12];
        private readonly string[] listOutput = new string[4];

        public string[] GetData(Type windowType)
        {
            if (windowType == typeof(RetiredDroneWindow))
            {
                infoOutput[0] = Name;
                infoOutput[1] = HubName;
                infoOutput[2] = Waypoint.ToStringXZ();
                infoOutput[3] = DestroyedTime.ToString();
                infoOutput[4] = CollisionLocation.ToStringXYZ();
                infoOutput[5] = "$" + PackageWorth.ToString("0.00");
                infoOutput[6] = OtherDroneName;
                infoOutput[7] = BatteryCharge.ToString("0.000");
                infoOutput[8] = (AssignedJob == null) ? "" : AssignedJob.Name;
                infoOutput[9] = (AssignedJob == null) ? "" : AssignedJob.Pickup.ToStringXZ();
                infoOutput[10] = (AssignedJob == null) ? "" : AssignedJob.Dest.ToStringXZ();
                infoOutput[11] = (AssignedJob == null) ? "" : AssignedJob.Deadline.ToString();
                return infoOutput;
            }
            if (windowType == typeof(RetiredDroneListWindow))
            {
                listOutput[0] = Name;
                listOutput[1] = DestroyedTime.ToString();
                listOutput[2] = CollisionLocation.ToStringXYZ();
                listOutput[3] = "$" + PackageWorth.ToString("0.00");
                return listOutput;
            }
            throw new ArgumentException("Wrong Window Type Supplied");
        }

        public void OpenInfoWindow()
        {
            if (InfoWindow == null)
            {
                InfoWindow = RetiredDroneWindow.New();
                InfoWindow.Source = this;
            }
            else
            {
                InfoWindow.transform.SetAsLastSibling();
            }
        }
        #endregion

        public SecureSortedSet<uint, IDataSource> CompletedJobs { get; }

        private readonly bool IsDroneCollision;

        public string HubName { get; }

        public float PackageWorth { get; }

        public TimeKeeper.Chronos DestroyedTime { get; }

        public Vector3 CollisionLocation { get; }

        public Vector3 Waypoint { get; }

        private readonly uint _OtherUID;

        public string OtherDroneName { get; }

        public float BatteryCharge { get; }

        public RetiredDrone OtherDrone
        {
            get
            {
                if (IsDroneCollision)
                {
                    return (RetiredDrone)SimManager.AllRetiredDrones[_OtherUID];
                }
                return null;
            }
        }

        public SRetiredDrone Serialize()
        {
            var data = new SRetiredDrone
            {
                uid = UID,
                isDroneCollision = IsDroneCollision,
                hub = HubName,
                assignedJob = (AssignedJob == null) ? 0 : AssignedJob.UID,
                packageworth = PackageWorth,
                destroyed = DestroyedTime.Serialize(),
                waypoint = Waypoint,
                location = CollisionLocation,
                completedJobs = new List<uint>(),
                otherDroneName = OtherDroneName,
                otherUID = _OtherUID,
                charge = BatteryCharge
            };

            foreach (var job in CompletedJobs.Values)
                data.completedJobs.Add(job.UID);

            return data;
        }
    }
}
