using System;
using UnityEngine;

namespace Drones
{
    using Drones.EventSystem;
    using Drones.UI;
    using Drones.Utils;
    using Drones.DataStreamer;
    using Drones.Utils.Extensions;
    using Drones.Serializable;
    using System.Collections.Generic;

    public class RetiredDrone : IDronesObject, IDataSource
    {
        public RetiredDrone(Drone drone, Collider other)
        {
            UID = drone.UID;
            Name = drone.Name;
            AssignedJob = drone.AssignedJob;
            HubName = (drone.AssignedHub == null) ? "" : drone.AssignedHub.Name;
            AssignedHub = null;
            AssignedDrone = null;
            CompletedJobs = drone.CompletedJobs;
            drone.StopCoroutine(drone.AssignedBattery.ChargeBattery());
            BatteryCharge = drone.AssignedBattery.Charge;
            var collidee = other.GetComponent<Drone>();
            if (collidee != null)
            {
                IsDroneCollision = true;
                OtherDroneName = collidee.Name;
                _OtherUID = collidee.UID;
            }
            Waypoint = drone.Waypoint.ToCoordinates();
            DestroyedTime = TimeKeeper.Chronos.Get();
            CollisionLocation = drone.Position;
            PackageWorth = (AssignedJob == null) ? 0 : AssignedJob.Earnings;
            SimManager.AllRetiredDrones.Add(UID, this);
            SimulationEvent.Invoke(EventType.Collision, new DroneCollision(this));
        }

        public RetiredDrone(Drone drone, bool sold = false)
        {
            IsDroneCollision = false;
            UID = drone.UID;
            Name = drone.Name;
            AssignedJob = drone.AssignedJob;
            HubName = (drone.AssignedHub == null) ? "" : drone.AssignedHub.Name;
            AssignedHub = null;
            AssignedDrone = null;
            CompletedJobs = drone.CompletedJobs;
            drone.StopCoroutine(drone.AssignedBattery.ChargeBattery());
            BatteryCharge = drone.AssignedBattery.Charge;
            Waypoint = drone.Waypoint.ToCoordinates();
            DestroyedTime = TimeKeeper.Chronos.Get();
            CollisionLocation = drone.Position;
            PackageWorth = (AssignedJob == null) ? 0 : AssignedJob.Earnings;
            SimManager.AllRetiredDrones.Add(UID, this);
            if (!sold)
            {
                OtherDroneName = "Environment";
                SimulationEvent.Invoke(EventType.Collision, new DroneCollision(this));
            }
            else
            {
                OtherDroneName = "Retired";
                SimulationEvent.Invoke(EventType.DroneRetired, new DroneCollision(this));
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
            SimManager.AllRetiredDrones.Add(UID, this);
        }

        #region Fields
        private SecureSortedSet<int, ISingleDataSourceReceiver> _Connections;
        #endregion

        #region IDronesObject
        public string Name { get; }

        public Job AssignedJob { get; }

        public Hub AssignedHub { get; }

        public Drone AssignedDrone { get; }
        #endregion

        #region IDataSource
        public uint UID { get; }

        public bool IsDataStatic { get; } = true;

        public AbstractInfoWindow InfoWindow { get; set; }

        public SecureSortedSet<int, ISingleDataSourceReceiver> Connections
        {
            get
            {
                if (_Connections == null)
                {
                    _Connections = new SecureSortedSet<int, ISingleDataSourceReceiver>((x, y) => (x.OpenTime <= y.OpenTime) ? -1 : 1)
                    {
                        MemberCondition = (ISingleDataSourceReceiver obj) => obj is ListTuple || obj is RetiredDroneWindow
                    };
                }
                return _Connections;
            }
        }

        public int TotalConnections => Connections.Count;

        private readonly string[] infoOutput = new string[12];
        private readonly string[] listOutput = new string[4];

        public string[] GetData(WindowType windowType)
        {
            if (windowType == WindowType.RetiredDrone)
            {
                infoOutput[0] = Name;
                infoOutput[1] = HubName;
                infoOutput[2] = CoordinateConverter.ToString(Waypoint);
                infoOutput[3] = DestroyedTime.ToString();
                infoOutput[4] = CoordinateConverter.ToString(CollisionLocation);
                infoOutput[5] = "$" + PackageWorth.ToString("0.00");
                infoOutput[6] = OtherDroneName;
                infoOutput[7] = BatteryCharge.ToString("0.000");
                infoOutput[8] = (AssignedJob == null) ? "" : AssignedJob.Name;
                infoOutput[9] = (AssignedJob == null) ? "" : CoordinateConverter.ToString(AssignedJob.Origin);
                infoOutput[10] = (AssignedJob == null) ? "" : CoordinateConverter.ToString(AssignedJob.Destination);
                infoOutput[11] = (AssignedJob == null) ? "" : AssignedJob.Deadline.ToString();
                return infoOutput;
            }
            if (windowType == WindowType.RetiredDroneList)
            {
                listOutput[0] = Name;
                listOutput[1] = DestroyedTime.ToString();
                listOutput[2] = CoordinateConverter.ToString(CollisionLocation);
                listOutput[3] = "$" + PackageWorth.ToString("0.00");
                return listOutput;
            }
            throw new ArgumentException("Wrong Window Type Supplied");
        }

        public void OpenInfoWindow()
        {
            if (InfoWindow == null)
            {
                InfoWindow = (RetiredDroneWindow)UIObjectPool.Get(WindowType.RetiredDrone, Singletons.UICanvas);
                InfoWindow.Source = this;
                Connections.Add(InfoWindow.UID, InfoWindow);
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

        public Vector2 CollisionLocation { get; }

        public Vector2 Waypoint { get; }

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
