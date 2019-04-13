using System.Collections;
using System.Collections.Generic;
using Drones.DataStreamer;
using Drones.UI;
using Drones.Utils;
using UnityEngine;

namespace Drones
{
    public class DestroyedDrone : IDronesObject, IDataSource
    {
        private static uint _Count;

        public DestroyedDrone(Drone drone, Collider collider)
        {
            UID = _Count++;
            Name = drone.Name;
            AssignedJob = drone.AssignedJob;
            AssignedHub = drone.AssignedHub;
            AssignedDrone = null;
            CompletedJobs = drone.CompletedJobs;

            var collidee = collider.GetComponent<Drone>();
            if (collidee != null)
            {
                CollidedWithName = collidee.name;
            }
            CollisionTime = TimeKeeper.Chronos.Get();
            PackageWorth = AssignedJob.Revenue;
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

        public float PackageWorth { get; }

        public TimeKeeper.Chronos CollisionTime { get; }

        private string CollidedWithName { get; }

        public DestroyedDrone CollidedWith 
        { 
            get
            {
                if (_CollidedWith == null)
                {
                    _CollidedWith = (DestroyedDrone)SimManager.AllDestroyedDrones.Find((obj) => ((DestroyedDrone)obj).Name == CollidedWithName);
                }
                return _CollidedWith;
            }
        }

        #region IDataSource
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

        public string[] GetData(WindowType windowType)
        {
            return new string[1];
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
