using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Drones
{
    using static Singletons;
    public class Compass : MonoBehaviour
    {
        public GameObject target;
        Vector3 startPosition;
        float movementPerDegree;
        RectTransform rect;

        void Start()
        {
            rect = (RectTransform)transform;
            startPosition = transform.localPosition;
            Vector2 tmp = rect.sizeDelta;
            tmp.x = 2048 / 52 * tmp.y;
            rect.sizeDelta = tmp;
            movementPerDegree = rect.sizeDelta.x / 720;
        }

        IEnumerator GetDirection()
        {
            var wait = new WaitForSeconds(1 / 45f);
            while (true)
            {
                Vector3 localForward = Vector3.Cross(CameraTransform.right, Vector3.up);
                float dir = -Vector3.Dot(localForward, Vector3.right);
                transform.localPosition = startPosition + Vector3.Angle(localForward, Vector3.forward) * Mathf.Sign(dir) * movementPerDegree * Vector3.right;
                yield return wait;
            }

        }
    }
}
