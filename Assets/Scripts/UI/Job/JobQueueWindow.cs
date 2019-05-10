using System;

namespace Drones.UI
{
    using Utils;
    public class JobQueueWindow : AbstractListWindow
    {
        public static JobQueueWindow New() => PoolController.Get(WindowPool.Instance).Get<JobQueueWindow>(Singletons.UICanvas);

        public override Type DataSourceType { get; } = typeof(Job);

        public override WindowType Type { get; } = WindowType.JobQueue;

        public override ListElement TupleType { get; } = ListElement.JobQueue;
    }
}