using UnityEngine;
namespace Drones.Interface
{
    public interface IPoolable
    {
        void SelfRelease();
        void OnRelease();
        void OnGet(Transform parent);
    }
}
