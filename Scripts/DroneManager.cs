using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using System.Collections;

namespace Drones
{
    using Utils;
    public struct JobInfo
    {
        public float speed;
        public DroneMovement moveType;
        public Vector3 direction;
        public float height;
        public Vector3 waypoint;
    }

    public struct MovementJob : IJobParallelForTransform
    {
        public float deltaTime;
        [ReadOnly]
        public NativeList<JobInfo> nextMove;

        public void Execute(int k, TransformAccess transform)
        {
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
        private static List<Drone> _drones;
        private static TransformAccessArray _transforms;
        [SerializeField]
        private readonly int _DroneNumber = 700;

        private static NativeList<JobInfo> _jobInfoList;

        private void OnDisable()
        {
            movementHandle.Complete();
            _transforms.Dispose();
            _jobInfoList.Dispose();
        }

        private void Awake()
        {
            Instance = this;
        }

        void Initialise()
        {
            //_drones = SimManager.AllDrones.ToList<Drone>();
            _drones = new List<Drone>();

            _transforms = new TransformAccessArray(0);
            for (int i = 0; i < _DroneNumber; i++)
            {
                _drones.Add((Drone)ObjectPool.Get(typeof(Drone)));
                _drones[i].transform.position = new Vector3(i * 2f, 150, i * 2f);
                _transforms.Add(_drones[i].transform);
            }

            _jobInfoList = new NativeList<JobInfo>(_transforms.length, Allocator.Persistent);

            for (int j = 0; j < _drones.Count; j++)
            {
                _jobInfoList.Add(new JobInfo
                {
                    speed = _drones[j].MaxSpeed,
                    moveType = _drones[j].Movement,
                    direction = _drones[j].Direction
                });
            }
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

            for (int j = 0; j < _drones.Count; j++)
            {
                _jobInfoList[j] = new JobInfo
                {
                    speed = _drones[j].MaxSpeed,
                    moveType = _drones[j].Movement,
                    direction = _drones[j].Direction,
                    height = _drones[j].Height,
                    waypoint = _drones[j].Waypoint
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

                for (int n = 0; n < _drones.Count; n++)
                {
                    List<Vector3> wplist = new List<Vector3>();
                    float height = Random.value * 20 + 200;
                    for (int i = 0; i < 25; i++)
                    {
                        wplist.Add(new Vector3(Random.value * 20 - 10, height, Random.value * 20 - 10));
                    }
                    _drones[n].NavigateWaypoints(wplist);
                }
            }
        }

        public static void DroneCountChange(Drone drone)
        {
            _drones.Remove(drone);

            _transforms.Dispose();
            _transforms = new TransformAccessArray(0);
            for (int i = 0; i < _drones.Count; i++)
            {
                _transforms.Add(_drones[i].transform);
            }
        }

    }


}
