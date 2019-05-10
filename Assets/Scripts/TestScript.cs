using System.Collections;
using UnityEngine;
using Drones;
using Drones.Utils;

public class TestScript : MonoBehaviour
{

    private void Start()
    {
        PoolController.Get(ListElementPool.Instance);
        PoolController.Get(ObjectPool.Instance);
        PoolController.Get(WindowPool.Instance);
    }
}
