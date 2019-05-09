using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Drones.Utils
{
    using Interface;
    using Managers;

    public class UIObjectPool: MonoBehaviour
    {
        public static UIObjectPool Instance { get; private set; }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Initialized = false;
        }

        public static bool Initialized { get; private set; }

        public static Transform PoolContainer{ get; private set; }

        public static void Release(Enum type, IPoolable item)
        {
            item.OnRelease();

            if (Pool.TryGetValue(type.GetType(), out Dictionary<Enum, Queue<IPoolable>> dict))
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
            if (Pool.TryGetValue(type.GetType(), out Dictionary<Enum, Queue<IPoolable>> dict))
            {
                if (!dict.ContainsKey(type))
                {
                    throw new ArgumentException("No such type!");
                }
                if (dict[type].Count == PoolNumber[type.GetType()][type]/4 && !IsBuilding[type.GetType()][type])
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
            GameObject go = Instantiate(Templates[type.GetType()][type], PoolContainer);
            return (IPoolable)go.GetComponent(Components[type.GetType()]);
        }

        public static GameObject GetTemplate(Enum type)
        {
            return Templates[type.GetType()][type];
        }

        private static IEnumerator Build(Enum type, int number)
        {
            IsBuilding[type.GetType()][type] = true;
            var end = Time.realtimeSinceStartup;
            for (int i = 0; i < number; i++)
            {
                GameObject go = Instantiate(Templates[type.GetType()][type], PoolContainer);

                Release(type, (IPoolable)go.GetComponent(Components[type.GetType()]));

                if (TimeKeeper.DeltaFrame() > Constants.CoroutineTimeSlice)
                {
                    yield return null;
                }
            }
            IsBuilding[type.GetType()][type] = false;
            yield break;
        }

        public static IEnumerator Init()
        {
            if (Initialized) { yield break; }
            GameObject go = new GameObject
            {
                name = "UIObjectPool"
            };
            go.AddComponent<UIObjectPool>();
            PoolContainer = go.transform;
            PoolContainer.position = Vector3.zero;

            foreach (var key in PrefabPaths.Keys)
            {
                foreach (Enum type in PrefabPaths[key].Keys)
                {
                    try
                    {
                        Templates[key].Add(type, (GameObject)Resources.Load(PrefabPaths[key][type]));
                        IsBuilding[key].Add(type, false);
                    }
                    catch (ArgumentException)
                    {

                    }

                    if (TimeKeeper.DeltaFrame() > Constants.CoroutineTimeSlice)
                    {
                        yield return null;
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
        public const string RetiredDroneWindowPath = WindowPrefabPath + "/Drone/RetiredDrone Window";
        public const string RetiredDroneListWindowPath = WindowPrefabPath + "/Drone/RetiredDroneList Window";
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
        public const string RetiredDroneListTuplePath = WindowPrefabPath + "/Drone/RetiredDroneListTuple";
        public const string HubListTuplePath = WindowPrefabPath + "/Hub/HubListTuple";
        public const string JobHistoryTuplePath = WindowPrefabPath + "/Job/JobHistoryTuple";
        public const string JobQueueTuplePath = WindowPrefabPath + "/Job/JobQueueTuple";
        public const string NoFlyZoneTuplePath = WindowPrefabPath + "/NoFlyZone/NoFlyZoneListTuple";
        public const string ConsoleElementPath = WindowPrefabPath + "/Console/ConsoleElement";
        public const string SaveLoadTuplePath = WindowPrefabPath + "/SaveLoad/SaveLoadTuple";
        #endregion

        #region Dictionaries
        private Dictionary<Type, Dictionary<Enum, string>> _Paths;
        private static Dictionary<Type, Dictionary<Enum, string>> PrefabPaths
        {
            get
            {
                if (Instance._Paths == null)
                {
                    Instance._Paths = new Dictionary<Type, Dictionary<Enum, string>>
                    {
                        {typeof(WindowType), new Dictionary<Enum, string>()},
                        {typeof(ListElement), new Dictionary<Enum, string>()},
                    };

                    Instance._Paths[typeof(WindowType)].Add(WindowType.Drone, DroneWindowPath);
                    Instance._Paths[typeof(WindowType)].Add(WindowType.DroneList, DroneListWindowPath);
                    Instance._Paths[typeof(WindowType)].Add(WindowType.RetiredDrone, RetiredDroneWindowPath);
                    Instance._Paths[typeof(WindowType)].Add(WindowType.RetiredDroneList, RetiredDroneListWindowPath);
                    Instance._Paths[typeof(WindowType)].Add(WindowType.Hub, HubWindowPath);
                    Instance._Paths[typeof(WindowType)].Add(WindowType.HubList, HubListWindowPath);
                    Instance._Paths[typeof(WindowType)].Add(WindowType.Job, JobWindowPath);
                    Instance._Paths[typeof(WindowType)].Add(WindowType.JobQueue, JobQueueWindowPath);
                    Instance._Paths[typeof(WindowType)].Add(WindowType.JobHistory, JobHistoryWindowPath);
                    Instance._Paths[typeof(WindowType)].Add(WindowType.NFZList, NoFlyZoneListWindowPath);
                    Instance._Paths[typeof(WindowType)].Add(WindowType.Console, ConsoleLogPath);
                    Instance._Paths[typeof(WindowType)].Add(WindowType.Navigation, NavigationWindowPath);

                    Instance._Paths[typeof(ListElement)].Add(ListElement.Console, ConsoleElementPath);
                    Instance._Paths[typeof(ListElement)].Add(ListElement.DroneList, DroneListTuplePath);
                    Instance._Paths[typeof(ListElement)].Add(ListElement.RetiredDroneList, RetiredDroneListTuplePath);
                    Instance._Paths[typeof(ListElement)].Add(ListElement.HubList, HubListTuplePath);
                    Instance._Paths[typeof(ListElement)].Add(ListElement.JobQueue, JobQueueTuplePath);
                    Instance._Paths[typeof(ListElement)].Add(ListElement.JobHistory, JobHistoryTuplePath);
                    Instance._Paths[typeof(ListElement)].Add(ListElement.NFZList, NoFlyZoneTuplePath);
                    Instance._Paths[typeof(ListElement)].Add(ListElement.SaveLoad, SaveLoadTuplePath);
                }

                return Instance._Paths;
            }
        }

        private Dictionary<Type, Dictionary<Enum, int>> _PoolNumber;

        private static Dictionary<Type, Dictionary<Enum, int>> PoolNumber
        {
            get
            {
                if (Instance._PoolNumber == null)
                {
                    Instance._PoolNumber = new Dictionary<Type, Dictionary<Enum, int>>
                    {
                        {typeof(WindowType), new Dictionary<Enum, int>()},
                        {typeof(ListElement), new Dictionary<Enum, int>()},
                    };

                    Instance._PoolNumber[typeof(WindowType)].Add(WindowType.Drone, 10);
                    Instance._PoolNumber[typeof(WindowType)].Add(WindowType.DroneList, 10);
                    Instance._PoolNumber[typeof(WindowType)].Add(WindowType.RetiredDrone, 10);
                    Instance._PoolNumber[typeof(WindowType)].Add(WindowType.Hub, 10);
                    Instance._PoolNumber[typeof(WindowType)].Add(WindowType.HubList, 10);
                    Instance._PoolNumber[typeof(WindowType)].Add(WindowType.Job, 10);
                    Instance._PoolNumber[typeof(WindowType)].Add(WindowType.JobHistory, 10);
                    Instance._PoolNumber[typeof(WindowType)].Add(WindowType.JobQueue, 5);
                    Instance._PoolNumber[typeof(WindowType)].Add(WindowType.RetiredDroneList, 5);
                    Instance._PoolNumber[typeof(WindowType)].Add(WindowType.NFZList, 2);
                    Instance._PoolNumber[typeof(WindowType)].Add(WindowType.Console, 2);
                    Instance._PoolNumber[typeof(WindowType)].Add(WindowType.Navigation, 2);

                    Instance._PoolNumber[typeof(ListElement)].Add(ListElement.Console, 100);
                    Instance._PoolNumber[typeof(ListElement)].Add(ListElement.DroneList, 100);
                    Instance._PoolNumber[typeof(ListElement)].Add(ListElement.RetiredDroneList, 100);
                    Instance._PoolNumber[typeof(ListElement)].Add(ListElement.HubList, 50);
                    Instance._PoolNumber[typeof(ListElement)].Add(ListElement.JobQueue, 100);
                    Instance._PoolNumber[typeof(ListElement)].Add(ListElement.JobHistory, 100);
                    Instance._PoolNumber[typeof(ListElement)].Add(ListElement.NFZList, 100);
                    Instance._PoolNumber[typeof(ListElement)].Add(ListElement.SaveLoad, 30);
                }

                return Instance._PoolNumber;
            }
        }

        public static Dictionary<Type, Dictionary<Enum, GameObject>> Templates => Instance._Templates;

        public static Dictionary<Type, string> Components => Instance._Components;

        public static Dictionary<Type, Dictionary<Enum, Queue<IPoolable>>> Pool => Instance._Pool;

        public static Dictionary<Type, Dictionary<Enum, bool>> IsBuilding => Instance._IsBuilding;

        private readonly Dictionary<Type, string> _Components
        = new Dictionary<Type, string>
        {
            {typeof(WindowType), "AbstractWindow"},
            {typeof(ListElement), "AbstractListElement"}
        };

        private readonly Dictionary<Type, Dictionary<Enum, Queue<IPoolable>>> _Pool
        = new Dictionary<Type, Dictionary<Enum, Queue<IPoolable>>>
        {
            {typeof(WindowType), new Dictionary<Enum, Queue<IPoolable>>()},
            {typeof(ListElement), new Dictionary<Enum, Queue<IPoolable>>()},
        };

        private readonly Dictionary<Type, Dictionary<Enum, GameObject>> _Templates
        = new Dictionary<Type, Dictionary<Enum, GameObject>>
        {
            {typeof(WindowType), new Dictionary<Enum, GameObject>()},
            {typeof(ListElement), new Dictionary<Enum, GameObject>()}
        };

        private readonly Dictionary<Type, Dictionary<Enum, bool>> _IsBuilding
        = new Dictionary<Type, Dictionary<Enum, bool>>
        {
            {typeof(WindowType), new Dictionary<Enum, bool>()},
            {typeof(ListElement), new Dictionary<Enum, bool>()}
        };
        #endregion
    }

}