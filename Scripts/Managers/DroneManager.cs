using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using System.Collections;

namespace Drones
{
    using Drones.DataStreamer;
    using Drones.Utils.Jobs;
    using Utils;

    public class DroneManager : MonoBehaviour
    {   
        public static JobHandle movementJobHandle = new JobHandle();
        public static JobHandle energyJobHandle = new JobHandle();
        private static TimeKeeper.Chronos _time = TimeKeeper.Chronos.Get();
        private static SecureSortedSet<uint, IDataSource> Drones => SimManager.AllDrones;
        private static TransformAccessArray _transforms;
        private static NativeArray<MovementInfo> _jobInfoList;
        private static NativeArray<EnergyInfo> _energyInfoList;

        private void OnDisable()
        {
            movementJobHandle.Complete();
            energyJobHandle.Complete();
            if (_transforms.isCreated) _transforms.Dispose();
            if (_jobInfoList.IsCreated) _jobInfoList.Dispose();
            if (_energyInfoList.IsCreated) _energyInfoList.Dispose();
        }

        private void Awake()
        {
            Drones.SetChanged += (obj) => OnDroneCountChange();
        }

        private static void Initialise()
        {
            _transforms = new TransformAccessArray(0);
            _jobInfoList = new NativeArray<MovementInfo>(_transforms.length, Allocator.Persistent);
            _energyInfoList = new NativeArray<EnergyInfo>(_transforms.length, Allocator.Persistent);
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
                if (_transforms.length == 0) yield return null;
                int j = 0;

                foreach (Drone drone in Drones.Values)
                {
                    var dE = _energyInfoList[j].energy;
                    drone.TotalEnergy += dE;
                    drone.AssignedHub.UpdateEnergy(dE);
                    drone.AssignedBattery.DischargeBattery(dE);
                    SimManager.UpdateEnergy(dE);
                    tmpJ = _jobInfoList[j];
                    tmpJ.speed = drone.MaxSpeed;
                    tmpJ.moveType = drone.Movement;
                    tmpJ.height = drone.TargetAltitude;
                    tmpJ.waypoint = drone.Waypoint;
                    tmpJ.isWaiting = drone.IsWaiting ? 1 : 0;
                    _jobInfoList[j] = tmpJ;

                    tmpE = _energyInfoList[j];
                    tmpE.speed = drone.MaxSpeed;
                    tmpE.moveType = drone.Movement;
                    tmpE.pkgWgt = (drone.AssignedJob == null) ? 0 : drone.AssignedJob.PackageWeight;
                    _energyInfoList[j++] = tmpE;
                }

                _movementJob.nextMove = _jobInfoList;
                _movementJob.deltaTime = _time.Timer();

                _energyJob.energies = _energyInfoList;
                _energyJob.deltaTime = _time.Timer();
                _time.Now();

                movementJobHandle = _movementJob.Schedule(_transforms);
                energyJobHandle = _energyJob.Schedule(_transforms.length, 1);

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("Starting movement.");

                    foreach (Drone drone in Drones.Values)
                    {
                        List<Vector3> wplist = new List<Vector3>();
                        float height = Random.value * 50 + 150;
                        Vector3 pos = drone.transform.position;
                        pos.y = height;
                        for (int i = 0; i < 50; i++)
                        {
                            pos.x = Random.value * 150;
                            pos.z = Random.value * 300 + 2000;
                            wplist.Add(pos);
                        }
                        drone.NavigateWaypoints(wplist);
                        if (drone.InHub)
                        {
                            drone.AssignedHub.ExitingDrones.Enqueue(drone);
                        }
                    }
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
            _transforms.Dispose();
            _transforms = new TransformAccessArray(0);
            foreach (Drone drone in Drones.Values)
            {
                _transforms.Add(drone.transform);
            }
            _jobInfoList.Dispose();
            _jobInfoList = new NativeArray<MovementInfo>(_transforms.length, Allocator.Persistent);
            _energyInfoList.Dispose();
            _energyInfoList = new NativeArray<EnergyInfo>(_transforms.length, Allocator.Persistent);
            int j = 0;
            foreach (Drone drone in Drones.Values)
            {
                _jobInfoList[j] = new MovementInfo
                {
                    speed = drone.MaxSpeed,
                    moveType = drone.Movement,
                    height = drone.TargetAltitude,
                    waypoint = drone.Waypoint,
                    isWaiting = drone.IsWaiting ? 1 : 0
                };
                _energyInfoList[j++] = new EnergyInfo
                {
                    speed = drone.MaxSpeed,
                    moveType = drone.Movement,
                    pkgWgt = (drone.AssignedJob == null) ? 0 : drone.AssignedJob.PackageWeight
                };
            }

        }

    }


}
