using System;

namespace Drones.UI
{
    using UnityEngine;
    using Utils;
    using static Singletons;
    public class JobHistoryWindow : ObjectListWindow
    {
        public static JobHistoryWindow New() => PoolController.Get(WindowPool.Instance).Get<JobHistoryWindow>(UICanvas);

        public override ListElement TupleType { get; } = ListElement.JobHistory;

        protected override Vector2 MaximizedSize { get; } = new Vector2(1000, 500);
    }
}