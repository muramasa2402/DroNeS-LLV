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
        public DestroyedDrone(Drone drone, Collider other)
        {
            UID = drone.UID;
            Name = drone.Name;
            AssignedJob = drone.AssignedJob;
            HubName = (drone.AssignedHub == null) ? "" : drone.AssignedHub.Name;
            AssignedHub = null;
            AssignedDrone = null;
            CompletedJobs = drone.CompletedJobs;
            drone.StopCoroutine(drone.AssignedBattery.Operate());
            BatteryCharge = drone.AssignedBattery.Charge;
            var collidee = other.GetComponent<Drone>();
            Debug.Log(collidee);
            if (collidee != null)
            {
                OtherDroneName = collidee.Name;
                _OtherUID = collidee.UID;
            }
            Waypoint = drone.Waypoint.ToCoordinates();
            DestroyedTime = TimeKeeper.Chronos.Get();
            CollisionLocation = drone.Position;
            PackageWorth = (AssignedJob == null) ? 0 : AssignedJob.ExpectedEarnings;
            SimulationEvent.Invoke(EventType.Collision, new DroneCollision(this));
        }

        public DestroyedDrone(Drone drone)
        {
            UID = drone.UID;
            Name = drone.Name;
            AssignedJob = drone.AssignedJob;
            HubName = (drone.AssignedHub == null) ? "" : drone.AssignedHub.Name;
            AssignedHub = null;
            AssignedDrone = null;
            CompletedJobs = drone.CompletedJobs;
            drone.StopCoroutine(drone.AssignedBattery.Operate());
            BatteryCharge = drone.AssignedBattery.Charge;
            OtherDroneName = null;
            Waypoint = drone.Waypoint.ToCoordinates();
            DestroyedTime = TimeKeeper.Chronos.Get();
            CollisionLocation = drone.Position;
            PackageWorth = (AssignedJob == null) ? 0 : AssignedJob.ExpectedEarnings;
            SimulationEvent.Invoke(EventType.Collision, new DroneCollision(this));
        }

        #region Fields
        private SecureSortedSet<int, ISingleDataSourceReceiver> _Connections;
        #endregion

        #region IDronesObject
        public string Name { get; }

        public Job AssignedJob { get; }

        public Hub AssignedHub { get; }

        public Drone AssignedDrone { get; }

        public SecureSortedSet<uint, IDataSource> CompletedJobs { get; }
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
                        MemberCondition = (ISingleDataSourceReceiver obj) => obj is ListTuple || obj is DestroyedDroneWindow
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
            if (windowType == WindowType.DestroyedDrone)
            {
                infoOutput[0] = Name;
                infoOutput[1] = HubName;
                infoOutput[2] = StaticFunc.CoordString(Waypoint);
                infoOutput[3] = DestroyedTime.ToString();
                infoOutput[4] = StaticFunc.CoordString(CollisionLocation);
                infoOutput[5] = "$" + PackageWorth.ToString("0.00");
                infoOutput[6] = (OtherDrone == null) ? "" : OtherDrone.Name;
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
                InfoWindow = (DestroyedDroneWindow)UIObjectPool.Get(WindowType.DestroyedDrone, Singletons.UICanvas);
                InfoWindow.Source = this;
                Connections.Add(InfoWindow.UID, InfoWindow);
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

        private readonly uint _OtherUID;

        public string OtherDroneName { get; }

        public float BatteryCharge { get; }

        public DestroyedDrone OtherDrone => (DestroyedDrone)SimManager.AllDestroyedDrones[_OtherUID];
    }
}
