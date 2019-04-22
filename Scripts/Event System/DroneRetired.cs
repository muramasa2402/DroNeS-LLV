namespace Drones.EventSystem
{
    using Utils;
    using System;

    public class DroneRetired : IEvent
    {
        public DroneRetired(RetiredDrone a)
        {
            Type = EventType.DroneRetired;
            OpenWindow = a.OpenInfoWindow;
            ID = a.Name;
            Target = null;
            Time = TimeKeeper.Chronos.Get();
            if (a.OtherDroneName != null)
            {
                Message = Time.ToString() + " - " + ID + " retired";
            }
        }

        public EventType Type { get; }
        public string ID { get; }
        public float[] Target { get; }
        public Action OpenWindow { get; }
        public string Message { get; }
        public TimeKeeper.Chronos Time { get; }
    }
}
