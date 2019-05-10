using System;

namespace Drones.UI
{
    using Utils;
    using static Singletons;
    public class RetiredDroneListWindow : AbstractListWindow
    {
        public static RetiredDroneListWindow New() => PoolController.Get(WindowPool.Instance).Get<RetiredDroneListWindow>(UICanvas);

        public override Type DataSourceType { get; } = typeof(RetiredDrone);

        public override WindowType Type { get; } = WindowType.RetiredDroneList;

        public override ListElement TupleType { get; } = ListElement.RetiredDroneList;

    }
}