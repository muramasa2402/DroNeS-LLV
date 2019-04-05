using System;
using UnityEngine;
namespace Drones.UI
{
    using Utils;
    public class DroneListWindow : AbstractListWindow
    {
        public override Type DataSourceType { get; } = typeof(Drone);

        public override WindowType Type { get; } = WindowType.DroneList;

        public override ListElement TupleType { get; } = ListElement.DroneList;

    }
}