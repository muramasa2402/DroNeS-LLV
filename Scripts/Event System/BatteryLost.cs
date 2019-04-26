namespace Drones.EventSystem
{
    using Utils;
    using System;

    public class BatteryLost : IEvent
    {
        public BatteryLost(Battery battery)
        {
            Type = EventType.BatteryLost;
            OpenWindow = null;
            ID = battery?.Name;
            Target = null;
            Time = TimeKeeper.Chronos.Get();
            Message = "";
            battery?.AssignedHub.DestroyBattery(battery);
        }

        public EventType Type { get; }
        public string ID { get; }
        public float[] Target { get; }
        public Action OpenWindow { get; }
        public string Message { get; }
        public TimeKeeper.Chronos Time { get; }
    }
}
