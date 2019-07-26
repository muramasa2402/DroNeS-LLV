using System.Collections;
using Drones.Utils;
using UnityEngine;

namespace Drones.Objects
{
    public class HubCollisionController : MonoBehaviour
    {
        [SerializeField]
        private Hub _Owner;
        [SerializeField]
        private DeploymentPath _DronePath;

        int _intersects;

        private void Awake()
        {
            if (_Owner == null) _Owner = GetComponent<Hub>();
            if (_DronePath == null) _DronePath = transform.GetComponentInChildren<DeploymentPath>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Hub") ||
                other.gameObject.layer == LayerMask.NameToLayer("NoFlyZone"))
            {
                _intersects++;
                //_DronePath.Stop();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if ((other.gameObject.layer == LayerMask.NameToLayer("Hub") ||
                other.gameObject.layer == LayerMask.NameToLayer("NoFlyZone")) && _intersects > 0)
            {
                _intersects--;
                //if (_intersects == 0) _DronePath.StartDeploy();
            }
        }

        public IEnumerator Repulsion(Collider other)
        {
            _DronePath.Stop();
            yield return null;
            Vector3 acc;
            Vector3 vel = Vector3.zero;
            float dt = 0;
            float dist;
            do
            {
                transform.position += vel * dt;
                dist = Vector3.Distance(transform.position, other.transform.position);
                float k = 1e8f / Mathf.Pow(dist, 2);
                if (other.CompareTag("NoFlyZone")) k *= 100;
                k = Mathf.Clamp(k, 0, 1e5f);
                acc = k * (transform.position - other.transform.position).normalized;
                acc.y = 0;
                dt = Time.deltaTime;
                vel = acc * dt;
                yield return null;
            } while (_intersects > 0);
            _DronePath.StartDeploy();
        }

        public IEnumerator Reposition(Vector3 inDirection)
        {
            _DronePath.Stop();
            Vector3.Normalize(inDirection);
            while (!_DronePath.IsClear) // Building
            {
                transform.position += 5 * _DronePath.Direction + inDirection;
                yield return null;
            }
            _DronePath.StartDeploy();
            yield break;
        }
    }
}
