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
        }

        public float CompleteIn { get; private set; }
        public float Reward { get; private set; }

        public float GetPaid(TimeKeeper.Chronos complete, TimeKeeper.Chronos deadline)
        {
            float dt = complete - deadline;
            return Reward * Step(dt, 0);
        }

        public SCostFunction Serialize()
        {
            return new SCostFunction
            {
                valid_time = CompleteIn,
                reward = Reward
            };
        }

        public static float Tanh(float x, float a = 1, float b = 1, float c = 0) => (float)(a * Math.Tanh(b * x + c));

        public static float Poly(float x, float a = 1, int n = 1) => (float)(a * Math.Pow(x, n));

        public static float Step(float x, float n, float yU = 1, float yD = -1) => (x <= n) ? yU : yD;

        public static float Exp(float x, float a = 1, float b = 1) => (float)(a * Math.Exp(b * x));
    }
}
