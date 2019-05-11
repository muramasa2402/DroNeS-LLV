using System;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Utils
{
    using UI;
    using Interface;

    public class ListElementPool : AbstractPool, IPool
    {
        private static ListElementPool _Instance;
        public static ListElementPool Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new ListElementPool();
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
                        { typeof(ConsoleLog), ConsoleElementPath },
                        { typeof(DroneListWindow), DroneListTuplePath },
                        { typeof(RetiredDroneListWindow), RetiredDroneListTuplePath },
                        { typeof(HubListWindow), HubListTuplePath },
                        { typeof(JobQueueWindow), JobQueueTuplePath },
                        { typeof(JobHistoryWindow), JobHistoryTuplePath },
                        { typeof(NoFlyZoneListWindow), NoFlyZoneTuplePath},
                        { typeof(SaveLoadWindow), SaveLoadTuplePath}
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
                        { typeof(ConsoleLog), 500 },
                        { typeof(DroneListWindow), 200 },
                        { typeof(RetiredDroneListWindow), 200 },
                        { typeof(HubListWindow), 50 },
                        { typeof(JobQueueWindow), 200 },
                        { typeof(JobHistoryWindow), 200 },
                        { typeof(NoFlyZoneListWindow), 50 },
                        { typeof(SaveLoadWindow), 200 }
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
                        { typeof(ConsoleLog), new Queue<IPoolable>() },
                        { typeof(DroneListWindow), new Queue<IPoolable>() },
                        { typeof(RetiredDroneListWindow), new Queue<IPoolable>() },
                        { typeof(HubListWindow), new Queue<IPoolable>() },
                        { typeof(JobQueueWindow), new Queue<IPoolable>() },
                        { typeof(JobHistoryWindow), new Queue<IPoolable>() },
                        { typeof(NoFlyZoneListWindow), new Queue<IPoolable>() },
                        { typeof(SaveLoadWindow), new Queue<IPoolable>() }
                    };
                }

                return _Pool;
            }
        }

        public Dictionary<Type, bool> IsBuilding { get; } = new Dictionary<Type, bool>();

        private const string WindowPrefabPath = "Prefabs/UI/Windows";
        /* List Elements */
        public const string DroneListTuplePath = WindowPrefabPath + "/Drone/DroneListTuple";
        public const string RetiredDroneListTuplePath = WindowPrefabPath + "/Drone/RetiredDroneListTuple";
        public const string HubListTuplePath = WindowPrefabPath + "/Hub/HubListTuple";
        public const string JobHistoryTuplePath = WindowPrefabPath + "/Job/JobHistoryTuple";
        public const string JobQueueTuplePath = WindowPrefabPath + "/Job/JobQueueTuple";
        public const string NoFlyZoneTuplePath = WindowPrefabPath + "/NoFlyZone/NoFlyZoneListTuple";
        public const string ConsoleElementPath = WindowPrefabPath + "/Console/ConsoleElement";
        public const string SaveLoadTuplePath = WindowPrefabPath + "/SaveLoad/SaveLoadTuple";
    }
}
