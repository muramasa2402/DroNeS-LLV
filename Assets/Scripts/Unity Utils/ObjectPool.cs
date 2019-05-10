using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones
{
    using Interface;
    using Utils;
    using Managers;

    public class ObjectPool : AbstractPool, IPool
    {
        private static ObjectPool _Instance;
        public static ObjectPool Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new ObjectPool();
                }
                return _Instance;
            }
        }

        public static void Reset()
        {
            if (_Instance != null)
            {
                Instance.Pool.Clear();
                Instance.Templates.Clear();
                _Instance = null;
            }
        }

        public Dictionary<Type, string> Paths
        {
            get
            {
                if (_Paths == null)
                {
                    _Paths = new Dictionary<Type, string>
                    {
                        { typeof(Drone), DroneObjectPath },
                        { typeof(Hub), HubObjectPath },
                        { typeof(NoFlyZone), NFZObjectPath }
                    };
                }

                return _Paths;
            }
        }

        public Dictionary<Type, uint> StartSize
        {
            get
            {
                if (_StartSize == null)
                {
                    _StartSize = new Dictionary<Type, uint>
                    {
                        { typeof(Drone), 700 },
                        { typeof(Hub), 20 },
                        { typeof(NoFlyZone), 20 }
                    };
                }

                return _StartSize;
            }
        }

        public Dictionary<Type, GameObject> Templates { get; } = new Dictionary<Type, GameObject>();

        public Dictionary<Type, Queue<IPoolable>> Pool
        {
            get
            {
                if (_Pool == null)
                {
                    _Pool = new Dictionary<Type, Queue<IPoolable>>
                    {
                        { typeof(Drone), new Queue<IPoolable>() },
                        { typeof(Hub), new Queue<IPoolable>() },
                        { typeof(NoFlyZone), new Queue<IPoolable>() }
                    };
                }

                return _Pool;
            }
        }

        public Dictionary<Type, bool> IsBuilding { get; } = new Dictionary<Type, bool>();

        #region Paths
        private const string DroneObjectPath = "Prefabs/Objects/Drone";
        private const string HubObjectPath = "Prefabs/Objects/Hub";
        private const string NFZObjectPath = "Prefabs/Objects/NoFlyZone";
        #endregion
    }
}
