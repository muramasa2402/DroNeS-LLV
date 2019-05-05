using UnityEngine;

namespace Drones.EventSystem
{
    using Utils.Extensions;
    using Utils;
    using System;

    public class NoFlyZoneEntry : IEvent
    {
        public NoFlyZoneEntry(Drone drone, NoFlyZone nfz)
        {
            Type = EventType.EnteredNoFlyZone;
            OpenWindow = () => { if (ID == drone.Name) drone.OpenInfoWindow(); };
            ID = drone.Name;
            Target = nfz.Location.ToUnity().ToArray();
            Time = TimeKeeper.Chronos.Get();
            Message = Time.ToString() + " - " + ID + " entered illegal airspace " + nfz.name;
        }

        public EventType Type { get; }
        public string ID { get; }
        public float[] Target { get; }
        public Action OpenWindow { get; }
        public string Message { get; }
        public TimeKeeper.Chronos Time { get; }
    }
}
