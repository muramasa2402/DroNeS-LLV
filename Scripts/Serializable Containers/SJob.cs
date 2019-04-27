using System;
using Drones.Utils;

namespace Drones.Serializable
{
    [Serializable]
    public class SCostFunction
    {
        public float valid_time; // seconds
        public float reward;
        public float penalty;
    }

    [Serializable]
    public class SJob
    {
        // Generator set
        public uint uid;
        public int status;
        public long creationTime;
        public string content;
        public float packageWeight;
        public float packageXarea;
        public uint droneUID;
        public SCostFunction costFunction;
        public SVector2 pickup;
        public SVector2 destination;
        // Scheduler set
        public uint droneUID;
        // Unity set
        public STime deadline;
        public STime completedOn;
        public STime createdUnity;
        public STime assignedTime;
    }
}
