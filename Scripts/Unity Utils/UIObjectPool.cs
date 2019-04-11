using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Drones.Utils
{
    using Interface;

    public static class UIObjectPool
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
                        name = "UIObjectPool"
                    };
                    _PoolContainer = go.transform;
                    _PoolContainer.position = Vector3.zero;
                }
                return _PoolContainer;
            }
        }

        public static void Release(Enum type, IPoolable item)
        {
            item.OnRelease();

            if (_Pool.TryGetValue(type.GetType(), out Dictionary<Enum, Queue<IPoolable>> dict))
            {
                if (!dict.ContainsKey(type))
                {
                    dict.Add(type, new Queue<IPoolable>());
                }
                dict[type].Enqueue(item);
            }
        }

        public static IPoolable Get(Enum type, Transform parent)
        {
            IPoolable item = null;
            if (_Pool.TryGetValue(type.GetType(), out Dictionary<Enum, Queue<IPoolable>> dict))
            {
                if (!dict.ContainsKey(type))
                {
                    throw new ArgumentException("No such type!");
                }
                if (dict[type].Count == 10 && !_IsBuilding[type.GetType()][type])
                {
                    SimManager.Instance.StartCoroutine(Build(type, PoolNumber[type.GetType()][type]));
                }
                if (dict[type].Count == 0)
                {
                    item = ManualBuild(type);
                }
                else
                {
                    item = dict[type].Dequeue();
                }
                item.OnGet(parent);
            }

            return item;
        }

        private static IPoolable ManualBuild(Enum type)
        {
            GameObject go = UnityEngine.Object.Instantiate(_Templates[type.GetType()][type], PoolContainer);
            return (IPoolable)go.GetComponent(_Components[type.GetType()]);
        }

        public static GameObject GetTemplate(Enum type)
        {
            return _Templates[type.GetType()][type];
        }

        private static IEnumerator Build(Enum type, int number)
        {
            _IsBuilding[type.GetType()][type] = true;
            var end = Time.realtimeSinceStartup;
            for (int i = 0; i < number; i++)
            {
                GameObject go = UnityEngine.Object.Instantiate(_Templates[type.GetType()][type], PoolContainer);

                Release(type, (IPoolable)go.GetComponent(_Components[type.GetType()]));

                if (Time.realtimeSinceStartup - end > Constants.CoroutineTimeLimit)
                {
                    yield return null;
                    end = Time.realtimeSinceStartup;
                }
            }
            _IsBuilding[type.GetType()][type] = false;
            yield break;
        }

        public static IEnumerator Init()
        {
            if (Initialized) { yield break; }

            var end = Time.realtimeSinceStartup;

            foreach (var key in PrefabPaths.Keys)
            {
                foreach (Enum type in PrefabPaths[key].Keys)
                {
                    _Templates[key].Add(type, (GameObject)Resources.Load(PrefabPaths[key][type]));
                    _IsBuilding[key].Add(type, false);

                    if (Time.realtimeSinceStartup - end > Constants.CoroutineTimeLimit)
                    {
                        yield return null;
                        end = Time.realtimeSinceStartup;
                    }
                }
            }

            foreach (var key in PrefabPaths.Keys)
            {
                foreach (Enum type in PrefabPaths[key].Keys)
                {
                    SimManager.Instance.StartCoroutine(Build(type, PoolNumber[type.GetType()][type]));
                }
            }

            Initialized = true;
            yield break;
        }

        #region Paths
        public const string WindowPrefabPath = "Prefabs/UI/Windows";
        /* Windows */
        public const string DroneWindowPath = WindowPrefabPath + "/Drone/Drone Window";
        public const string DroneListWindowPath = WindowPrefabPath + "/Drone/DroneList Window";
        public const string NavigationWindowPath = WindowPrefabPath + "/Navigation/Navigation Window";
        public const string HubWindowPath = WindowPrefabPath + "/Hub/Hub Window";
        public const string HubListWindowPath = WindowPrefabPath + "/Hub/HubList Window";
        public const string JobWindowPath = WindowPrefabPath + "/Job/Job Window";
        public const string JobHistoryWindowPath = WindowPrefabPath + "/Job/JobHistory Window";
        public const string JobQueueWindowPath = WindowPrefabPath + "/Job/JobQueue Window";
        public const string NoFlyZoneListWindowPath = WindowPrefabPath + "/NoFlyZone/NoFlyZoneList Window";
        public const string ConsoleLogPath = WindowPrefabPath + "/Console/Console Log";

        /* List Elements */
        public const string DroneListTuplePath = WindowPrefabPath + "/Drone/DroneListTuple";
        public const string HubListTuplePath = WindowPrefabPath + "/Hub/HubListTuple";
        public const string JobHistoryTuplePath = WindowPrefabPath + "/Job/JobHistoryTuple";
        public const string JobQueueTuplePath = WindowPrefabPath + "/Job/JobQueueTuple";
        public const string NoFlyZoneTuplePath = WindowPrefabPath + "/NoFlyZone/NoFlyZoneListTuple";
        public const string ConsoleElementPath = WindowPrefabPath + "/Console/ConsoleElement";
        #endregion

        #region Dictionaries
        private static Dictionary<Type, Dictionary<Enum, string>> _Paths;
        private static Dictionary<Type, Dictionary<Enum, string>> PrefabPaths
        {
            get
            {
                if (_Paths == null)
                {
                    _Paths = new Dictionary<Type, Dictionary<Enum, string>>
                    {
                        {typeof(WindowType), new Dictionary<Enum, string>()},
                        {typeof(ListElement), new Dictionary<Enum, string>()},
                    };

                    _Paths[typeof(WindowType)].Add(WindowType.Drone, DroneWindowPath);
                    _Paths[typeof(WindowType)].Add(WindowType.DroneList, DroneListWindowPath);
                    _Paths[typeof(WindowType)].Add(WindowType.Hub, HubWindowPath);
                    _Paths[typeof(WindowType)].Add(WindowType.HubList, HubListWindowPath);
                    _Paths[typeof(WindowType)].Add(WindowType.Job, JobWindowPath);
                    _Paths[typeof(WindowType)].Add(WindowType.JobQueue, JobQueueWindowPath);
                    _Paths[typeof(WindowType)].Add(WindowType.JobHistory, JobHistoryWindowPath);
                    _Paths[typeof(WindowType)].Add(WindowType.NFZList, NoFlyZoneListWindowPath);
                    _Paths[typeof(WindowType)].Add(WindowType.Console, ConsoleLogPath);
                    _Paths[typeof(WindowType)].Add(WindowType.Navigation, NavigationWindowPath);

                    _Paths[typeof(ListElement)].Add(ListElement.Console, ConsoleElementPath);
                    _Paths[typeof(ListElement)].Add(ListElement.DroneList, DroneListTuplePath);
                    _Paths[typeof(ListElement)].Add(ListElement.HubList, HubListTuplePath);
                    _Paths[typeof(ListElement)].Add(ListElement.JobQueue, JobQueueTuplePath);
                    _Paths[typeof(ListElement)].Add(ListElement.JobHistory, JobHistoryTuplePath);
                    _Paths[typeof(ListElement)].Add(ListElement.NFZList, NoFlyZoneTuplePath);
                }

                return _Paths;
            }
        }

        private static Dictionary<Type, Dictionary<Enum, int>> _PoolNumber;

        private static Dictionary<Type, Dictionary<Enum, int>> PoolNumber
        {
            get
            {
                if (_PoolNumber == null)
                {
                    _PoolNumber = new Dictionary<Type, Dictionary<Enum, int>>
                    {
                        {typeof(WindowType), new Dictionary<Enum, int>()},
                        {typeof(ListElement), new Dictionary<Enum, int>()},
                    };

                    _PoolNumber[typeof(WindowType)].Add(WindowType.Drone, 20);
                    _PoolNumber[typeof(WindowType)].Add(WindowType.DroneList, 20);
                    _PoolNumber[typeof(WindowType)].Add(WindowType.Hub, 20);
                    _PoolNumber[typeof(WindowType)].Add(WindowType.HubList, 20);
                    _PoolNumber[typeof(WindowType)].Add(WindowType.Job, 20);
                    _PoolNumber[typeof(WindowType)].Add(WindowType.JobQueue, 20);
                    _PoolNumber[typeof(WindowType)].Add(WindowType.JobHistory, 20);
                    _PoolNumber[typeof(WindowType)].Add(WindowType.NFZList, 1);
                    _PoolNumber[typeof(WindowType)].Add(WindowType.Console, 0);
                    _PoolNumber[typeof(WindowType)].Add(WindowType.Navigation, 0);

                    _PoolNumber[typeof(ListElement)].Add(ListElement.Console, 100);
                    _PoolNumber[typeof(ListElement)].Add(ListElement.DroneList, 100);
                    _PoolNumber[typeof(ListElement)].Add(ListElement.HubList, 100);
                    _PoolNumber[typeof(ListElement)].Add(ListElement.JobQueue, 100);
                    _PoolNumber[typeof(ListElement)].Add(ListElement.JobHistory, 100);
                    _PoolNumber[typeof(ListElement)].Add(ListElement.NFZList, 100);
                }

                return _PoolNumber;
            }
        }

        private readonly static Dictionary<Type, string> _Components
        = new Dictionary<Type, string>
        {
            {typeof(WindowType), "AbstractWindow"},
            {typeof(ListElement), "AbstractListElement"}
        };

        private readonly static Dictionary<Type, Dictionary<Enum, Queue<IPoolable>>> _Pool
        = new Dictionary<Type, Dictionary<Enum, Queue<IPoolable>>>
        {
            {typeof(WindowType), new Dictionary<Enum, Queue<IPoolable>>()},
            {typeof(ListElement), new Dictionary<Enum, Queue<IPoolable>>()},
        };

        private readonly static Dictionary<Type, Dictionary<Enum, GameObject>> _Templates
        = new Dictionary<Type, Dictionary<Enum, GameObject>>
        {
            {typeof(WindowType), new Dictionary<Enum, GameObject>()},
            {typeof(ListElement), new Dictionary<Enum, GameObject>()}
        };

        private readonly static Dictionary<Type, Dictionary<Enum, bool>> _IsBuilding
        = new Dictionary<Type, Dictionary<Enum, bool>>
        {
            {typeof(WindowType), new Dictionary<Enum, bool>()},
            {typeof(ListElement), new Dictionary<Enum, bool>()}
        };
        #endregion
    }

}