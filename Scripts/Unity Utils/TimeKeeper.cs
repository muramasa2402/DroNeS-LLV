using System.Collections;
using UnityEngine;

namespace Drones.Utils
{
    public class TimeKeeper : MonoBehaviour
    {
        [SerializeField]
        private TimeSpeed _TimeSpeed;
        public TimeSpeed TimeSpeed
        {
            get
            {
                return _TimeSpeed;
            }
            set
            {
                _TimeSpeed = value;
            }
        }
        private static float _Degree;

        private static int _Day;

        private static int Hour
        {
            get
            {
                return (int)(_Degree % 360 % 24);
            }
        }

        private static int Minute
        {
            get
            {
                return (int)(_Degree % 360 % 24 % 60);
            }
        }

        private static float Seconds
        {
            get
            {
                return _Degree % 360 % 24 % 3600;
            }
        }

        IEnumerator Start()
        {
            var wait = new WaitForSeconds(1 / 30f);
            transform.RotateAround(Vector3.zero, new Vector3(0, 0, 1), 180);
            StartCoroutine(NightLights.Bloom());
            while (true)
            {
                float speed;
                switch (TimeSpeed)
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
                _Degree += 0.25f * speed;

                if (_Degree > 360)
                {
                    _Day++;
                    _Degree %= 360;
                }

                transform.RotateAround(Vector3.zero, new Vector3(0, 0, 1), 0.25f * speed);
                yield return wait;
            }
        }

        public struct Chronos
        {
            int day;
            int hr;
            int min;
            float sec;
            public Chronos(int d, int h, int m, float s)
            {
                day = d;
                hr = h;
                min = m;
                sec = s;
            }

            public override string ToString()
            {
                return string.Format("Day {0}, {1}:{2}", day, hr, min);
            }

            public string ToStringLong()
            {
                return ToString() + sec.ToString("0.000");
            }

            public static Chronos Get()
            {
                return new Chronos(_Day, Hour, Minute, Seconds);
            }

            public Chronos Now()
            {
                day = _Day;
                hr = Hour;
                min = Minute;
                sec = Seconds;
                return this;
            }

            public float Timer()
            {
                return (_Day - day) * 24 * 3600 + (Hour - hr) * 3600 + (Minute - min) * 60 + (Seconds - sec);
            }

        }

    }


}
