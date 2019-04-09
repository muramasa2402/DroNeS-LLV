using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine;

namespace Drones.UI
{
    using static Singletons;
    public class EagleEyeCamera : MonoBehaviour
    {
        // Start is called before the first frame update
        void OnEnable()
        {
            StartCoroutine(MaintainViewDirection());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public IEnumerator MaintainViewDirection()
        {
            while(true)
            {
                Vector3 v = transform.parent.position;
                v.y = 2 * CameraControl.Controller.Ceiling;
                transform.position = v;
                transform.LookAt(transform.parent, Vector3.Cross(CameraTransform.forward, CameraTransform.right));
                yield return null;
            }

        }

    }
}
