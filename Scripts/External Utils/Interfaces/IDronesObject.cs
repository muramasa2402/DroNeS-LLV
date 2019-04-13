using System.Collections.Generic;

namespace Drones
{
    using Utils;

    public interface IDronesObject
    {
        uint UID { get; }
        string Name { get; }
        Job AssignedJob { get; }
        Hub AssignedHub { get; }
        Drone AssignedDrone { get; }
    }
}
