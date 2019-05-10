using System;

namespace Drones.UI
{
    using UnityEngine;
    using Utils;
    using static Singletons;
    public class RetiredDroneListWindow : ObjectListWindow
    {
        public static RetiredDroneListWindow New() => PoolController.Get(WindowPool.Instance).Get<RetiredDroneListWindow>(UICanvas);

        public override ListElement TupleType { get; } = ListElement.RetiredDroneList;

        protected override Vector2 MaximizedSize { get; } = new Vector2(1000, 650);
    }
}