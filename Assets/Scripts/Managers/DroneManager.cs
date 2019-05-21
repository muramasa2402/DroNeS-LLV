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
            yield return new WaitUntil(() => SimManager.Instance != null && SimManager.Instance.Initialized);
            Drones.SetChanged += (obj) => OnDroneCountChange();
            Initialise();
            StartCoroutine(Operate());
        }

        private static IEnumerator Operate()
        {
            MovementJob _movementJob = new MovementJob();
            EnergyJob _energyJob = new EnergyJob();
            while (true)
            {
                if (_Transforms.length == 0) yield return null;
                int j = 0;

                foreach (Drone drone in Drones.Values)
                {
                    var dE = _EnergyInfoArray[j].energy;
                    drone.UpdateEnergy(dE);
                    drone.GetHub().UpdateEnergy(dE);
                    drone.GetBattery().DischargeBattery(dE);
                    SimManager.UpdateEnergy(dE);

                    _PreviousPositions[j] = drone.PreviousPosition;
                    drone.PreviousPosition = drone.transform.position;
                   
                    _JobInfoArray[j] = drone.GetMovementInfo(_JobInfoArray[j]);
                    _EnergyInfoArray[j] = drone.GetEnergyInfo(_EnergyInfoArray[j]);
                    j++;
                }

                _movementJob.nextMove = _JobInfoArray;
                _movementJob.rt_dt = _PreviousPositions;
                _movementJob.deltaTime = _time.Timer();

                _energyJob.energies = _EnergyInfoArray;
                _energyJob.deltaTime = _time.Timer();
                _time.Now();

                movementJobHandle = _movementJob.Schedule(_Transforms);
                energyJobHandle = _energyJob.Schedule(_Transforms.length, 1);

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

                _JobInfoArray[j] = new MovementInfo();
                _JobInfoArray[j] = drone.GetMovementInfo(_JobInfoArray[j]);
                _EnergyInfoArray[j] = new EnergyInfo();
                _EnergyInfoArray[j] = drone.GetEnergyInfo(_EnergyInfoArray[j]);
                j++;
            }

        }

    }


}
