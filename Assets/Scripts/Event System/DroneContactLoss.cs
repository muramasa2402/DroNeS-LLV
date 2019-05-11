namespace Drones.EventSystem
{
    using Utils.Extensions;
    using Utils;
    using System;
    using Drones.UI;

    public class DroneContactLoss : IEvent
    {
        public DroneContactLoss(Drone drone)
        {
            var rDrone = new RetiredDrone(drone);
            Type = EventType.DroneContactLoss;
            OpenWindow = rDrone.OpenInfoWindow;
            ID = rDrone.Name;
            Target = rDrone.CollisionLocation.ToArray();
            Time = TimeKeeper.Chronos.Get();
            Message = Time + " - " + ID + " contact lost";
            drone.AssignedHub.Drones.Remove(drone);
            ConsoleLog.WriteToConsole(new BatteryLost(drone.AssignedBattery));
            drone.Delete();
        }

        public EventType Type { get; }
        public string ID { get; }
        public float[] Target { get; }
        public Action OpenWindow { get; }
        public string Message { get; }
        public TimeKeeper.Chronos Time { get; }
    }
}
