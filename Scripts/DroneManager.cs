using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using System.Collections;

namespace Drones
{
    using Drones.DataStreamer;
    using Utils;
    public struct JobInfo
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
        public NativeList<JobInfo> nextMove;

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

    public class DroneManager : MonoBehaviour
    {   

        public static JobHandle movementHandle = new JobHandle();
        public static DroneManager Instance { get; private set; }
        private static TimeKeeper.Chronos _time = TimeKeeper.Chronos.Get();
        private static SecureSortedSet<uint, IDataSource> Drones => SimManager.AllDrones;
        private static TransformAccessArray _transforms;
        private static NativeList<JobInfo> _jobInfoList;

        private void OnDisable()
        {
            movementHandle.Complete();
            if (_transforms.isCreated) _transforms.Dispose();
            if (_jobInfoList.IsCreated) _jobInfoList.Dispose();

        }

        private void Awake()
        {
            Instance = this;
        }

        void Initialise()
        {
            Drones.SetChanged += (obj) => OnDroneCountChange();
            _transforms = new TransformAccessArray(0);
            _jobInfoList = new NativeList<JobInfo>(_transforms.length, Allocator.Persistent);

            _time.Now();
        }

        IEnumerator Start()
        {
            yield return new WaitUntil(() => Time.unscaledDeltaTime < 1 / 60f);
            yield return new WaitForSeconds(5f);
            Initialise();
            while(true)
            {
                Operate();
                yield return null;
            }
        }

        private static void Operate()
        {
            movementHandle.Complete();
            if (_transforms.length == 0) return;

            int j = 0;
            foreach (Drone drone in Drones.Values)
            {
                _jobInfoList[j++] = new JobInfo
                {
                    speed = drone.MaxSpeed,
                    moveType = drone.Movement,
                    height = drone.Height,
                    waypoint = drone.Waypoint,
                    isWaiting = drone.IsWaiting ? 1 : 0
                };
            }

            MovementJob movementJob = new MovementJob
            {
                nextMove = _jobInfoList,
                deltaTime = _time.Timer()
            };

            _time.Now();

            movementHandle = movementJob.Schedule(_transforms);
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Starting movement.");

                foreach (Drone drone in Drones.Values)
                {
                    List<Vector3> wplist = new List<Vector3>();
                    float height = Random.value * 50 + 150;
                    Vector3 pos = drone.transform.position;
                    pos.y = height;
                    pos.x = Random.value * 150;
                    pos.z = Random.value * 150;
                    for (int i = 0; i < 50; i++)
                    {
                        wplist.Add(pos);
                    }
                    drone.NavigateWaypoints(wplist);
                    drone.AssignedHub.ExitingDrones.Enqueue(drone);
                }
            }
        }

        public static void OnDroneCountChange()
        {
            movementHandle.Complete();
            _transforms.Dispose();
            _transforms = new TransformAccessArray(0);
            _jobInfoList.Dispose();
            _jobInfoList = new NativeList<JobInfo>(_transforms.length, Allocator.Persistent);
            foreach (Drone drone in Drones.Values)
            {
                _transforms.Add(drone.transform);
                _jobInfoList.Add(new JobInfo
                {
                    speed = drone.MaxSpeed,
                    moveType = drone.Movement,
                    height = drone.Height,
                    waypoint = drone.Waypoint,
                    isWaiting = drone.IsWaiting ? 1 : 0
                });
            }

        }

    }


}
