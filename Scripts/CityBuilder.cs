using UnityEngine;namespace Utilities.LoadingTools
{
    using static MeshOptimizer;
    public class CityBuilder : MonoBehaviour
    {
        private static readonly string cityFolder = "Prefabs/Manhattan";

        public static GameObject BuildManhattan()
        {
            string path = cityFolder + "/Manhattan";
            GameObject manhattan = Resources.Load(path, typeof(GameObject)) as GameObject;
            manhattan = Instantiate(manhattan);
            manhattan.name = "TestManhattan";

            return manhattan;
        }
        //TODO Serialize minimumCruisingHeight and make a UI field to input in feet or meters
        public static GameObject OptimiseCity(GameObject city, float minimumCruisingHeight, bool editMode = false)
        {
            OptimizeRenderer(city.transform);
            foreach (Transform tile in city.transform)
            {
                SortChildrenByName(tile);
            }
            SeparateShortAndTallBuildings(city, ScaleLength(minimumCruisingHeight), editMode);
            GroupBlocks(city.transform);
            SplitAllBlocks(city.transform);
            SortHeirarchy(city.transform);

            Transform road = null;
            foreach (Transform tile in city.transform)
            {
                foreach (Transform building in tile)
                {
                    if (building.name.Substring(0, 4) == "Road")
                    {
                        road = building;
                        continue;
                    }
                    CombineParentAndChildrenMeshes(building, true);
                }

                if (road != null)
                {
                    if (editMode) { DestroyImmediate(road.gameObject); }
                    else { Destroy(road.gameObject); }
                }
            }

            return city;
        }

        private static float ScaleLength(float realHeight)
        {
            return Constants.unityTileSize / Constants.realWorldTileSize * realHeight;
        }        public static float FeetToMeters(float feet)
        {
            return feet * Constants.metersPerFeet;
        }

    }}