using System.Collections.Generic;
using Mapbox.Utils;
using UnityEngine;

namespace Drones
{
    using UI;
    using Utils;
    using Utils.Extensions;
    public class Job : IDronesObject
    {
        #region IDronesObject
        public Dictionary<WindowType, int> Connections { get; set; } = new Dictionary<WindowType, int>
        {
            { WindowType.Job, 0 },
            { WindowType.JobHistory, 0 },
            { WindowType.JobQueue, 0 }
        };

        public int TotalConnections
        {
            get
            {
                int sum = 0;
                foreach (var value in Connections.Values)
                {
                    sum += value;
                }
                return sum;
            }
        }

        public Job AssignedJob { get; set; }
        public Hub AssignedHub { get; set; }
        public Drone AssignedDrone { get; set; }

        public string[] GetData(WindowType windowType)
        {
            return null;
        }
        #endregion

        public static Job CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<Job>(jsonString);
        }
        public Vector2d Destination { get; }
        public Vector2d Origin { get; }
        public Status JobStatus { get; }
        public string ID { get; }

        // More stuff....

        public override string ToString()
        {
            return ID;
        }
    };
}