using System.Collections.Generic;
using UnityEngine;

namespace Drones
{
    using Utils;
    using DataStreamer;
    using Drones.UI;
    using Drones.Serializable;
    public class Job : IDronesObject, IDataSource
    {
        private SecureSortedSet<int, ISingleDataSourceReceiver> _Connections;

        public Job(SJob data) 
        {
            UID = data.uid;
            Name = "J" + UID.ToString("000000000");
            PackageWeight = data.packageWeight;
            if (data.cost_function != null)
            {
                CostFunc = new CostFunction(data.cost_function);
            }
            if (data.deadline != null)
            {
                Deadline = new TimeKeeper.Chronos(data.deadline).SetReadOnly();
                ExpectedEarnings = CostFunc.GetPaid(Deadline - 1f, Deadline);
            }
            if (data.completedOn != null)
            {
                CompletedOn = new TimeKeeper.Chronos(data.completedOn).SetReadOnly();
            }
        }

        #region IDronesObject
        public uint UID { get; private set; }
        public string Name { get; private set; }
        public Job AssignedJob => this;
        public Hub AssignedHub => null;
        public Drone AssignedDrone { get; set; }
        #endregion

        #region IDataSource
        public SecureSortedSet<int, ISingleDataSourceReceiver> Connections
        {
            get
            {
                if (_Connections == null)
                {
                    _Connections = new SecureSortedSet<int, ISingleDataSourceReceiver>
                    {
                        MemberCondition = (ISingleDataSourceReceiver obj) => obj is ListTuple || obj is DroneWindow
                    };
                }
                return _Connections;
            }
        }

        public int TotalConnections => Connections.Count;

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
                Connections.Add(InfoWindow.UID, InfoWindow);
            }
            else
            {
                InfoWindow.transform.SetAsLastSibling();
            }
        }

        public bool IsDataStatic { get; private set; } = false;
        #endregion

        public Vector2 Destination { get; private set; }
        public Vector2 Origin { get; private set; }
        public Status JobStatus { get; }
        public float ExpectedEarnings { get; private set; }
        public TimeKeeper.Chronos Deadline { get; private set; }
        public TimeKeeper.Chronos CompletedOn { get; private set; }
        public float PackageWeight { get; }
        public CostFunction CostFunc { get; }
        // More stuff....
        public void FailJob() 
        {
            IsDataStatic = true;
            CompletedOn = new TimeKeeper.Chronos(int.MaxValue, 23, 59, 59.999999f);
            SimManager.UpdateRevenue(CostFunc.GetPaid(CompletedOn, Deadline));
        }

        public void CompleteJob()
        {
            IsDataStatic = true;
            CompletedOn = TimeKeeper.Chronos.Get().SetReadOnly();
            AssignedDrone.UpdateDelay(Deadline.Timer());
            SimManager.UpdateDelay(Deadline.Timer());
            SimManager.UpdateRevenue(CostFunc.GetPaid(CompletedOn, Deadline));
        }

        public override string ToString() => Name;

        public SJob Serialize()
        {
            return new SJob
            {
                uid = UID,
                packageWeight = PackageWeight,
                cost_function = CostFunc.Serialize(),
                completedOn = CompletedOn.Serialize(),
                deadline = Deadline.Serialize()
            };
        }
        
    };
}