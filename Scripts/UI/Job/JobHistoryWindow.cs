namespace Drones.UI
{
    using System;
    using Utils;
    public class JobHistoryWindow : AbstractListWindow
    {
        public override Type DataSourceType { get; } = typeof(Job);

        public override WindowType Type { get; } = WindowType.JobHistory;

    }
}