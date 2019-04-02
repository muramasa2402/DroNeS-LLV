using System.Collections.Generic;
using Mapbox.Utils;
using UnityEngine;

namespace Drones
{
    using Utils;
    using Utils.Extensions;

    public class Hub : MonoBehaviour, IDronesObject
    {
        public Dictionary<WindowType, int> Connections { get; set; }
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
        public Drone AssignedDrone { get; set; } = null;

        private AlertHashSet<Drone> _Drones;
        public AlertHashSet<Drone> Drones 
        { 
            get 
            { 
                if (_Drones == null)
                {
                    _Drones = new AlertHashSet<Drone>();
                }
                return _Drones;
            } 
        }

        public Vector2d Position
        {
            get
            {
                return transform.position.ToCoordinates();
            }
        }

        public int DroneCount
        {
            get
            {
                return Drones.Count;
            }
        }

        public string[] GetData(WindowType windowType)
        {
            return null;
        }
    };
}