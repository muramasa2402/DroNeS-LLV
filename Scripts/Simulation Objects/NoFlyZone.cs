using UnityEngine.EventSystems;
using UnityEngine;
using Drones.Utils.Extensions;
using System.Collections;
using System.Collections.Generic;

namespace Drones
{
    using Drones.DataStreamer;
    using Drones.Interface;
    using Drones.UI;
    using Drones.Utils;

    public class NoFlyZone : MonoBehaviour, IPoolable, IDataSource
    {
        private readonly Dictionary<System.Type, int> _EntryCount = new Dictionary<System.Type, int>
        {
            {typeof(Drone), 0},
            {typeof(Hub), 0}
        };
        private SecureSet<ISingleDataSourceReceiver> _Connections;


        private void OnTriggerEnter(Collider other)
        {
            _EntryCount[other.GetComponent<IDronesObject>().GetType()]++;
            //TODO Invoke simulation event
        }

        public int GetCount(System.Type entree)
        {
            return _EntryCount.TryGetValue(entree, out int count) ? count : 0;
        }

        public Vector2 Location
        {
            get
            {
                return transform.position.ToCoordinates();
            }
        }

        #region IPoolable
        public void SelfRelease()
        {
            ObjectPool.Release(this);
        }

        public void OnRelease()
        {
            foreach (var key in _EntryCount.Keys)
            {
                _EntryCount[key] = 0;
            }
            GameManager.AllNFZ.Remove(this);
            Connections.Clear();
            if (InfoWindow != null)
            {
                InfoWindow.SelfRelease();
                InfoWindow = null;
            }
            transform.SetParent(ObjectPool.PoolContainer);
            gameObject.SetActive(false);
        }

        public void OnGet(Transform parent)
        {
            gameObject.SetActive(false);
            transform.SetParent(null);
            GameManager.AllNFZ.Add(this);
        }
        #endregion

        #region IDataSource
        public AbstractInfoWindow InfoWindow { get; set; } = null;

        public SecureSet<ISingleDataSourceReceiver> Connections
        {
            get
            {
                if (_Connections == null)
                {
                    _Connections = new SecureSet<ISingleDataSourceReceiver>
                    {
                        MemberCondition = (ISingleDataSourceReceiver obj) => obj is ListTuple
                    };
                }
                return _Connections;
            }
        }

        public int TotalConnections
        {
            get
            {
                return Connections.Count;
            }
        }

        public string[] GetData(WindowType windowType)
        {
            var output = new string[3];
            output[0] = Location.ToString();
            output[1] = GetCount(typeof(Drone)).ToString();
            output[2] = GetCount(typeof(Hub)).ToString();
            return output;
        }

        public void OpenInfoWindow()
        {
            return;
        }
        #endregion

    }

}