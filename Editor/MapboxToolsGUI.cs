using static Utilities.LoadingTools.MeshOptimizer;
using UnityEngine;
using UnityEditor;
using Mapbox.Unity.Map;

public class MapboxToolsGUI : EditorWindow
{
    AbstractMap abstractMap;
    GameObject citySimulatorMap;
    public float minHeight;
    public float maxHeight;

    [MenuItem("Window/Create Map")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MapboxToolsGUI sizeWindow = new MapboxToolsGUI
        {
            autoRepaintOnSceneChange = true
        };
        sizeWindow.Show();
    }

    void OnGUI()
    {
        if (citySimulatorMap == null) { citySimulatorMap = GameObject.Find("CitySimulatorMap"); }
        if (abstractMap == null) { abstractMap = citySimulatorMap.GetComponent<AbstractMap>(); }

        minHeight = EditorGUILayout.FloatField("Minimum Building Height:", minHeight);
        maxHeight = EditorGUILayout.FloatField("Maximum Building Height:", maxHeight);
        string heightFolderName = minHeight + " - " + maxHeight;

        if (GUILayout.Button("Verify Heigth Range")) { Debug.Log(heightFolderName); }

        if (GUILayout.Button("1. Create Map!"))
        {

            if (citySimulatorMap.transform.childCount == 0)
            {
                CreateCity(abstractMap);
            }
            else { Debug.LogError("A city already exists, destroy it first!"); }
            if (abstractMap != null && abstractMap.enabled) abstractMap.enabled = false;
            abstractMap.VectorData.GetFeatureSubLayerAtIndex(0).filterOptions.GetFilter(0).SetNumberIsInRange("height", minHeight, maxHeight);
            abstractMap.VectorData.GetFeatureSubLayerAtIndex(0).filterOptions.GetFilter(1).SetNumberIsEqual("height", minHeight);
        }

        if (GUILayout.Button("2. Setup Objects"))
        {
            RenameRoads(citySimulatorMap.transform);
            OptimizeRenderer(citySimulatorMap.transform);
            foreach(Transform tile in citySimulatorMap.transform)
            {
                SortChildrenByName(tile);
            }
            GroupBlocks(citySimulatorMap.transform);
            SplitAllBlocks(citySimulatorMap.transform);
            SortHeirarchy(citySimulatorMap.transform);
        }

        if (GUILayout.Button("3. Combine Building Meshes"))
        {
            Transform road = null;
            foreach (Transform tile in citySimulatorMap.transform)
            {
                for (int i = 0; i < tile.childCount; i++)
                {
                    Transform current = tile.GetChild(i);
                    if (current.name.Substring(0,4) == "Road")
                    {
                        road = current;
                        continue;
                    }
                    CombineParentAndChildrenMeshes(current, true);
                }
                if (road != null) { DestroyImmediate(road.gameObject); }
            }
        }

        if (GUILayout.Button("Destroy All Tiles"))
        {
            while (citySimulatorMap.transform.childCount > 0)
            {
                DestroyImmediate(citySimulatorMap.transform.GetChild(0).gameObject);
            }

        }
        if (GUILayout.Button("Destroy Tiles with No Buildings"))
        {
            DeleteEmptyTiles(citySimulatorMap.transform);
        }

        if (GUILayout.Button("Count Buildings"))
        {
            Debug.Log(BuildingCounter(citySimulatorMap.transform));
        }

        if (GUILayout.Button("Save Building Meshes"))
        {
            foreach (Transform tile in citySimulatorMap.transform)
            {
                string tileFolder = "Assets/Resources/Meshes/Manhattan/" + tile.name.Replace("/", " ");
                string heightFolder = tileFolder + "/" + heightFolderName;
                if (!AssetDatabase.IsValidFolder(heightFolder))
                {
                    AssetDatabase.CreateFolder(tileFolder, heightFolderName);
                }

                foreach (Transform building in tile)
                {
                    /* Saving Meshes */
                    Mesh mesh = building.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    AssetDatabase.CreateAsset(mesh, heightFolder + "/" + building.name.Replace("Tall", "Building") + ".asset");
                }
            }
        }

        if (GUILayout.Button("Save Building Prefab"))
        {
            foreach (Transform tile in citySimulatorMap.transform)
            {
                string tileFolder = "Assets/Resources/Prefabs/Manhattan/" + tile.name.Replace("/", " ");
                string heightFolder = tileFolder + "/" + heightFolderName;
                if (!AssetDatabase.IsValidFolder(heightFolder))
                {
                    AssetDatabase.CreateFolder(tileFolder, heightFolderName);
                }

                foreach (Transform building in tile)
                {
                    /* Saving prefabs */
                    GameObject go = building.gameObject;
                    PrefabUtility.SaveAsPrefabAsset(go, heightFolder + "/" + building.name.Replace("Tall", "Building") + ".prefab");
                }
            }
        }

        if (GUILayout.Button("Delete All Building"))
        {
            DeleteAllBuildings(citySimulatorMap.transform);
        }

    }

    public static void CreateCity(AbstractMap m)
    {
        if (m.MapVisualizer != null) { m.ResetMap(); }
        m.MapVisualizer = CreateInstance<MapVisualizer>();
        m.Initialize(new Mapbox.Utils.Vector2d(40.764170691358686f, -73.97670925665614f), 16);
    }

    public static int BuildingCounter(Transform city)
    {
        int count = 0;
        foreach (Transform tile in city)
        {
            foreach (Transform building in tile)
            {
                count++;
            }
        }
        return count;
    }

    public static void DeleteAllBuildings(Transform city)
    {
        foreach (Transform tile in city)
        {
            while (tile.childCount > 0)
            {
                DestroyImmediate(tile.GetChild(0).gameObject);
            }
        }
    }

    public static void DeleteEmptyTiles(Transform city)
    {
        for (int i = city.childCount - 1; i >= 0; i--)
        {
            Transform tile = city.GetChild(i);
            if (tile.childCount == 0) { DestroyImmediate(tile.gameObject); }
        }
    }

    private static void RenameRoads(Transform city)
    {
        foreach(Transform tile in city)
        {
            foreach (Transform component in tile)
            {
                GameObjectUtility.SetStaticEditorFlags(component.gameObject, StaticEditorFlags.BatchingStatic);
                if (component.name == "road")
                {
                    component.name = "Road - " + tile.name.Replace("/", " ");
                }
            }
        }
    }

}

