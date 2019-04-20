using UnityEngine;

namespace Drones.EventSystem
{
    using Utils.Extensions;
    using Utils;
    using System;

    public class DroneCollision : IEvent
    {
        public DroneCollision(DestroyedDrone a)
        {
            Type = EventType.Collision;
            OpenWindow = a.OpenInfoWindow;
            ID = a.Name;
            Target = a.CollisionLocation.ToUnity().ToArray();
            Time = TimeKeeper.Chronos.Get();
            if (a.CollidedWithDroneName != null)
            {
                Message = Time.ToString() + " - " + ID + " collided with " + a.CollidedWithDroneName;
            }
            else
            {
                Message = Time.ToString() + " - " + ID + " crashed";
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
