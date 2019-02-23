using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Altimeter : MonoBehaviour
{
    public GameObject target;
    Vector3 startPosition;
    float heightScale;
    float meterScale;
    // Start is called before the first frame update
    void Awake()
    {
        startPosition = transform.localPosition;
        heightScale = 463.2582f / 150f; // Map to Real
        meterScale = 2232f / 2480f / 600f * transform.parent.GetComponent<RectTransform>().sizeDelta.y;
        if (target == null) { target = GameObject.Find("RTSCamera"); }
    }

    // Update is called once per frame
    void Update()
    {
        float value = Mathf.Clamp(target.transform.position.y * heightScale, 0, 600f);
        transform.localPosition = startPosition + new Vector3(0, value * meterScale, 0);

    }
}
