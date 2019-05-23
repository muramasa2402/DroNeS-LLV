using System;

namespace Drones.UI
{
    using DataStreamer;
    using UnityEngine;
    using Utils;
    public class JobQueueWindow : ObjectListWindow
    {
        public static JobQueueWindow New() => PoolController.Get(WindowPool.Instance).Get<JobQueueWindow>(null);

        public override ListElement TupleType { get; } = ListElement.JobQueue;

        protected override Vector2 MaximizedSize { get; } = new Vector2(1180, 500);

        public override void OnNewSource(IDataSource source)
        {
            var element = JobQueueTuple.New(this);
            element.Source = source;
            DataReceivers.Add(source, element);
            ListChanged += element.OnListChange;
            OnContentChange();
        }
    }
}