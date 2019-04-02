using UnityEngine;

namespace Drones.EventSystem
{
    using Utils;
    public class ListUpdate : IEvent
    {
        public ListUpdate(string name, WindowType window)
        {
            Type = EventType.ListUpdate;
            Window = window;
            GO = null;
            ID = name;
            Target = null;
            Message = "";
            ToConsole = false;
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