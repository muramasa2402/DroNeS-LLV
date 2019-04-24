using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Drones.Utils.Jobs
{
    public struct EnergyInfo
    {
        public float package;
        public float energy;
        public float speed;
        public DroneMovement moveType;
    }

    public struct EnergyJob : IJobParallelFor
    {
        public const float mass = Drone.DroneAndBatteryMass;
        public const float Cd = 1.6f;
        public const float g = 9.81f;
        public const float R_d = 0.5f;
        public const float rho = 1.225f;
        public const float A = Mathf.PI * R_d * R_d;
        public const float Prop_D = 0.25f;
        public const float n_Prop = 4;
        public const float eff = 0.5f;
        public const float dragpower = 1000f;
        public float deltaTime;
        public NativeArray<EnergyInfo> energies;

        public void Execute(int i)
        {
            var tmp = energies[i];
            if (energies[i].moveType == DroneMovement.Idle)
            {
                tmp.energy = 0;
                energies[i] = tmp;
                return;
            }

            float m = mass + energies[i].package;
            float w = m * g;
            float power = 8 * Mathf.Sqrt(Mathf.Pow(w / 8, 3) * 2 / Mathf.PI / Mathf.Pow(Prop_D, 2) / rho) / eff;
            if (energies[i].moveType != DroneMovement.Hover)
            {
                if (energies[i].moveType == DroneMovement.Ascend)
                    power *= 2.5f;
                if (energies[i].moveType != DroneMovement.Descend)
                    power += dragpower;
                else
                    power -= dragpower;
            }

            tmp.energy = power * deltaTime;
            energies[i] = tmp;
        }
    }
}
