using System.Collections.Generic;

namespace Drones
{
    using DataStreamer;
    using Utils;

    public interface IDronesObject
    {
        Job AssignedJob { get; set; }
        Hub AssignedHub { get; set; }
        Drone AssignedDrone { get; set; }
        string[] GetData(WindowType windowType);
        //HashSet<ISingleDataSourceReceiver> Receivers { get; }
        Dictionary<WindowType, int> Connections { get; }
        int TotalConnections { get; }
    }
}
