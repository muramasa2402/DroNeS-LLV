using UnityEngine;

namespace Drones.EventSystem
{
    using Utils.Extensions;
    using Utils;
    using System;
    using Drones.DataStreamer;

    public class NoFlyZoneEntry : IEvent
    {
        public NoFlyZoneEntry(IDataSource obj, NoFlyZone nfz)
        {
            Type = EventType.EnteredNoFlyZone;
            OpenWindow = () => { if (ID == obj.ToString()) obj.OpenInfoWindow(); };
            ID = obj.ToString();
            Target = nfz.Location.ToUnity().ToArray();
            Time = TimeKeeper.Chronos.Get();
            Message = Time + " - " + ID + " entered illegal airspace " + nfz.Name;
        }

        public EventType Type { get; }
        public string ID { get; }
        public float[] Target { get; }
        public Action OpenWindow { get; }
        public string Message { get; }
        public TimeKeeper.Chronos Time { get; }
    }
}
