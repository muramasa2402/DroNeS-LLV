using System;

namespace Drones.UI
{
    using Utils;
    public class RetiredDroneListWindow : AbstractListWindow
    {
        public override Type DataSourceType { get; } = typeof(RetiredDrone);

        public override WindowType Type { get; } = WindowType.RetiredDroneList;

        public override ListElement TupleType { get; } = ListElement.RetiredDroneList;

    }
}