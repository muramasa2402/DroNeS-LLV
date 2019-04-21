using System.Collections;
using UnityEngine;

namespace Drones.Utils
{
    public class AudioSensor : MonoBehaviour
    {
        private bool _Active;

        private Drone _Drone;

        public float TotalTime { get; private set; }

        private int _inRadius;

        private TimeKeeper.Chronos _Time;

        private readonly WaitForSeconds _Wait = new WaitForSeconds(1 / 10f);

        public Drone AssignedDrone
        {
            get
            {
                if (_Drone == null)
                {
                    _Drone = transform.parent.GetComponent<Drone>();
                }
                return _Drone;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == 11 || other.gameObject.layer == 12 || other.gameObject.layer == 13)
            {
                _inRadius++;
                if (!_Active)
                {
                    _Active = true;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == 11 || other.gameObject.layer == 12 || other.gameObject.layer == 13)
            {
                _inRadius--;
                if (_inRadius <= 0)
                {
                    _inRadius = 0;
                    _Active = false;
                }
            }
        }

        private IEnumerator StartTimer()
        {
            _Active = true;
            while (_Active)
            {
                _Time = _Time.Now();
                yield return _Wait;
                TotalTime += _Time.Timer();
            }
            yield break;

        }

        public void SetSensorRadius(float radius)
        {
            transform.localScale = transform.worldToLocalMatrix * Vector3.one * radius;
        }
    }
}

