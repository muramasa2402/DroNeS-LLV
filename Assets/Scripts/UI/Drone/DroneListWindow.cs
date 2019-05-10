using System;

namespace Drones.UI
{
    using UnityEngine;
    using Utils;
    public class DroneListWindow : ObjectListWindow
    {
        public static DroneListWindow New() => PoolController.Get(WindowPool.Instance).Get<DroneListWindow>(Singletons.UICanvas);

        public override ListElement TupleType { get; } = ListElement.DroneList;

        protected override Vector2 MaximizedSize { get; } = new Vector2(1000, 650);
    }
}