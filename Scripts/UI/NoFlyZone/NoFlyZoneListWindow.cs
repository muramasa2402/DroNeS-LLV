using System;
using Drones.Utils;

namespace Drones.UI
{
    public class NoFlyZoneListWindow : AbstractListWindow
    {
        public override ListElement TupleType { get; } = ListElement.NFZList;

        public override Type DataSourceType { get; } = typeof(NoFlyZone);

        public override WindowType Type { get; } = WindowType.NFZList;
    }
}
