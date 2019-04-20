using System;
using UnityEngine;

namespace Drones
{
    using Drones.EventSystem;
    using Drones.UI;
    using Drones.Utils;
    using Drones.DataStreamer;
    using Drones.Utils.Extensions;
    public class DestroyedDrone : IDronesObject, IDataSource
    {
        private static uint _Count;

        public DestroyedDrone(Drone drone, Collider collider)
        {
            UID = _Count++;
            Name = drone.Name;
            AssignedJob = drone.AssignedJob;
            HubName = (drone.AssignedHub == null) ? "" : drone.AssignedHub.Name;
            AssignedHub = null;
            AssignedDrone = null;
            CompletedJobs = drone.CompletedJobs;
            drone.StopCoroutine(drone.AssignedBattery.Operate());
            BatteryCharge = drone.AssignedBattery.Charge;
            var collidee = collider.GetComponent<Drone>();
            if (collidee != null)
            {
                CollidedWithDroneName = collidee.name;
            }
            Waypoint = drone.Waypoint.ToCoordinates();
            DestroyedTime = TimeKeeper.Chronos.Get();
            CollisionLocation = drone.Position;
            PackageWorth = (AssignedJob == null) ? 0 : AssignedJob.ExpectedEarnings;
            SimulationEvent.Invoke(EventType.Collision, new DroneCollision(this));
        }

        public DestroyedDrone(Drone drone)
        {
            UID = _Count++;
            Name = drone.Name;
            AssignedJob = drone.AssignedJob;
            HubName = (drone.AssignedHub == null) ? "" : drone.AssignedHub.Name;
            AssignedHub = null;
            AssignedDrone = null;
            CompletedJobs = drone.CompletedJobs;
            drone.StopCoroutine(drone.AssignedBattery.Operate());
            BatteryCharge = drone.AssignedBattery.Charge;
            CollidedWithDroneName = null;
            Waypoint = drone.Waypoint.ToCoordinates();
            DestroyedTime = TimeKeeper.Chronos.Get();
            CollisionLocation = drone.Position;
            PackageWorth = (AssignedJob == null) ? 0 : AssignedJob.ExpectedEarnings;
            SimulationEvent.Invoke(EventType.Collision, new DroneCollision(this));
        }

        #region Fields
        private SecureSet<ISingleDataSourceReceiver> _Connections;
        private DestroyedDrone _CollidedWith;

        #endregion

        #region IDronesObject
        public uint UID { get; }

        public string Name { get; }

        public Job AssignedJob { get; }

        public Hub AssignedHub { get; }

        public Drone AssignedDrone { get; }

        public SecureSet<IDataSource> CompletedJobs { get; }
        #endregion

        #region IDataSource
        public bool IsDataStatic { get; } = true;

        public AbstractInfoWindow InfoWindow { get; set; }

        public SecureSet<ISingleDataSourceReceiver> Connections
        {
            get
            {
                if (_Connections == null)
                {
                    _Connections = new SecureSet<ISingleDataSourceReceiver>
                    {
                        MemberCondition = (ISingleDataSourceReceiver obj) => obj is ListTuple || obj is DroneWindow
                    };
                }
                return _Connections;
            }
        }

        public int TotalConnections
        {
            get
            {
                return Connections.Count;
            }
        }

        private readonly string[] infoOutput = new string[12];
        private readonly string[] listOutput = new string[4];

        public string[] GetData(WindowType windowType)
        {
            if (windowType == WindowType.DestroyedDrone)
            {
                infoOutput[0] = Name;
                infoOutput[1] = HubName;
                infoOutput[2] = StaticFunc.CoordString(Waypoint);
                infoOutput[3] = DestroyedTime.ToString();
                infoOutput[4] = StaticFunc.CoordString(CollisionLocation);
                infoOutput[5] = "$" + PackageWorth.ToString("0.00");
                infoOutput[6] = (CollidedWithDrone == null) ? "" : CollidedWithDrone.Name;
                infoOutput[7] = BatteryCharge.ToString("0.000");
                infoOutput[8] = (AssignedJob == null) ? "" : AssignedJob.Name;
                infoOutput[9] = (AssignedJob == null) ? "" : StaticFunc.CoordString(AssignedJob.Origin);
                infoOutput[10] = (AssignedJob == null) ? "" : StaticFunc.CoordString(AssignedJob.Destination);
                infoOutput[11] = (AssignedJob == null) ? "" : AssignedJob.Deadline.ToString();
                return infoOutput;
            }
            if (windowType == WindowType.DestroyedDroneList)
            {
                listOutput[0] = Name;
                listOutput[1] = DestroyedTime.ToString();
                listOutput[2] = StaticFunc.CoordString(CollisionLocation);
                listOutput[3] = "$" + PackageWorth.ToString("0.00");
                return listOutput;
            }
            throw new ArgumentException("Wrong Window Type Supplied");
        }

        public void OpenInfoWindow()
        {
            if (InfoWindow == null)
            {
                InfoWindow = (DroneWindow)UIObjectPool.Get(WindowType.Drone, Singletons.UICanvas);
                InfoWindow.Source = this;
                Connections.Add(InfoWindow);
            }
            else
            {
                InfoWindow.transform.SetAsLastSibling();
            }
        }

        #endregion

        public string HubName { get; }

        public float PackageWorth { get; }

        public TimeKeeper.Chronos DestroyedTime { get; }

        public Vector2 CollisionLocation { get; }

        public Vector2 Waypoint { get; }

        public string CollidedWithDroneName { get; }

        public float BatteryCharge { get; }

        public DestroyedDrone CollidedWithDrone
        { 
            get
            {
                if (_CollidedWith == null)
                {
                    _CollidedWith = (DestroyedDrone)SimManager.AllDestroyedDrones.Find((obj) => ((DestroyedDrone)obj).Name == CollidedWithDroneName);
                }
                return _CollidedWith;
            }
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is DestroyedDrone && GetHashCode() == obj.GetHashCode();
        }
    }
}
