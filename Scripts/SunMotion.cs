using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunMotion : MonoBehaviour
{
    public float daySpeed = 1.0f;
    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(Vector3.zero, new Vector3(0, 0, 1), 0.25f * daySpeed);
    }
}
