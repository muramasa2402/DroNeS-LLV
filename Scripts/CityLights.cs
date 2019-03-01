using UnityEngine;
using UnityEngine.PostProcessing;


public class CityLights : MonoBehaviour
{
    public GameObject sun;
    public PostProcessingProfile postProcess;
    float pi = Mathf.PI;

    private void Awake() { sun = GameObject.Find("Sun"); }

    void Update()
    {
        float opp = sun.transform.position.y;
        float adj = sun.transform.position.x;
        float angle;

        if (adj >= 1e-6 || adj <= -1e-6) { angle = Mathf.Atan(opp / adj); }
        else { angle = pi / 2; }

        if (adj <= 0) { angle = pi + angle; }
        else if (opp < 0 && adj > 0) { angle = 2 * pi + angle; }

        angle = Mathf.Clamp(angle, 0f, 2*pi);

        float level = 0.5f * (1 + Tanh(24 * (angle - pi + 1 / 36)));
        level -= 0.5f *(Tanh(24 * (angle - 1 / 36)) - Tanh(24 * (angle - 2 * pi - 1 / 36)));
        level = Mathf.Clamp(level, 0f, 1f);

        BloomModel.Settings newSettings = postProcess.bloom.settings;
        newSettings.bloom.intensity = 2.0f * level;

        postProcess.bloom.settings = newSettings;
    }

    private float Tanh(float x) {
        // Truncation
        if (x >= 3.65f) return 1;
        if (x < -3.65f) return -1;

        return (Mathf.Exp(x) - Mathf.Exp(-x)) / (Mathf.Exp(x) + Mathf.Exp(-x));
    }
}
