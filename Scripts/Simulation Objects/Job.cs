using System.Collections.Generic;
using UnityEngine;

namespace Drones
{
    using Utils.Extensions;
    using Utils;
    using DataStreamer;
    using UI;
    using Serializable;
    using Managers;

    public class Job : IDataSource
    {
        private SecureSortedSet<int, ISingleDataSourceReceiver> _Connections;

        public Job(SJob data) 
        {
            UID = data.uid;
            Status = data.status;
            Name = "J" + UID.ToString("000000000");
            PackageWeight = data.packageWeight;
            PackageXArea = data.packageXarea;
            if (data.costFunction != null)
            {
                CostFunc = new CostFunction(data.costFunction);
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
                CompletedBy = data.droneUID;
                CompletedOn = new TimeKeeper.Chronos(data.completedOn).SetReadOnly();
                Pickup = data.pickup;
                Dest = data.destination;
            }
            else
            {
                Vector3 o = data.pickup;
                o.y = 600;
                Vector3 d = data.destination;
                d.y = 600;
                Vector3 dir = Random.insideUnitSphere.normalized;
                dir.y = 0;
                while (Physics.Raycast(new Ray(o, Vector3.down), out RaycastHit info, 600, 1 << 12))
                {
                    var v = info.collider.ClosestPoint(info.transform.position + 500 * dir);
                    o += (v - o).normalized * 4 + (v - o);
                }
                while (Physics.Raycast(new Ray(d, Vector3.down), out RaycastHit info, 600, 1 << 12))
                {
                    var v = info.collider.ClosestPoint(info.transform.position + 500 * dir);
                    d += (v - d).normalized * 4 + (v - d);
                }
                o.y = 0;
                d.y = 0;
                Pickup = o;
                Dest = d;
            }

        }

        #region Fields
        private Drone _AssignedDrone;
        #endregion

        public uint UID { get; private set; }
        public string Name { get; private set; }
        public Drone AssignedDrone
        {
            get => _AssignedDrone;

            set
            {
                _AssignedDrone = value;
                if (_AssignedDrone != null)
                {
                    Status = JobStatus.Pickup;
                }
            }
        }

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

        private readonly string[] infoWindow = new string[12];
        private readonly string[] queueWindow = new string[5];
        private readonly string[] historyWindow = new string[4];
        public string[] GetData(WindowType windowType)
        {
            if (windowType == WindowType.Job)
            {
                infoWindow[0] = Name;
                infoWindow[1] = Pickup.ToStringXZ();
                infoWindow[2] = Dest.ToStringXZ();
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
                queueWindow[0] = Pickup.ToStringXZ();
                queueWindow[1] = Dest.ToStringXZ();
                queueWindow[2] = (Created is null) ? "" : Created.ToString();
                queueWindow[3] = (AssignedTime is null) ? "" : AssignedTime.ToString();
                queueWindow[4] = (AssignedDrone is null) ? "" : AssignedDrone.Name;
            }
            else if (windowType == WindowType.JobHistory)
            {
                historyWindow[0] = Pickup.ToStringXZ();
                historyWindow[1] = Dest.ToStringXZ();
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

        public JobStatus Status { get; private set; }
        public Vector3 Dest { get; }
        public Vector3 Pickup { get; }
        public float Earnings { get; private set; }
        public TimeKeeper.Chronos Created { get; private set; }
        public TimeKeeper.Chronos AssignedTime { get; private set; }
        public TimeKeeper.Chronos Deadline { get; private set; }
        public TimeKeeper.Chronos CompletedOn { get; private set; }
        public float PackageWeight { get; }
        public float PackageXArea { get; }
        public uint CompletedBy { get; private set; } = 0;
        public CostFunction CostFunc { get; }
        // More stuff....
        public void FailJob() 
        {
            IsDataStatic = true;
            CompletedOn = new TimeKeeper.Chronos(int.MaxValue, 23, 59, 59.999999f);
            AssignedDrone.AssignedJob = null;
            AssignedDrone = null;
            Status = JobStatus.Failed;
            SimManager.UpdateRevenue(CostFunc.GetPaid(CompletedOn, Deadline));
        }

        public void CompleteJob()
        {
            CompletedOn = TimeKeeper.Chronos.Get().SetReadOnly();
            Status = JobStatus.Complete;
            IsDataStatic = true;
            AssignedDrone.CompletedJobs.Add(UID, this);
            AssignedDrone.UpdateDelay(Deadline.Timer());
            CompletedBy = AssignedDrone.UID;
            AssignedDrone.AssignedJob = null;
            AssignedDrone = null;
            Earnings = CostFunc.GetPaid(CompletedOn, Deadline);
            SimManager.UpdateDelay(Deadline.Timer());
            SimManager.UpdateRevenue(Earnings);
        }

        public void StartDelivery() => Status = JobStatus.Delivering;

        private float Progress()
        {
            if (CompletedOn is null)
            {
                if (Status != JobStatus.Delivering) return 0.00f;
                return AssignedDrone.JobProgress;
            }
            return 1.00f;
        }

        public override string ToString() => Name;

        public SJob Serialize()
        {
            var output = new SJob
            {
                uid = UID,
                packageWeight = PackageWeight,
                costFunction = CostFunc.Serialize(),
                completedOn = CompletedOn.Serialize(),
                deadline = Deadline.Serialize(),
                status = Status,
                pickup = Pickup,
                destination = Dest
            };

            if (CompletedBy == 0 && AssignedDrone == null)
            {
                output.droneUID = 0;
            }
            else
            {
                output.droneUID = (CompletedBy == 0) ? AssignedDrone.UID : CompletedBy;
            }


            return output;
        }
        
    };
}