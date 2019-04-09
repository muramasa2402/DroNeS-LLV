using System.Collections;
using UnityEngine;
using UnityEngine.PostProcessing;
using static Drones.Singletons;

namespace Drones
{
    using static Utils.StaticFunc;
    public static class NightLights
    {
        const float pi = Mathf.PI;
        private static PostProcessingProfile _PostProcessing;
        public static PostProcessingProfile PostProcessing
        {
            get
            {
                if (_PostProcessing == null)
                {
                    _PostProcessing = Resources.Load("PostProcessing/CityLights") as PostProcessingProfile;
                }
                return _PostProcessing;
            }
        }

        public static IEnumerator Bloom()
        {
            var wait = new WaitForSeconds(1 / 30f);
            while (true)
            {
                var angle = Mathf.Atan2(Sun.transform.position.y, Sun.transform.position.x);
                if (angle < 0) { angle += 2 * pi; }

                float level = 0.5f * (1 + Tanh(24 * (angle - pi + 1 / 36)));
                level -= 0.5f * (Tanh(24 * (angle - 1 / 36)) + Tanh(24 * (angle - 2 * pi - 1 / 36)));
                level = Mathf.Clamp(level, 0f, 1f);

                BloomModel.Settings newSettings = PostProcessing.bloom.settings;
                newSettings.bloom.intensity = 2.0f * level;
                PostProcessing.bloom.settings = newSettings;
                yield return wait;
            }

        }
    }
}
