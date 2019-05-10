using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Drones.Utils
{
    using Interface;
    using Object = UnityEngine.Object;

    public class PoolController
    {
        private static Dictionary<Type, PoolController> _ExistingPools = new Dictionary<Type, PoolController>();
        public static PoolController Get(IPool pool)
        {
            var type = pool.GetType();
            if (_ExistingPools.TryGetValue(type, out PoolController value))
            {
                if (value != null) return value;
                var i = new PoolController(pool);
                _ExistingPools[type] = i;
                i._Container.StartCoroutine(i.Initialize());
            }
            else
            {
                var i = new PoolController(pool);
                _ExistingPools.Add(type, i);
                i._Container.StartCoroutine(i.Initialize());
            }
            
            return _ExistingPools[type];
        }

        public static void Reset()
        {
            Type[] keys = new Type[_ExistingPools.Count];
            _ExistingPools.Keys.CopyTo(keys,0);
            for (int i = 0; i < keys.Length; i++)
                Object.Destroy(_ExistingPools[keys[i]]._Container);

            _ExistingPools = null;
        }

        public static void Clear(IPool pool)
        {
            var type = pool.GetType();
            Object.Destroy(_ExistingPools[type]._Container.gameObject);
            _ExistingPools.Remove(type);
        }

        private readonly IPool _Pool;
        private PoolController(IPool pool)
        {
            _Pool = pool;
            _Container = new GameObject(_Pool.GetType().ToString()).AddComponent<PoolComponent>();
            _Container.pool = _Pool;
            PoolParent.position = Vector3.zero;
        }

        public bool Initialized { get; private set; } = false;

        private readonly PoolComponent _Container;

        public Transform PoolParent => _Container.transform;

        public void Release(Type type, IPoolable item)
        {
            item.OnRelease();
            _Pool.Pool[type].Enqueue(item);
        }

        public T Get<T>(Transform parent, bool noOnGet = false) => (T)Get(typeof(T), parent, noOnGet);

        public IPoolable Get(Type type, Transform parent = null, bool noOnGet = false)
        {
            IPoolable item = null;
            if (_Pool.Pool.TryGetValue(type, out Queue<IPoolable> pool))
            {
                if (pool.Count < _Pool.StartSize[type] / 4 && !_Pool.IsBuilding[type])
                    _Container.StartCoroutine(Build(type));

                if (pool.Count == 0)
                    item = ManualBuild(type);
                else
                    item = pool.Dequeue();

                if (!noOnGet)
                    item.OnGet(parent);
            }
            return item;
        }

        private IPoolable ManualBuild(Type type)
        {
            GameObject go = Object.Instantiate(_Pool.Templates[type], PoolParent);
            return go.GetComponent<IPoolable>();
        }

        public GameObject GetTemplate(Type type) => _Pool.Templates[type];

        private IEnumerator Build(Type type)
        {
            _Pool.IsBuilding[type] = true;
            for (int i = 0; i < _Pool.StartSize[type]; i++)
            {
                GameObject go = Object.Instantiate(_Pool.Templates[type], PoolParent);

                Release(type, go.GetComponent<IPoolable>());

                if (TimeKeeper.DeltaFrame() > Constants.CoroutineTimeSlice)
                    yield return null;
            }
            _Pool.IsBuilding[type] = false;
        }

        public IEnumerator Initialize()
        {
            if (!Initialized)
            {
                foreach (var type in _Pool.Paths.Keys)
                {
                    try
                    {
                        _Pool.Templates.Add(type, Resources.Load(_Pool.Paths[type]) as GameObject);
                        _Pool.IsBuilding.Add(type, false);
                    }
                    catch (ArgumentException)
                    {

                    }

                    if (TimeKeeper.DeltaFrame() > Constants.CoroutineTimeSlice)
                    {
                        yield return null;
                    }
                }
                foreach (var type in _Pool.Paths.Keys)
                {
                    _Container.StartCoroutine(Build(type));
                }
                Initialized = true;
            }
            yield break;
        }
    }

}