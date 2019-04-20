using System;

namespace Drones.UI
{
    using Utils;
    public class DestroyedDroneListWindow : AbstractListWindow
    {
        public override Type DataSourceType { get; } = typeof(DestroyedDrone);

        public override WindowType Type { get; } = WindowType.DestroyedDroneList;

        public override ListElement TupleType { get; } = ListElement.DestroyedDroneList;

    }
}