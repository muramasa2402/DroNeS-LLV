using UnityEngine;
using UnityEngine.UI;

namespace Drones.UI
{
    using EventSystem;
    using Utils;
    using Utils.Extensions;
    using static Singletons;

    public class ListTupleContainer : MonoBehaviour
    {
        private bool Unassigned = true;
        private float _Separation;
        [SerializeField]
        private float _ListTupleHeight;

        public void Awake()
        {
            SimulationEvent.RegisterListener(EventType.ListUpdate, OnListChange);
        }

        public void GetHeight()
        {
            if (transform.childCount > 0)
            {
                _ListTupleHeight = transform.GetChild(0).ToRect().sizeDelta.y;
            }
        }

        public void GetSeparation()
        {
            _Separation = GetComponent<VerticalLayoutGroup>().spacing;
        }

        public void SetHeight()
        {
            int n = transform.childCount;
            Vector2 sizeDelta = transform.ToRect().sizeDelta;
            sizeDelta.y = n * _ListTupleHeight + (n - 1) * _Separation;
            transform.ToRect().sizeDelta = sizeDelta;
        }

        private void OnListChange(IEvent info)
        {
            if (Unassigned)
            {
                GetHeight();
                GetSeparation();
                Unassigned = false;
            }
            if (info.GO != null && info.GO == gameObject)
            {
                SetHeight();
            }
        }
    }
}