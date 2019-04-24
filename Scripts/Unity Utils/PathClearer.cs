using UnityEngine;

namespace Drones.Utils
{
    public class PathClearer : MonoBehaviour
    {
        private Hub _hub;
        private Hub Owner
        {
            get
            {
                if (_hub == null)
                {
                    _hub = transform.parent.GetComponent<Hub>();
                }
                return _hub;
            }
        }
        private int _Intersects;
        public Vector3 Direction { get; private set; }

        public bool IsClear => _Intersects == 0;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == 12)
            {
                Debug.Log("ENTERED");
                _Intersects++;
                Vector3 v;
                if (Vector3.Distance(transform.position, other.transform.position) < 1e-3f)
                {
                    v = Random.insideUnitSphere;
                    v.y = 0;
                    transform.parent.position += v;
                }
                v = (transform.position - other.transform.position).normalized;
                v.y = 0;
                Direction += v;
                StartCoroutine(Owner.Reposition(Vector3.zero));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == 12 && _Intersects > 0)
            {
                _Intersects--;
                if (_Intersects == 0)
                {
                    Direction = Vector3.zero;
                }
            }
        }
    }
}
