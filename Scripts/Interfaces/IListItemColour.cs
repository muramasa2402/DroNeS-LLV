using Drones.EventSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Drones.UI
{
    public interface IListItemColour
    {
        Color Odd { get; }
        Color Even { get; }
        Image ItemImage { get; }
        void SetColor();
        void OnListChange(IEvent e);
    }
}
