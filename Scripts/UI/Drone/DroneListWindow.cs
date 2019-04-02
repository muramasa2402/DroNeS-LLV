namespace Drones.UI
{
    using System;
    using Utils;
    public class DroneListWindow : AbstractListWindow
    {
        public override Type DataSourceType { get; } = typeof(Drone);

        public override WindowType Type { get; } = WindowType.DroneList;

    }
}