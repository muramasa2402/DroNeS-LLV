using System;
using UnityEngine;
namespace Drones.UI
{
    using Utils;
    public class DroneListWindow : AbstractListWindow
    {
        public static DroneListWindow New() => PoolController.Get(WindowPool.Instance).Get<DroneListWindow>(Singletons.UICanvas);

        public override Type DataSourceType { get; } = typeof(Drone);

        public override WindowType Type { get; } = WindowType.DroneList;

        public override ListElement TupleType { get; } = ListElement.DroneList;

    }
}