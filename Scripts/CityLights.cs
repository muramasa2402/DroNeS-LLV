using UnityEngine;
using UnityEngine.PostProcessing;
using static Drones.Singletons;

namespace Drones
{
    using static Utils.Functions;
    public class CityLights : MonoBehaviour
    {
        const float pi = Mathf.PI;

        void Update()
        {
            float opp = Sun.transform.position.y;
            float adj = Sun.transform.position.x;
            float angle;

            if (adj >= 1e-6 || adj <= -1e-6) { angle = Mathf.Atan(opp / adj); }
            else { angle = pi / 2; }

            if (adj <= 0) { angle = pi + angle; }
            else if (opp < 0 && adj > 0) { angle = 2 * pi + angle; }


            angle = Mathf.Clamp(angle, 0f, 2 * pi);

            float level = 0.5f * (1 + Tanh(24 * (angle - pi + 1 / 36)));
            level -= 0.5f * (Tanh(24 * (angle - 1 / 36)) + Tanh(24 * (angle - 2 * pi - 1 / 36)));
            level = Mathf.Clamp(level, 0f, 1f);

            BloomModel.Settings newSettings = PostProcessing.bloom.settings;
            newSettings.bloom.intensity = 2.0f * level;

            PostProcessing.bloom.settings = newSettings;
        }
    }
}
