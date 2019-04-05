using System;
using System.Collections.Generic;

namespace Drones.Utils
{
    public static class Constants
    {
        public const float EPSILON = 1e-5f;
        public const float unityTileSize = 150f;
        private const float latitude = 40.764170691358686f;
        private const float longitude = -73.97670925665614f;
        public static float[] OriginCoordinates { get; } = { latitude, longitude };
        public const int mapboxZoom = 16;
        public const int LODLayer = 12;
        public const float R = 6378.137f; // Radius of earth in KM
        public const float CoroutineTimeLimit = 1 / 100f;

        public const string mapStyle = "mapbox://styles/jw5514/cjr5l685g4u4z2sjxfdupnl8b";
        public const string buildingMaterialPath = "Materials/WhiteBuilding";
        public const string WindowPrefabPath = "Prefabs/UI/Windows";
        public const string DroneWindowPath = WindowPrefabPath + "/Drone/Drone Window";
        public const string DroneListWindowPath = WindowPrefabPath + "/Drone/DroneList Window";
        public const string NavigationWindowPath = WindowPrefabPath + "/Navigation/Navigation Window";
        public const string HubWindowPath = WindowPrefabPath + "/Hub/Hub Window";
        public const string HubListWindowPath = WindowPrefabPath + "/Hub/HubList Window";
        public const string JobWindowPath = WindowPrefabPath + "/Job/Job Window";
        public const string JobHistoryWindowPath = WindowPrefabPath + "/Job/JobHistory Window";
        public const string JobQueueWindowPath = WindowPrefabPath + "/Job/JobQueue Window";
        public const string DroneListTuplePath = WindowPrefabPath + "/Drone/DroneListTuple";
        public const string HubListTuplePath = WindowPrefabPath + "/Hub/HubListTuple";
        public const string JobHistoryTuplePath = WindowPrefabPath + "/Job/JobHistoryTuple";
        public const string JobQueueTuplePath = WindowPrefabPath + "/Job/JobQueueTuple";
        public const string ConsoleElementPath = WindowPrefabPath + "/Console/ConsoleElement";
        public const string PositionHighlightPath = "Prefabs/UI/PositionHighlight";
        public const string WaypointPath = "Prefabs/UI/Waypoint";

        private static Dictionary<Type, Dictionary<Enum, string>> _Paths;
        public static Dictionary<Type, Dictionary<Enum, string>> PrefabPaths
        {
            get
            {
                if (_Paths == null)
                {
                    _Paths = new Dictionary<Type, Dictionary<Enum, string>>
                    {
                        {typeof(WindowType), new Dictionary<Enum, string>()},
                        {typeof(ListElement), new Dictionary<Enum, string>()},
                        {typeof(PositionHighlight), new Dictionary<Enum, string>()}
                    };

                    _Paths[typeof(WindowType)].Add(WindowType.Drone, DroneWindowPath);
                    _Paths[typeof(WindowType)].Add(WindowType.DroneList, DroneListWindowPath);
                    _Paths[typeof(WindowType)].Add(WindowType.Hub, HubWindowPath);
                    _Paths[typeof(WindowType)].Add(WindowType.HubList, HubListWindowPath);
                    _Paths[typeof(WindowType)].Add(WindowType.Job, JobWindowPath);
                    _Paths[typeof(WindowType)].Add(WindowType.JobQueue, JobQueueWindowPath);
                    _Paths[typeof(WindowType)].Add(WindowType.JobHistory, JobHistoryWindowPath);

                    _Paths[typeof(ListElement)].Add(ListElement.Console, ConsoleElementPath);
                    _Paths[typeof(ListElement)].Add(ListElement.DroneList, DroneListTuplePath);
                    _Paths[typeof(ListElement)].Add(ListElement.HubList, HubListTuplePath);
                    _Paths[typeof(ListElement)].Add(ListElement.JobQueue, JobQueueTuplePath);
                    _Paths[typeof(ListElement)].Add(ListElement.JobHistory, JobHistoryTuplePath);

                    _Paths[typeof(PositionHighlight)].Add(PositionHighlight.Normal, PositionHighlightPath);
                    _Paths[typeof(PositionHighlight)].Add(PositionHighlight.Waypoint, WaypointPath);
                }

                return _Paths;
            }
        }

    }
}