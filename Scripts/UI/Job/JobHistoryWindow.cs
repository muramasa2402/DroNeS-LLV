using System;

namespace Drones.UI
{
    using Utils;

    public class JobHistoryWindow : AbstractListWindow
    {
        public override Type DataSourceType { get; } = typeof(Job);

        public override WindowType Type { get; } = WindowType.JobHistory;

        public override ListElement TupleType { get; } = ListElement.JobHistory;
    }
}