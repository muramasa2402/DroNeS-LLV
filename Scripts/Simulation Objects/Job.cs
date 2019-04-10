using System.Collections.Generic;
using UnityEngine;

namespace Drones
{
    using Utils;
    using DataStreamer;
    using Drones.UI;

    public class Job : IDronesObject, IDataSource
    {
        private SecureSet<ISingleDataSourceReceiver> _Connections;

        #region IDronesObject
        public string Name { get; set; }
        public Job AssignedJob { get; set; }
        public Hub AssignedHub { get; set; }
        public Drone AssignedDrone { get; set; }
        #endregion

        #region IDataSource
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
            return null;
        }

        public AbstractInfoWindow InfoWindow { get; set; }

        public void OpenInfoWindow()
        {
            if (InfoWindow == null)
            {
                InfoWindow = (JobWindow)UIObjectPool.Get(WindowType.Job, Singletons.UICanvas);
                InfoWindow.Source = this;
                Connections.Add(InfoWindow);
            }
            else
            {
                InfoWindow.transform.SetAsLastSibling();
            }
        }
        #endregion

        public static Job CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<Job>(jsonString);
        }
        public Vector2 Destination { get; }
        public Vector2 Origin { get; }
        public Status JobStatus { get; }
        public string ID { get; }

        // More stuff....

        public override string ToString()
        {
            return ID;
        }
    };
}