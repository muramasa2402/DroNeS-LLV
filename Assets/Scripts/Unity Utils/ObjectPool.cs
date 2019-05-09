using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones
{
    using Interface;
    using Utils;
    using Managers;

    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }
        public static bool Initialized { get; private set; }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Initialized = false;
        }

        public static Transform PoolContainer { get; private set; }

        public static IPoolable Get(Type type, bool isLoad = false)
        {
            IPoolable item = null;
            if (Pool.TryGetValue(type, out Queue<IPoolable> pool))
            {
                if (pool.Count < PoolNumber[type] / 3 && !IsBuilding[type])
                    SimManager.Instance.StartCoroutine(Build(type, PoolNumber[type]));

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
            GameObject go = Instantiate(Templates[type], PoolContainer);
            return (IPoolable)go.GetComponent(type.ToString());
        }

        private static IEnumerator Build(Type type, int number)
        {
            IsBuilding[type] = true;
            var end = Time.realtimeSinceStartup;
            for (int i = 0; i < number; i++)
            {
                GameObject go = Instantiate(Templates[type], PoolContainer);

                Release((IPoolable)go.GetComponent(type.ToString()));

                if (TimeKeeper.DeltaFrame() > Constants.CoroutineTimeSlice)
                {
                    yield return null;
                    end = Time.realtimeSinceStartup;
                }
            }
            IsBuilding[type] = false;
            yield break;
        }

        public static IEnumerator Init()
        {
            if (Initialized) { yield break; }
            GameObject go = new GameObject
            {
                name = "ObjectPool"
            };
            go.AddComponent<ObjectPool>();
            PoolContainer = go.transform;
            PoolContainer.position = Vector3.zero;

            foreach (var type in PrefabPaths.Keys)
            {

                try
                {
                    Templates[type] = (GameObject)Resources.Load(PrefabPaths[type]);
                }
                catch (ArgumentException)
                {

                }

                if (TimeKeeper.DeltaFrame() > Constants.CoroutineTimeSlice)
                {
                    yield return null;
                }
            }

            foreach (var type in PrefabPaths.Keys)
            {
                SimManager.Instance.StartCoroutine(Build(type, PoolNumber[type]));
            }
            Initialized = true;
            yield break;
        }

        public static void Release(IPoolable item)
        {
            item.OnRelease();

            if (Pool.TryGetValue(item.GetType(), out Queue<IPoolable> pool))
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
        public static Dictionary<Type, string> PrefabPaths => Instance._PrefabPaths;

        public static Dictionary<Type, Queue<IPoolable>> Pool => Instance._Pool;

        public static Dictionary<Type, bool> IsBuilding => Instance._IsBuilding;

        public static Dictionary<Type, GameObject> Templates => Instance._Templates;

        public static Dictionary<Type, int> PoolNumber => Instance._PoolNumber;

        private readonly Dictionary<Type, string> _PrefabPaths = new Dictionary<Type, string>
        {
            {typeof(Drone), DroneObjectPath},
            {typeof(Hub), HubObjectPath},
            {typeof(NoFlyZone), NFZObjectPath}
        };

        private readonly Dictionary<Type, Queue<IPoolable>> _Pool = new Dictionary<Type, Queue<IPoolable>>
        {
            {typeof(Drone), new Queue<IPoolable>()},
            {typeof(Hub), new Queue<IPoolable>()},
            {typeof(NoFlyZone), new Queue<IPoolable>()}
        };

        private readonly Dictionary<Type, bool> _IsBuilding = new Dictionary<Type, bool>
        {
            {typeof(Drone), false},
            {typeof(Hub), false},
            {typeof(NoFlyZone), false}
        };

        private readonly Dictionary<Type, GameObject> _Templates = new Dictionary<Type, GameObject>
        {
            {typeof(Drone), null},
            {typeof(Hub), null},
            {typeof(NoFlyZone), null}
        };

        private readonly Dictionary<Type, int> _PoolNumber = new Dictionary<Type, int>
        {
            {typeof(Drone), 300},
            {typeof(Hub), 40},
            {typeof(NoFlyZone), 40}
        };
        #endregion
    }
}
