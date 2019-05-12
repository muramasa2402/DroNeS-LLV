using System;

namespace Drones.UI
{
    using UnityEngine;
    using Utils;
    public class HubListWindow : ObjectListWindow
    {
        public static HubListWindow New() => PoolController.Get(WindowPool.Instance).Get<HubListWindow>(null);

        public override ListElement TupleType { get; } = ListElement.HubList;

        protected override Vector2 MaximizedSize { get; } = new Vector2(1000, 650);
    }
}