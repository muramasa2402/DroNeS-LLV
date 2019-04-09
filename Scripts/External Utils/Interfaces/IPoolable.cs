using UnityEngine;
namespace Drones.Interface
{
    public interface IPoolable
    {
        void OnRelease();
        void OnGet(Transform parent);
    }
}
