using Drones.Utils;
using UnityEngine;
namespace Drones.Interface
{
    public interface IPoolable
    {
        void Delete();
        void OnRelease();
        void OnGet(Transform parent);
        bool InPool { get; }
        PoolController PC();
    }
}
