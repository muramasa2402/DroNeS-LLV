using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using System.Collections;

namespace Drones.Managers
{
    using DataStreamer;
    using Utils.Jobs;
    using Utils;

    public class DroneManager : MonoBehaviour
    {   
        public static JobHandle movementJobHandle = new JobHandle();
        public static JobHandle energyJobHandle = new JobHandle();
        private static readonly TimeKeeper.Chronos _time = TimeKeeper.Chronos.Get();
        private static SecureSortedSet<uint, IDataSource> Drones => SimManager.AllDrones;
        private static TransformAccessArray _Transforms;
        private static NativeArray<MovementInfo> _JobInfoArray;
        private static NativeArray<EnergyInfo> _EnergyInfoArray;
        private static NativeArray<Vector3> _PreviousPositions;

        private void OnDisable()
        {
            movementJobHandle.Complete();
            energyJobHandle.Complete();
            if (_Transforms.isCreated) _Transforms.Dispose();
            if (_JobInfoArray.IsCreated) _JobInfoArray.Dispose();
            if (_EnergyInfoArray.IsCreated) _EnergyInfoArray.Dispose();
            if (_PreviousPositions.IsCreated) _PreviousPositions.Dispose();
        }

        private void Awake()
        {
            Drones.SetChanged += (obj) => OnDroneCountChange();
        }

        private static void Initialise()
        {
            _Transforms = new TransformAccessArray(0);
            _JobInfoArray = new NativeArray<MovementInfo>(_Transforms.length, Allocator.Persistent);
            _EnergyInfoArray = new NativeArray<EnergyInfo>(_Transforms.length, Allocator.Persistent);
            _PreviousPositions = new NativeArray<Vector3>(_Transforms.length, Allocator.Persistent);
            _time.Now();
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => Time.unscaledDeltaTime < 1 / 30f);
            yield return new WaitForSeconds(2f);
            Initialise();
            StartCoroutine(Operate());
        }

        private static IEnumerator Operate()
        {
            MovementJob _movementJob = new MovementJob();
            EnergyJob _energyJob = new EnergyJob();
            MovementInfo tmpJ;
            EnergyInfo tmpE;
            while (true)
            {
                if (_Transforms.length == 0) yield return null;
                int j = 0;

                foreach (Drone drone in Drones.Values)
                {
                    var dE = _EnergyInfoArray[j].energy;
                    drone.TotalEnergy += dE;
                    drone.AssignedHub.UpdateEnergy(dE);
                    drone.AssignedBattery.DischargeBattery(dE);
                    SimManager.UpdateEnergy(dE);

                    _PreviousPositions[j] = drone.PreviousPosition;
                    drone.PreviousPosition = drone.transform.position;

                    tmpJ = _JobInfoArray[j];
                    tmpJ.speed = drone.MaxSpeed;
                    tmpJ.moveType = drone.Movement;
                    tmpJ.height = drone.TargetAltitude;
                    tmpJ.waypoint = drone.Waypoint;
                    tmpJ.isWaiting = drone.IsWaiting ? 1 : 0;
                    _JobInfoArray[j] = tmpJ;

                    tmpE = _EnergyInfoArray[j];
                    tmpE.speed = drone.MaxSpeed;
                    tmpE.moveType = drone.Movement;
                    tmpE.pkgWgt = (drone.AssignedJob == null) ? 0 : drone.AssignedJob.PackageWeight;
                    _EnergyInfoArray[j++] = tmpE;
                }

                _movementJob.nextMove = _JobInfoArray;
                _movementJob.rt_dt = _PreviousPositions;
                _movementJob.deltaTime = _time.Timer();

                _energyJob.energies = _EnergyInfoArray;
                _energyJob.deltaTime = _time.Timer();
                _time.Now();

                movementJobHandle = _movementJob.Schedule(_Transforms);
                energyJobHandle = _energyJob.Schedule(_Transforms.length, 1);

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("Starting movement.");

                    //foreach (Drone drone in Drones.Values)
                    //{
                    //    List<Vector3> wplist = new List<Vector3>();
                    //    float height = Random.value * 50 + 150;
                    //    Vector3 pos = drone.transform.position;
                    //    pos.y = height;
                    //    for (int i = 0; i < 50; i++)
                    //    {
                    //        pos.x = Random.value * 150;
                    //        pos.z = Random.value * 300 + 2000;
                    //        wplist.Add(pos);
                    //    }
                    //    drone.NavigateWaypoints(wplist);
                    //    if (drone.InHub)
                    //    {
                    //        drone.AssignedHub.ExitingDrones.Enqueue(drone);
                    //    }
                    //}
                }

                yield return null;
                energyJobHandle.Complete();
                movementJobHandle.Complete();
            }
        }

        public static void OnDroneCountChange()
        {
            movementJobHandle.Complete();
            energyJobHandle.Complete();
            _Transforms.Dispose();
            _Transforms = new TransformAccessArray(0);
            foreach (Drone drone in Drones.Values)
            {
                _Transforms.Add(drone.transform);
            }
            _JobInfoArray.Dispose();
            _JobInfoArray = new NativeArray<MovementInfo>(_Transforms.length, Allocator.Persistent);
            _EnergyInfoArray.Dispose();
            _EnergyInfoArray = new NativeArray<EnergyInfo>(_Transforms.length, Allocator.Persistent);
            _PreviousPositions.Dispose();
            _PreviousPositions = new NativeArray<Vector3>(_Transforms.length, Allocator.Persistent);
            int j = 0;
            foreach (Drone drone in Drones.Values)
            {
                _PreviousPositions[j] = drone.PreviousPosition;

                _JobInfoArray[j] = new MovementInfo
                {
                    speed = drone.MaxSpeed,
                    moveType = drone.Movement,
                    height = drone.TargetAltitude,
                    waypoint = drone.Waypoint,
                    isWaiting = drone.IsWaiting ? 1 : 0
                };
                _EnergyInfoArray[j++] = new EnergyInfo
                {
                    speed = drone.MaxSpeed,
                    moveType = drone.Movement,
                    pkgWgt = (drone.AssignedJob == null) ? 0 : drone.AssignedJob.PackageWeight
                };
            }

        }

    }


}
