using System.Collections;
using UnityEngine;

namespace Drones.Utils
{
    public class TimeKeeper : MonoBehaviour
    {
        public TimeSpeed timeSpeed;
        IEnumerator Start()
        {
            var wait = new WaitForSeconds(1 / 60f);
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
                    case TimeSpeed.RealTime:
                        speed = 360.0f / (24 * 3600) / 60 / 0.25f;
                        break;
                    default:
                        speed = 1;
                        break;
                }
                transform.RotateAround(Vector3.zero, new Vector3(0, 0, 1), 0.25f * speed);
                yield return wait;
            }
        }

    }
}
