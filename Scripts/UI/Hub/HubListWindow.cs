namespace Drones.UI
{
    using System;
    using Utils;
    public class HubListWindow : AbstractListWindow
    {
        public override Type DataSourceType { get; } = typeof(Hub);

        public override WindowType Type { get; } = WindowType.HubList;

    }
}