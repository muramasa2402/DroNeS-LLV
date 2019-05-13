using UnityEngine;
using System;

namespace Drones
{
    using Utils.Extensions;
    using DataStreamer;
    using EventSystem;
    using Interface;
    using Serializable;
    using UI;
    using Utils;
    using Managers;

    public class NoFlyZone : MonoBehaviour, IPoolable, IDataSource
    {
        private static uint _Count;
        public static void Reset() => _Count = 0;
        public static NoFlyZone New() => PoolController.Get(ObjectPool.Instance).Get<NoFlyZone>(null);

        private uint _DroneEntryCount;
        private uint _HubEntryCount;
        private SecureSortedSet<int, ISingleDataSourceReceiver> _Connections;

        private void OnTriggerEnter(Collider other)
        {
            var obj = other.GetComponent<IDataSource>();
            if (obj != null)
            {
                if (obj is Drone)
                {
                    _DroneEntryCount++;
                    ConsoleLog.WriteToConsole(new NoFlyZoneEntry(obj, this));
                }
                else if (obj is Hub)
                {
                    _HubEntryCount++;
                    ConsoleLog.WriteToConsole(new NoFlyZoneEntry(obj, this));
                }
            }
        }

        public Vector2 Location => transform.position.ToCoordinates();

        #region IPoolable
        public PoolController PC() => PoolController.Get(ObjectPool.Instance);
        public bool InPool { get; private set; }
        public void Delete() => PC().Release(GetType(), this);

        public void OnRelease()
        {
            InPool = true;
            _HubEntryCount = 0;
            _DroneEntryCount = 0;
            SimManager.AllNFZ.Remove(this);
            Connections.Clear();
            transform.SetParent(PC().PoolParent);
            gameObject.SetActive(false);
        }

        public void OnGet(Transform parent = null)
        {
            InPool = false;
            UID = ++_Count;
            gameObject.SetActive(true);
            transform.SetParent(parent);
            SimManager.AllNFZ.Add(UID, this);
        }
        #endregion

        #region IDataSource
        public uint UID { get; private set; }

        public bool IsDataStatic { get; } = false;

        public AbstractInfoWindow InfoWindow { get; set; } = null;

        public SecureSortedSet<int, ISingleDataSourceReceiver> Connections
        {
            get
            {
                if (_Connections == null)
                {
                    _Connections = new SecureSortedSet<int, ISingleDataSourceReceiver>
                    {
                        MemberCondition = (ISingleDataSourceReceiver obj) => obj is ObjectTuple
                    };
                }
                return _Connections;
            }
        }

        public string[] GetData(Type windowType)
        {
            var output = new string[3];
            output[0] = Location.ToString();
            output[1] = _DroneEntryCount.ToString();
            output[2] = _HubEntryCount.ToString();
            return output;
        }

        public void OpenInfoWindow()
        {
            return;
        }
        #endregion

        public SNoFlyZone Serialize()
        {
            return new SNoFlyZone
            {
                count = _Count,
                uid = UID,
                droneEntry = _DroneEntryCount,
                hubEntry = _HubEntryCount,
                position = transform.position,
                orientation = transform.eulerAngles,
                size = transform.localScale
            };
        }

        public static NoFlyZone Load(SNoFlyZone data)
        {
            var nfz = PoolController.Get(ObjectPool.Instance).Get<NoFlyZone>(null, true);
            nfz.InPool = false;
            _Count = data.count;
            nfz.UID = data.uid;
            nfz._HubEntryCount = data.hubEntry;
            nfz._DroneEntryCount = data.droneEntry;
            nfz.transform.position = data.position;
            nfz.transform.eulerAngles = data.orientation;
            nfz.transform.localScale = data.size;
            SimManager.AllNFZ.Add(nfz.UID, nfz);
            return nfz;

        }

    }

}