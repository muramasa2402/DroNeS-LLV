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
    using Data;
    public class NoFlyZone : MonoBehaviour, IPoolable, IDataSource
    {
        public static NoFlyZone New() => PoolController.Get(ObjectPool.Instance).Get<NoFlyZone>(null);
        public static NoFlyZone Load(SNoFlyZone data)
        {
            var nfz = PoolController.Get(ObjectPool.Instance).Get<NoFlyZone>(null, true);
            nfz.InPool = false;
            nfz._Data = new NFZData(data, nfz);
            SimManager.AllNFZ.Add(nfz.UID, nfz);
            return nfz;
        }

        public string Name => "NFZ" + UID.ToString("000000");
        public override string ToString() => Name;

        private NFZData _Data;
        private void OnTriggerEnter(Collider other)
        {
            var obj = other.GetComponent<IDataSource>();
            if (obj != null)
            {
                if (obj is Drone)
                {
                    _Data.droneEntryCount++;
                    ConsoleLog.WriteToConsole(new NoFlyZoneEntry(obj, this));
                }
                else if (obj is Hub)
                {
                    _Data.hubEntryCount++;
                    ConsoleLog.WriteToConsole(new NoFlyZoneEntry(obj, this));
                }
            }
        }
        public Vector3 Position => transform.position;

        #region IPoolable
        public PoolController PC() => PoolController.Get(ObjectPool.Instance);
        public bool InPool { get; private set; }
        public void Delete() => PC().Release(GetType(), this);

        public void OnRelease()
        {
            InPool = true;
            _Data = null;
            SimManager.AllNFZ.Remove(this);
            transform.SetParent(PC().PoolParent);
            gameObject.SetActive(false);
        }

        public void OnGet(Transform parent = null)
        {
            InPool = false;
            _Data = new NFZData(this);
            gameObject.SetActive(true);
            transform.SetParent(parent);
            SimManager.AllNFZ.Add(UID, this);
        }
        #endregion

        #region IDataSource
        public uint UID { get; private set; }

        public bool IsDataStatic { get; } = false;

        public AbstractInfoWindow InfoWindow { get; set; } = null;

        public void GetData(ISingleDataSourceReceiver receiver) => receiver.SetData(_Data);

        public void OpenInfoWindow() { return; }
        #endregion

        public SNoFlyZone Serialize() => new SNoFlyZone(_Data);

    }

}