using System;
using Drones.Utils;

namespace Drones.UI
{
    public class NoFlyZoneListWindow : AbstractListWindow
    {
        public override ListElement TupleType { get; } = ListElement.NFZList;

        //TODO
        public override Type DataSourceType => throw new NotImplementedException();

        public override WindowType Type { get; } = WindowType.NFZList;
    }
}
