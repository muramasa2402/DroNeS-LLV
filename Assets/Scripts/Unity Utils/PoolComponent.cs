using UnityEngine;
using System.Collections;

namespace Drones.Utils
{
    public class PoolComponent : MonoBehaviour
    {
        public IPool pool;
        private void OnDisable()
        {
            StopAllCoroutines();
            PoolController.Clear(pool);
        }
    }
}
