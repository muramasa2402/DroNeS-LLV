using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace Drones.Utils
{
    using Serializable;
    using Managers;
    public class TimeKeeper : MonoBehaviour
    {
        [SerializeField]
        private static TimeSpeed _TimeSpeed = TimeSpeed.Pause;
        public static TimeSpeed TimeSpeed
        {
            get => _TimeSpeed;

            set
            {
                if (SimManager.SimStatus != SimulationStatus.EditMode)
                {
                    _TimeSpeed = value;
                }
            }
        }
        private static Stopwatch StopWatch { get; } = Stopwatch.StartNew();
        public static long DeltaFrame() => StopWatch.ElapsedMilliseconds;

        private static float _Degree;

        private static int _Day;

        private static int Hour
        {
            get
            {
                return (int)(_Degree / 360 * 24);
            }
        }

        private static int Minute
        {
            get
            {
                return (int)((_Degree / 360 * 24 - Hour) * 60);
            }
        }

        private static float Seconds
        {
            get
            {
                return ((_Degree / 360 * 24 - Hour) * 60 - Minute) * 60;
            }
        }

        void Awake()
        {
            transform.position = Vector3.up * 200;
            transform.eulerAngles = new Vector3(90, -90, -90);
            transform.RotateAround(Vector3.zero, new Vector3(0, 0, 1), 180);
            transform.RotateAround(Vector3.zero, new Vector3(0, 0, 1), 135); // 9am
            _Degree = 135;
        }

        private void FixedUpdate()
        {
            float speed;
            switch (TimeSpeed)
            {
                case TimeSpeed.Slow:
                    speed = 0.5f * 360.0f / (24 * 3600);
                    break;
                case TimeSpeed.Fast:
                    speed = 4 * 360.0f / (24 * 3600);
                    break;
                case TimeSpeed.Ultra:
                    speed = 8 * 360.0f / (24 * 3600);
                    break;
                case TimeSpeed.Pause:
                    speed = 0;
                    break;
                default:
                    speed = 360.0f / (24 * 3600);
                    break;
            }

            float dTheta = Time.fixedDeltaTime * speed;

            transform.RotateAround(Vector3.zero, new Vector3(0, 0, 1), dTheta);

            _Degree += dTheta;

            if (_Degree > 360)
            {
                _Day++;
                _Degree %= 360;
            }

        }

        private void Update()
        {
            StopWatch.Restart();
        }

        public class Chronos
        {
            int day;
            int hr;
            int min;
            float sec;
            bool readOnly;

            public Chronos(int d, int h, int m, float s)
            {
                int acc;
                sec = s % 60;
                if (sec < 0) sec += 60;
                acc = Mathf.FloorToInt(s / 60);
                min = (m + acc) % 60;
                if (min < 0) min += 60;
                acc = Mathf.FloorToInt((m + acc) / 60f);
                hr = (h + acc) % 24;
                if (hr < 0) hr += 24;
                acc = Mathf.FloorToInt((h + acc) / 24f);
                day = d + acc;
                readOnly = false;
            }

            public Chronos(STime time)
            {
                sec = time.sec;
                min = time.min;
                hr = time.hr;
                day = time.day;
                readOnly = time.isReadOnly;
            }

            public Chronos SetReadOnly()
            {
                readOnly = true;
                return this;
            }

            public override string ToString()
            {
                return string.Format("Day {0}, {1}:{2}", day, hr.ToString("00"), min.ToString("00"));
            }

            public string ToStringLong()
            {
                return ToString() + ":" + sec.ToString("00.000");
            }

            public static Chronos Get()
            {
                return new Chronos(_Day, Hour, Minute, Seconds);
            }

            public Chronos Now()
            {
                if (!readOnly)
                {
                    day = _Day;
                    hr = Hour;
                    min = Minute;
                    sec = Seconds;
                }
                return this;
            }

            public float Timer()
            {
                return (_Day - day) * 24 * 3600 + (Hour - hr) * 3600 + (Minute - min) * 60 + (Seconds - sec);
            }

            public STime Serialize()
            {
                return new STime
                {
                    sec = this.sec,
                    min = this.min,
                    hr = this.hr,
                    day = this.day,
                    isReadOnly = readOnly
                };
            }

            public override bool Equals(object obj) => obj is Chronos && this == ((Chronos)obj);

            public override int GetHashCode() => base.GetHashCode();

            public static bool operator <(Chronos t1, Chronos t2)
            {
                if (t1.day < t2.day)
                {
                    return true;
                }

                if (t1.day == t2.day)
                {
                    if (t1.hr < t2.hr)
                    {
                        return true;
                    }
                    if (t1.hr == t2.hr)
                    {
                        if (t1.min < t2.min)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public static bool operator >(Chronos t1, Chronos t2)
            {
                if (t1.day > t2.day)
                {
                    return true;
                }

                if (t1.day == t2.day)
                {
                    if (t1.hr > t2.hr)
                    {
                        return true;
                    }
                    if (t1.hr == t2.hr)
                    {
                        if (t1.min > t2.min)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public static bool operator ==(Chronos t1, Chronos t2)
            {

                return !(t1 is null) && !(t2 is null) && t1.day == t2.day && t1.hr == t2.hr && t1.min == t2.min
                    || (t1 is null && t2 is null);
            }

            public static bool operator >=(Chronos t1, Chronos t2)
            {
                return t1 > t2 || t1 == t2;
            }

            public static bool operator <=(Chronos t1, Chronos t2)
            {
                return t1 < t2 || t1 == t2;
            }

            public static bool operator != (Chronos t1, Chronos t2)
            {
                return !(t1 == t2);
            }

            public static Chronos operator + (Chronos t1, float s)
            {
                return new Chronos(t1.day, t1.hr, t1.min, t1.sec + s);
            }

            public static Chronos operator - (Chronos t1, float s)
            {
                return new Chronos(t1.day, t1.hr, t1.min, t1.sec - s);
            }

            public static float operator - (Chronos t1, Chronos t2)
            {
                return (t1.day - t2.day) * 24 * 3600 + (t1.hr - t2.hr) * 3600 + (t1.min - t2.min) * 60 + (t1.sec - t1.sec);
            }


        }

    }


}
