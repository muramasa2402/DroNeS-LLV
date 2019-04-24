using System;
using Drones.Serializable;

namespace Drones.Utils
{
    public class CostFunction
    {
        public CostFunction(SCostFunction cf)
        {
            CompleteIn = cf.valid_time;
            Reward = Math.Abs(cf.reward);
            Penalty = Math.Abs(cf.penalty);
        }

        public float CompleteIn { get; private set; }
        public float Reward { get; private set; }
        public float Penalty { get; private set; }

        public float GetPaid(TimeKeeper.Chronos complete, TimeKeeper.Chronos deadline)
        {
            float dt = complete - deadline;
            return Reward * Step(dt, CompleteIn);
        }

        public SCostFunction Serialize()
        {
            return new SCostFunction
            {
                valid_time = CompleteIn,
                reward = Reward,
                penalty = Penalty
            };
        }

        public static float Tanh(float x, float a, float b, float c) => (float)(a * Math.Tanh(b * x + c));

        public static float Poly(float x, float a, int n) => (float)(a * Math.Pow(x, n));

        public static float Step(float x, float n, float yU = 1, float yD = -1) => (x <= n) ? yU : yD;
    }
}
