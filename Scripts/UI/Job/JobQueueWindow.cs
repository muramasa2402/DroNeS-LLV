using System;

namespace Drones.UI
{
    using Utils;
    public class JobQueueWindow : AbstractListWindow
    {
        public override Type DataSourceType { get; } = typeof(Job);

        public override WindowType Type { get; } = WindowType.JobQueue;

        public override ListElement TupleType { get; } = ListElement.JobQueue;
    }
}