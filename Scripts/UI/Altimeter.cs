using Drones.Utils;
using UnityEngine;

public class Altimeter : MonoBehaviour
{
    public GameObject target;
    Vector3 startPosition;
    float heightScale;
    float meterScale;

    void Awake()
    {
        startPosition = transform.localPosition;
        heightScale = Constants.realWorldTileSize / Constants.unityTileSize; // Map to Real
        // 2232/2480/600 is based on image size
        // 2232 is scale height, 2480 is image height. 600 scale limit (600 m)
        meterScale = 2232f / 2480f / 600f * transform.parent.GetComponent<RectTransform>().sizeDelta.y;
        if (target == null) { target = GameObject.Find("RTSCamera"); }
    }

    // Update is called once per frame
    void Update()
    {
        float value = Mathf.Clamp(target.transform.position.y * heightScale, 0, 600f);
        transform.localPosition = startPosition + value * meterScale * Vector3.up;
    }
}
