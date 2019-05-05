using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones
{
    using Interface;
    using Utils;
    using Managers;

    public static class ObjectPool
    {
        public static bool Initialized { get; private set; } = false;

        private static Transform _PoolContainer;

        public static Transform PoolContainer
        {
            get
            {
                if (_PoolContainer == null)
                {
                    GameObject go = new GameObject
                    {
                        name = "ObjectPool"
                    };
                    _PoolContainer = go.transform;
                    _PoolContainer.position = Vector3.zero;
                }
                return _PoolContainer;
            }
        }

        public static IPoolable Get(Type type, bool isLoad = false)
        {
            IPoolable item = null;
            if (_Pool.TryGetValue(type, out Queue<IPoolable> pool))
            {
                if (pool.Count < _PoolNumber[type] / 3 && !_IsBuilding[type])
                    SimManager.Instance.StartCoroutine(Build(type, _PoolNumber[type]));

                if (pool.Count == 0)
                    item = ManualBuild(type);
                else
                    item = pool.Dequeue();

                if (!isLoad)
                    item.OnGet(null);
            }

            return item;
        }

        private static IPoolable ManualBuild(Type type)
        {
            GameObject go = UnityEngine.Object.Instantiate(_Templates[type], PoolContainer);
            return (IPoolable)go.GetComponent(type.ToString());
        }

        private static IEnumerator Build(Type type, int number)
        {
            _IsBuilding[type] = true;
            var end = Time.realtimeSinceStartup;
            for (int i = 0; i < number; i++)
            {
                GameObject go = UnityEngine.Object.Instantiate(_Templates[type], PoolContainer);

                Release((IPoolable)go.GetComponent(type.ToString()));

                if (Time.realtimeSinceStartup - end > Constants.CoroutineTimeSlice)
                {
                    yield return null;
                    end = Time.realtimeSinceStartup;
                }
            }
            _IsBuilding[type] = false;
            yield break;
        }

        public static IEnumerator Init()
        {
            if (Initialized) { yield break; }

            var end = Time.realtimeSinceStartup;

            foreach (var type in _PrefabPaths.Keys)
            {
                _Templates[type] = (GameObject)Resources.Load(_PrefabPaths[type]);

                if (Time.realtimeSinceStartup - end > Constants.CoroutineTimeSlice)
                {
                    yield return null;
                    end = Time.realtimeSinceStartup;
                }
            }

            foreach (var type in _PrefabPaths.Keys)
            {
                SimManager.Instance.StartCoroutine(Build(type, _PoolNumber[type]));
            }
            Initialized = true;
            yield break;
        }

        public static void Release(IPoolable item)
        {
            item.OnRelease();

            if (_Pool.TryGetValue(item.GetType(), out Queue<IPoolable> pool))
            {
                pool.Enqueue(item);
            }
        }

        #region Paths
        private const string DroneObjectPath = "Prefabs/Objects/Drone";
        private const string HubObjectPath = "Prefabs/Objects/Hub";
        private const string NFZObjectPath = "Prefabs/Objects/NoFlyZone";
        #endregion

        #region Dictionary
        private readonly static Dictionary<Type, string> _PrefabPaths = new Dictionary<Type, string>
        {
            {typeof(Drone), DroneObjectPath},
            {typeof(Hub), HubObjectPath},
            {typeof(NoFlyZone), NFZObjectPath}
        };

        private readonly static Dictionary<Type, Queue<IPoolable>> _Pool = new Dictionary<Type, Queue<IPoolable>>
        {
            {typeof(Drone), new Queue<IPoolable>()},
            {typeof(Hub), new Queue<IPoolable>()},
            {typeof(NoFlyZone), new Queue<IPoolable>()}
        };

        private readonly static Dictionary<Type, bool> _IsBuilding = new Dictionary<Type, bool>
        {
            {typeof(Drone), false},
            {typeof(Hub), false},
            {typeof(NoFlyZone), false}
        };

        private readonly static Dictionary<Type, GameObject> _Templates = new Dictionary<Type, GameObject>
        {
            {typeof(Drone), null},
            {typeof(Hub), null},
            {typeof(NoFlyZone), null}
        };

        private readonly static Dictionary<Type, int> _PoolNumber = new Dictionary<Type, int>
        {
            {typeof(Drone), 300},
            {typeof(Hub), 40},
            {typeof(NoFlyZone), 40}
        };
        #endregion
    }
}
