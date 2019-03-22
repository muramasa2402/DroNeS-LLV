namespace Drones.Utils
{
    public static class Constants
    {
        public const float EPSILON = 1e-5f;
        public const float unityTileSize = 150f;
        public const float realWorldTileSize = 463.2582f;
        public const float metersPerFeet = 0.0254f * 12.0f;
        public const string mapStyle = "mapbox://styles/jw5514/cjr5l685g4u4z2sjxfdupnl8b";
        public const string buildingMaterialPath = "Materials/BuildingMaterial";
        public static readonly float[] latLong = { 40.764170691358686f, -73.97670925665614f };
        public const int mapboxZoom = 16;
        public const int TallLODLayer = 12;
        public const int ShortLODLayer = 14;

    }
    public enum Building { Short, Tall, Road }
    public enum Elevation { Flat, Real }
}