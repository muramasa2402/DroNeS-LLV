using System;

namespace Drones.UI
{
    using Utils;
    public class HubListWindow : AbstractListWindow
    {
        public override Type DataSourceType { get; } = typeof(Hub);

        public override WindowType Type { get; } = WindowType.HubList;

        public override ListElement TupleType { get; } = ListElement.HubList;
    }
}