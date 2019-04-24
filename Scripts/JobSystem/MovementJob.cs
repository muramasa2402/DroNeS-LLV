using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Drones.Utils.Jobs
{
    public struct MovementInfo
    {
        public float speed;
        public DroneMovement moveType;
        public float height;
        public Vector3 waypoint;
        public int isWaiting;
    }

    public struct MovementJob : IJobParallelForTransform
    {
        public float deltaTime;
        [ReadOnly]
        public NativeArray<MovementInfo> nextMove;

        public void Execute(int k, TransformAccess transform)
        {
            if (nextMove[k].isWaiting != 0) return;

            float step = deltaTime * nextMove[k].speed;
            if (nextMove[k].moveType == DroneMovement.Ascend || nextMove[k].moveType == DroneMovement.Descend)
            {
                Vector3 target = transform.position;
                target.y = nextMove[k].height;
                transform.position = Vector3.MoveTowards(transform.position, target, step);
            }
            else if (nextMove[k].moveType == DroneMovement.Horizontal)
            {
                transform.position = Vector3.MoveTowards(transform.position, nextMove[k].waypoint, step);
            }
        }
    }
}

