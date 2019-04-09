using UnityEngine;

namespace Drones.EventSystem
{
    using Utils;
    public class POIMarked : IEvent
    {
        public POIMarked(string name, float[] target, GameObject go)
        {
            Type = EventType.POIMarked;
            Window = WindowType.Null;
            GO = go;
            ID = name;
            Target = target;
            Message = "POI " + ID + " Marked";
            ToConsole = true;
        }

        public EventType Type { get; }
        public string ID { get; }
        public float[] Target { get; }
        public WindowType Window { get; }
        public string Message { get; }
        public GameObject GO { get; }
        public bool ToConsole { get; }
    }
}
