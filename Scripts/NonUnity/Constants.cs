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
        public const string DroneListElementPath = WindowPrefabPath + "/Drone/DroneListTuple";
        public const string HubListElementPath = WindowPrefabPath + "/Hub/HubListTuple";
        public const string JobHistoryElementPath = WindowPrefabPath + "/Job/JobHistoryTuple";
        public const string JobQueueElementPath = WindowPrefabPath + "/Job/QueueElement";
        public const string PositionHighlightPath = "Prefabs/UI/PositionHighlight";
    }
}