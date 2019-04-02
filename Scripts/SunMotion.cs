using System.Collections;
using UnityEngine;

namespace Drones
{
    public class SunMotion : MonoBehaviour
    {
        public enum TimeSpeed { Normal, Fast, Slow, Ultra, Realtime }

        public TimeSpeed timeSpeed;
        IEnumerator Start()
        {
            while (true)
            {
                float speed;
                switch (timeSpeed)
                {
                    case TimeSpeed.Slow:
                        speed = 0.5f;
                        break;
                    case TimeSpeed.Fast:
                        speed = 2;
                        break;
                    case TimeSpeed.Ultra:
                        speed = 4;
                        break;
                    case TimeSpeed.Realtime:
                        speed = 360.0f / (24 * 3600) / 60 / 0.25f;
                        break;
                    default:
                        speed = 1;
                        break;
                }
                transform.RotateAround(Vector3.zero, new Vector3(0, 0, 1), 0.25f * speed);
                yield return new WaitForSeconds(1 / 60f);
            }
        }

    }
}
