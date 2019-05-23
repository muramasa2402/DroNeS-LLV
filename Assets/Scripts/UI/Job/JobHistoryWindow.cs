using System;

namespace Drones.UI
{
    using Drones.DataStreamer;
    using UnityEngine;
    using Utils;
    public class JobHistoryWindow : ObjectListWindow
    {
        public static JobHistoryWindow New() => PoolController.Get(WindowPool.Instance).Get<JobHistoryWindow>(null);

        public override void OnNewSource(IDataSource source)
        {
            var element = JobHistoryTuple.New(this);
            element.Source = source;
            DataReceivers.Add(source, element);
            ListChanged += element.OnListChange;
            OnContentChange();
        }

        public override ListElement TupleType { get; } = ListElement.JobHistory;

        protected override Vector2 MaximizedSize { get; } = new Vector2(1000, 500);


    }
}