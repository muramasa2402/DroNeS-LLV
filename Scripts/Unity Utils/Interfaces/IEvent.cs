using UnityEngine;

namespace Drones.EventSystem
{
    using System;
    using Utils;

    public interface IEvent
    {
        EventType Type { get; }
        string ID { get; }
        float[] Target { get; }
        Action OpenWindow { get; }
        TimeKeeper.Chronos Time { get; }
        string Message { get; }
    }
}
