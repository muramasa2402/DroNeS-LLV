using UnityEngine;

namespace Drones
{
    public class SunMotion : MonoBehaviour
    {
        public float DaySpeed { get; set; } = 1.0f;

        void Update()
        {
            transform.RotateAround(Vector3.zero, new Vector3(0, 0, 1), 0.25f * DaySpeed);
        }
    }
}
