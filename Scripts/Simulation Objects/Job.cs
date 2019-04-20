using System.Collections.Generic;
using UnityEngine;

namespace Drones
{
    using Utils;
    using DataStreamer;
    using Drones.UI;

    [System.Serializable]
    public class Job : IDronesObject, IDataSource
    {
        private static uint _Count;
        private SecureSet<ISingleDataSourceReceiver> _Connections;

        public Job()
        {
            uid = _Count++;
            Name = "J" + UID.ToString("000000000");
        }

        public uint uid;

        #region IDronesObject
        public uint UID => uid;
        public string Name { get; private set; }
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
                        MemberCondition = (ISingleDataSourceReceiver obj) => obj is ListTuple || obj is JobWindow
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
            //TODO
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

        public bool IsDataStatic { get; set; } = false;

        public static Job CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<Job>(jsonString);
        }

        public Vector2 Destination { get; }
        public Vector2 Origin { get; }
        public Status JobStatus { get; }
        public float ExpectedEarnings { get; }
        public TimeKeeper.Chronos Deadline { get; }
        public float PackageWeight { get; }
        // More stuff....
        public void FailJob() 
        {
            SimManager.LoseMoney(ExpectedEarnings);
        }

        public void CompleteJob()
        {
            if (TimeKeeper.Chronos.Get() < Deadline)
            {
                SimManager.MakeMoney(ExpectedEarnings);
            }
            else
            {
                SimManager.LoseMoney(ExpectedEarnings);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    };
}