using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Drones.Utils.Jobs
{
    public struct GravityJob : IJobParallelForTransform
    {
        public float deltaTime;
        public const float g = 9.81f;
        public NativeArray<Vector3> v;

        public void Execute(int k, TransformAccess transform)
        {
            transform.position += v[k] * deltaTime;

            v[k] = g * Vector3.down * deltaTime;
        }
    }
}

