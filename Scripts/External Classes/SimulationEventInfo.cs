namespace Drones.Struct
{
    public struct SimulationEventInfo
    {
        public string ID { get; private set; }
        public float[] Target { get; private set; }
        public int WindowID { get; private set; }
        public string Message { get; private set; }

        public SimulationEventInfo(string name, float[] target)
        {
            ID = name;
            Target = target;
            WindowID = -1;
            Message = "Event " + ID + " occured";
        }

        public SimulationEventInfo(string name, int window)
        {
            ID = name;
            Target = null;
            WindowID = window;
            Message = "Event " + ID + " occured";
        }

        public SimulationEventInfo(string name, float[] target, int window)
        {
            ID = name;
            Target = target;
            WindowID = window;
            Message = "Event " + ID + " occured";
        }

    }
}
