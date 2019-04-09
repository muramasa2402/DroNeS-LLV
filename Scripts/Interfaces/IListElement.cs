using UnityEngine;
using UnityEngine.UI;

namespace Drones.UI
{
    using Drones.EventSystem;
    using Drones.Utils;
    public interface IListElement
    {
        ListElement Type { get; }
        AbstractWindow Window { get; }
        Color Odd { get; }
        Color Even { get; }
        Image ItemImage { get; }
        void SetColor();
        void OnListChange();
    }
}
