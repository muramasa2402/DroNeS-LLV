using System.Collections;
using System.Collections.Generic;
using Drones.Utils.Extensions;
using UnityEngine;

namespace Drones.StartScreen
{
    public class DroneAnimator : MonoBehaviour
    {
        readonly WaitForSecondsRealtime _spawnFreq = new WaitForSecondsRealtime(1);
        GameObject _template;
        readonly Stack<RectTransform> idles = new Stack<RectTransform>();
        readonly List<RectTransform> goingLeft = new List<RectTransform>();
        readonly List<RectTransform> goingRight = new List<RectTransform>();
        readonly List<float> speedLeft = new List<float>();
        readonly List<float> speedRight = new List<float>();
        const int max = 20;
        const float maxHeight = 580;
        const float sides = 1000;

        private void Awake()
        {
            _template = Resources.Load("Prefabs/StartScreen/Drone", typeof(GameObject)) as GameObject;
            for (int i = 0; i < max; i++)
            {
                idles.Push(Instantiate(_template).transform.ToRect());
                idles.Peek().SetParent(transform);
                idles.Peek().gameObject.SetActive(false);
            }

        }

        private void OnEnable()
        {
            StartCoroutine(Spawner());
            StartCoroutine(Animate());
        }

        private void OnDisable()
        {
            StopCoroutine(Spawner());
            StopCoroutine(Animate());
        }

        IEnumerator Animate()
        {
            while (true)
            {
                for (int i = goingLeft.Count - 1; i >= 0; i--)
                {
                    var drone = goingLeft[i];
                    drone.localPosition -= Vector3.right * speedLeft[i];
                    if (drone.localPosition.x <= -sides)
                    {
                        drone.gameObject.SetActive(false);
                        speedLeft.RemoveAt(i);
                        goingLeft.RemoveAt(i);
                        idles.Push(drone);
                    }
                }
                for (int i = goingRight.Count - 1; i >= 0; i--)
                {
                    var drone = goingRight[i];
                    drone.localPosition += Vector3.right * speedRight[i];
                    if (drone.localPosition.x >= sides)
                    {
                        drone.gameObject.SetActive(false);
                        speedRight.RemoveAt(i);
                        goingRight.RemoveAt(i);
                        idles.Push(drone);
                    }
                }
                yield return null;
            }

        }

        IEnumerator Spawner()
        {
            bool goLeft = Random.value < 0.5f;
            while (true)
            {
                if (idles.Count > 0)
                {
                    var d = idles.Pop();
                    d.gameObject.SetActive(true);
                    d.localPosition = new Vector3(goLeft ? sides : -sides, Random.value * maxHeight, 0);

                    if (goLeft)
                    {
                        goingLeft.Add(d);
                        speedLeft.Add(2 + Random.value * 3);
                    }
                    else
                    {
                        goingRight.Add(d);
                        speedRight.Add(2 + Random.value * 3);
                    }

                    goLeft = Random.value < 0.5f;
                }
                yield return _spawnFreq;

            }

        }


    }

}
