using UnityEngine;

namespace Drones.EventSystem
{
    using Utils;

    public interface IEvent
    {
        EventType Type { get; }
        string ID { get; }
        float[] Target { get; }
        WindowType Window { get; }
        string Message { get; }
        GameObject GO { get; }
        bool ToConsole { get; }
    }
}
