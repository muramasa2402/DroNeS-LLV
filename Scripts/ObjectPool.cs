using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Drones.Utils
{
    using static Constants;
    public class ObjectPool : MonoBehaviour
    {
        public bool Initializing { get; private set; }
        private readonly static Dictionary<Type, string> _Components 
        = new Dictionary<Type, string>
        {
            {typeof(WindowType), "AbstractWindow"},
            {typeof(ListElement), "AbstractListElement"}
        };

        private readonly static Dictionary<Type, int> _Numbers 
        = new Dictionary<Type, int>
        {
            {typeof(WindowType), 1},
            {typeof(ListElement), 1}
        };

        private readonly static Dictionary<Type, Dictionary<Enum, Queue<Component>>> _Pool 
        = new Dictionary<Type, Dictionary<Enum, Queue<Component>>>
        {
            {typeof(WindowType), new Dictionary<Enum, Queue<Component>>()},
            {typeof(ListElement), new Dictionary<Enum, Queue<Component>>()},
        };

        private readonly static Dictionary<Type, Dictionary<Enum, Component>> _Templates 
        = new Dictionary<Type, Dictionary<Enum, Component>>
        {
            {typeof(WindowType), new Dictionary<Enum, Component>()},
            {typeof(ListElement), new Dictionary<Enum, Component>()},
        };

        private readonly static Dictionary<Type, Dictionary<Enum, bool>> _IsBuilding
        = new Dictionary<Type, Dictionary<Enum, bool>>
        {
            {typeof(WindowType), new Dictionary<Enum, bool>()},
            {typeof(ListElement), new Dictionary<Enum, bool>()}
        };

        public void Dump(Enum type, Component item)
        {
            item.gameObject.SetActive(false);
            item.transform.SetParent(transform, false);

            if (_Pool.TryGetValue(type.GetType(), out Dictionary<Enum, Queue<Component>> dict))
            {
                if (!dict.ContainsKey(type))
                {
                    dict.Add(type, new Queue<Component>());
                }
                dict[type].Enqueue(item);
            }
        }

        public Component Get(Enum type, Transform parent)
        {
            Component item = null;
            if (_Pool.TryGetValue(type.GetType(), out Dictionary<Enum, Queue<Component>> dict))
            {
                if (!dict.ContainsKey(type))
                {
                    throw new ArgumentException("No such type!");
                }
                if (dict[type].Count < _Numbers[type.GetType()] / 2 && !_IsBuilding[type.GetType()][type])
                {
                    StartCoroutine(Build(type, _Numbers[type.GetType()]));
                }
                if (dict[type].Count == 0)
                {
                    item = ManualBuild(type);
                }
                else
                {
                    item = dict[type].Dequeue();
                }
                item.gameObject.SetActive(true);
                item.transform.SetParent(parent, false);
            }
            return item;
        }

        private Component ManualBuild(Enum type)
        {
            GameObject go = Instantiate(_Templates[type.GetType()][type].gameObject, transform);
            return go.GetComponent(_Components[type.GetType()]);
        }

        public Component PeekTemplate(Enum type)
        {
            return _Templates[type.GetType()][type];
        }

        IEnumerator Build(Enum type, int number)
        {
            _IsBuilding[type.GetType()][type] = true;
            var end = Time.realtimeSinceStartup;
            for (int i = 0; i < number; i++)
            {
                GameObject go = Instantiate(_Templates[type.GetType()][type].gameObject, transform);
                if (go == null) { Debug.Log(type); }
                go.SetActive(false);
                _Pool[type.GetType()][type].Enqueue(go.GetComponent(_Components[type.GetType()]));
                if (Time.realtimeSinceStartup - end > CoroutineTimeLimit)
                {
                    yield return null;
                    end = Time.realtimeSinceStartup;
                }
            }
            _IsBuilding[type.GetType()][type] = false;
            yield break;
        }

        IEnumerator Init()
        {
            Initializing = true;
            var end = Time.realtimeSinceStartup;
            foreach (var key in _Pool.Keys)
            {
                foreach (Enum type in Enum.GetValues(key))
                {
                    if (!PrefabPaths[key].TryGetValue(type, out var path)) { continue; }
                    GameObject go = (GameObject)Resources.Load(path);
                    _Templates[key].Add(type, go.GetComponent(_Components[key]));
                    _Pool[key].Add(type, new Queue<Component>());
                    _IsBuilding[key].Add(type, false);
                    if (Time.realtimeSinceStartup - end > CoroutineTimeLimit)
                    {
                        yield return null;
                        end = Time.realtimeSinceStartup;
                    }
                }
            }
            foreach (var key in _Pool.Keys)
            {
                foreach (Enum type in _Pool[key].Keys)
                {
                    StartCoroutine(Build(type, _Numbers[key]));
                }
            }
            Initializing = false;
            yield break;
        }

        private void Awake()
        {
            StartCoroutine(Init());
        }



    }

}