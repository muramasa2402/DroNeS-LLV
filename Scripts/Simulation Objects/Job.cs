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
            PackageXArea = data.packageXarea;
            if (data.cost_function != null)
            {
                CostFunc = new CostFunction(data.cost_function);
            }
            if (data.createdUnity != null)
            {
                Created = new TimeKeeper.Chronos(data.createdUnity).SetReadOnly();
            }
            else
            {
                Created = TimeKeeper.Chronos.Get();
            }
            if (data.assignedTime != null)
            {
                AssignedTime = new TimeKeeper.Chronos(data.assignedTime).SetReadOnly();
            }
            if (data.deadline != null)
            {
                Deadline = new TimeKeeper.Chronos(data.deadline).SetReadOnly();
                Earnings = CostFunc.GetPaid(Deadline - 1f, Deadline);
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

        private string[] infoWindow = new string[12];
        private string[] queueWindow = new string[5];
        private string[] historyWindow = new string[4];
        public string[] GetData(WindowType windowType)
        {
            if (windowType == WindowType.Job)
            {
                infoWindow[0] = Name;
                infoWindow[1] = CoordinateConverter.ToString(Origin);
                infoWindow[2] = CoordinateConverter.ToString(Destination);
                infoWindow[3] = (Created is null) ? "" : Created.ToString();
                infoWindow[4] = (AssignedTime is null) ? "" : AssignedTime.ToString();
                infoWindow[5] = (Deadline is null) ? "" : Deadline.ToString();
                infoWindow[6] = (CompletedOn is null) ? "" : CompletedOn.ToString();
                infoWindow[7] = UnitConverter.Convert(Mass.g, PackageWeight);
                infoWindow[8] = "$" + Earnings.ToString();
                infoWindow[9] = (Deadline is null) ? "" : UnitConverter.Convert(Chronos.min, Deadline.Timer());
                infoWindow[10] = (AssignedDrone is null) ? "" : AssignedDrone.Name;
                infoWindow[11] = Progress().ToString();
            }
            else if (windowType == WindowType.JobQueue)
            {
                queueWindow[0] = CoordinateConverter.ToString(Origin);
                queueWindow[1] = CoordinateConverter.ToString(Destination);
                queueWindow[2] = (Created is null) ? "" : Created.ToString();
                queueWindow[3] = (AssignedTime is null) ? "" : AssignedTime.ToString();
                queueWindow[4] = (AssignedDrone is null) ? "" : AssignedDrone.Name;
            }
            else if (windowType == WindowType.JobHistory)
            {
                historyWindow[0] = CoordinateConverter.ToString(Origin);
                historyWindow[1] = CoordinateConverter.ToString(Destination);
                historyWindow[2] = (Deadline is null) ? "" : UnitConverter.Convert(Chronos.min, Deadline.Timer());
                historyWindow[3] = "$" + Earnings.ToString();
            }
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
        public float Earnings { get; private set; }
        public TimeKeeper.Chronos Created { get; private set; }
        public TimeKeeper.Chronos AssignedTime { get; private set; }
        public TimeKeeper.Chronos Deadline { get; private set; }
        public TimeKeeper.Chronos CompletedOn { get; private set; }
        public float PackageWeight { get; }
        public float PackageXArea { get; }
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
            Earnings = CostFunc.GetPaid(CompletedOn, Deadline);
            SimManager.UpdateRevenue(Earnings);
        }

        private float Progress()
        {
            if (CompletedOn is null)
            {
                if (AssignedDrone is null) return 0.00f;
                return AssignedDrone.JobProgress;
            }
            return 1.00f;
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