using UnityEngine;

namespace Drones.Data
{
    using Utils;
    using Serializable;
    public class JobData : IData
    {
        public uint UID { get; }
        public bool IsDataStatic { get; set; } = false;

        public uint drone;
        public JobStatus status;
        public Vector3 dropoff;
        public Vector3 pickup;
        public float earnings;
        public readonly TimeKeeper.Chronos created;
        public readonly TimeKeeper.Chronos deadline;
        public TimeKeeper.Chronos assignment;
        public TimeKeeper.Chronos completed;
        public float packageWeight;
        public float packageXArea;
        public CostFunction costFunction;

        public JobData(SJob data)
        {
            UID = data.uid;
            status = data.status;
            packageWeight = data.packageWeight;
            packageXArea = data.packageXarea;

            costFunction = new CostFunction(data.costFunction);

            if (data.status != JobStatus.Assigning)
            {
                created = new TimeKeeper.Chronos(data.createdUnity).SetReadOnly();
                assignment = new TimeKeeper.Chronos(data.assignedTime).SetReadOnly();
                deadline = new TimeKeeper.Chronos(data.deadline).SetReadOnly();
                if (data.status != JobStatus.Delivering)
                {
                    completed = new TimeKeeper.Chronos(data.completedOn).SetReadOnly();
                    pickup = data.pickup;
                    dropoff = data.destination;
                }
            }
            else
            {
                created = TimeKeeper.Chronos.Get();
                deadline = TimeKeeper.Chronos.Get() + costFunction.CompleteIn;
                earnings = costFunction.GetPaid(deadline - 1f, deadline); // aproximate earnings
                Vector3 o = data.pickup;
                o.y = 0;
                Vector3 d = data.destination;
                d.y = 0;

                pickup = LandingZoneIdentifier.Reposition(o);
                dropoff = LandingZoneIdentifier.Reposition(d);
            }


        }

    }
}
