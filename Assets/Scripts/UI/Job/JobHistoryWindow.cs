using System;

namespace Drones.UI
{
    using Utils;
    using static Singletons;
    public class JobHistoryWindow : AbstractListWindow
    {
        public static JobHistoryWindow New() => (JobHistoryWindow)UIObjectPool.Get(WindowType.JobHistory, UICanvas);

        public override Type DataSourceType { get; } = typeof(Job);

        public override WindowType Type { get; } = WindowType.JobHistory;

        public override ListElement TupleType { get; } = ListElement.JobHistory;
    }
}