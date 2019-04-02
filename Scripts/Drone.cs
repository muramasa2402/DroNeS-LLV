using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mapbox.Utils;

namespace Drones
{
    using Drones.Utils;
    using Drones.Utils.Extensions;
    public class Drone : MonoBehaviour, IDronesObject
    {
        public Dictionary<WindowType, int> Connections { get; }
        = new Dictionary<WindowType, int>
        {
            { WindowType.Drone, 0 },
            { WindowType.DroneList, 0 }
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

        public Vector2d Position 
        {
            get
            {
                return transform.position.ToCoordinates();
            }
        }

        public float HubDistance
        {
            get
            {
                if (AssignedHub != null)
                {
                    return Functions.Metre(Position, AssignedHub.Position);
                }
                return -1;
            }
        }

        public int JobProgress
        {
            get
            {
                if (AssignedJob != null)
                {
                    float a = Functions.Metre(Position, AssignedJob.Destination);
                    float b = Functions.Metre(AssignedJob.Origin, AssignedJob.Destination);
                    int c = (int)(100 * a / b);
                    return c;
                }
                return 0;
            }
        }

        public string[] GetData(WindowType windowType)
        {
            return new string[1];
        }

    };
}
