using System;

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
        public uint uid;
        public long creation_time;
        public string content;
        public float packageWeight;
        public SCostFunction cost_function;
        public SVector2 pick_up;
        public SVector2 destination;
        public STime deadline;
        public STime completedOn;
    }
}
