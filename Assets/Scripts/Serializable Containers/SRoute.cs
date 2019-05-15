using System;
using System.Collections.Generic;

namespace Drones.Serializable
{
    [Serializable]
    public class SRoute
    {
        public List<SVector3> waypoints;
        public bool frequentRequest;
    }
}
