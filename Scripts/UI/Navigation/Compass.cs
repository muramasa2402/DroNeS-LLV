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
        RectTransform rectParent;

        void Start()
        {
            rect = (RectTransform)transform;
            rectParent = (RectTransform)transform.parent;
            Image image = GetComponent<Image>();
            startPosition = transform.localPosition;
            Vector2 tmp = rect.sizeDelta;
            tmp.x = 2048 / 52 * tmp.y;
            rect.sizeDelta = tmp;
            movementPerDegree = rect.sizeDelta.x / 720;
        }

        void Update()
        {
            Vector3 localForward = Vector3.Cross(CamTrans.right, Vector3.up);
            Vector3 perp = Vector3.Cross(Vector3.forward, localForward);
            float dir = -Vector3.Dot(localForward, Vector3.right);
            transform.localPosition = startPosition + Vector3.Angle(localForward, Vector3.forward) * Mathf.Sign(dir) * movementPerDegree * Vector3.right;
        }
    }
}
