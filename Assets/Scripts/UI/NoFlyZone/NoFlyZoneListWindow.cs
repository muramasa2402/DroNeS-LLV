using System;
using Drones.Utils;
using UnityEngine;

namespace Drones.UI
{
    public class NoFlyZoneListWindow : ObjectListWindow
    {
        public static NoFlyZoneListWindow New() => PoolController.Get(WindowPool.Instance).Get<NoFlyZoneListWindow>(Singletons.UICanvas);

        public override ListElement TupleType { get; } = ListElement.NFZList;

        protected override Vector2 MaximizedSize { get; } = new Vector2(730, 600);
    }
}
