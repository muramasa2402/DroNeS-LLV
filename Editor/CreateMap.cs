using static LoadingTools.MeshOptimizer;
using UnityEngine;
using UnityEditor;
using Mapbox.Unity.Map;
using System.IO;


public class CreateMap : EditorWindow {
    AbstractMap manhattanMap;
    AbstractMap brooklynMap;
    GameObject manhattan;
    GameObject brooklyn;

    [MenuItem("Window/Create Map")]
    static void Init() 
    {
        // Get existing open window or if none, make a new one:
        CreateMap sizeWindow = new CreateMap {
            autoRepaintOnSceneChange = true
        };
        sizeWindow.Show();
    }

    void OnGUI() 
    {
        if (manhattan == null) { manhattan = GameObject.Find("CitySimulatorMap"); }
        if (brooklyn == null) { brooklyn = GameObject.Find("Brooklyn"); }
        if (brooklynMap == null) { brooklynMap = brooklyn.GetComponent<AbstractMap>(); }
        if (manhattanMap == null) { manhattanMap = manhattan.GetComponent<AbstractMap>(); }

        if (GUILayout.Button("0. Set Filter Height")) 
        {
            var val = 70.0f;
            var key = manhattanMap.VectorData.GetFeatureSubLayerAtIndex(0).filterOptions.GetFilter(0).GetKey;
            manhattanMap.VectorData.GetFeatureSubLayerAtIndex(0).filterOptions.GetFilter(0).SetNumberIsGreaterThan(key, val);
            manhattanMap.VectorData.GetFeatureSubLayerAtIndex(1).filterOptions.GetFilter(0).SetNumberIsLessThan(key, val);
            manhattanMap.VectorData.GetFeatureSubLayerAtIndex(1).filterOptions.GetFilter(1).SetNumberIsEqual(key, val);
            while (manhattan.transform.childCount > 0) 
            {
                Destroy(manhattan.transform.GetChild(0).gameObject);
            }
            CreateCity(manhattanMap);
        }

        if (GUILayout.Button("1. Create Map!")) 
        {
            if (manhattan.transform.childCount == 0) 
            {
                CreateCity(manhattanMap);
                CreateCity(brooklynMap);
            }
            else { Debug.LogError("A city already exists, destroy it first!"); }
            if (manhattanMap != null && manhattanMap.enabled) manhattanMap.enabled = false;
        }

        if (GUILayout.Button("2. Setup Objects")) 
        {
            OptimizeRenderer(manhattan.transform);
        }

        if (GUILayout.Button("3. Group buildings")) 
        {
            GroupBlocks(manhattan.transform);
            SplitAllBlocks(manhattan.transform);
            for (int i = 0; i < 5; i++)
            {
                SortHeirarchy(manhattan.transform);
            }
        }

        if (GUILayout.Button("4. Combine Building Meshes")) 
        {
            foreach (Transform tile in manhattan.transform) 
            {
                for (int i = 0; i < tile.childCount; i++)
                {
                    Transform current = tile.GetChild(i);
                    if (current.name == "road")
                    {
                        DestroyImmediate(current.gameObject);
                        continue;
                    }
                    CombineParentAndChildrenMeshes(current);
                }
            }
        }

        if (GUILayout.Button("Create City Boundary"))
        {
            LoadingTools.CityBoundary.CreateBoundary();
        }

        if (GUILayout.Button("Reset Hierarchy"))
        {
            foreach (Transform tile in manhattan.transform) 
            {
                foreach (Transform building in tile) 
                {
                    while (building.childCount > 0) { building.GetChild(0).SetParent(tile, true); }
                }
            }
        }

        if (GUILayout.Button("Destroy Everything")) 
        {
            while (manhattan.transform.childCount > 0) 
            {
                DestroyImmediate(manhattan.transform.GetChild(0).gameObject);
            }
            while (brooklyn.transform.childCount > 0) 
            {
                DestroyImmediate(brooklyn.transform.GetChild(0).gameObject);
            }
        }
        if (GUILayout.Button("Count Buildings"))
        {
            int count = 0;
            foreach (Transform tile in manhattan.transform)
            {
                count += tile.childCount;
            }
            Debug.Log(count);

        }
        if (GUILayout.Button("Save Building Meshes"))
        {

        }

        //if (GUILayout.Button("Save Tile Meshes"))
        //{
        //    foreach (Transform tile in manhattan.transform)
        //    {
        //        /* Saving Meshes */
        //        Mesh mesh = tile.gameObject.GetComponent<MeshFilter>().sharedMesh;
        //        AssetDatabase.CreateFolder("Assets/Resources/Meshes/Manhattan", tile.name.Replace("/", " "));
        //        string meshPath = "Assets/Resources/Meshes/Manhattan/" + tile.name.Replace("/", " ") + "/";
        //        AssetDatabase.CreateAsset(mesh, meshPath + tile.name.Replace("/", " ") + ".asset");

        //        /* Saving Textures */
        //        Texture2D texture = tile.gameObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture as Texture2D;
        //        var data = texture.EncodeToPNG();
        //        AssetDatabase.CreateFolder("Assets/Resources/Textures/Manhattan", tile.name.Replace("/", " "));
        //        string texturePath = "/Users/bryanwong/Documents/Unity/city_v1/Assets/Resources/Textures/Manhattan/" + tile.name.Replace("/", " ") + "/";
        //        File.WriteAllBytes(texturePath + tile.name.Replace("/", " ") + ".png", data);

        //        /* Saving Materials */

        //        Material material = tile.gameObject.GetComponent<MeshRenderer>().sharedMaterial;
        //        AssetDatabase.CreateFolder("Assets/Resources/Materials/Manhattan", tile.name.Replace("/", " "));
        //        string materialPath = "Assets/Resources/Materials/Manhattan/" + tile.name.Replace("/", " ") + "/";
        //        AssetDatabase.CreateAsset(material, materialPath + tile.name.Replace("/", " ") + ".mat");
        //        AssetDatabase.SaveAssets();
        //        Debug.Log("Saving...");
        //    }
        //    PrefabUtility.SaveAsPrefabAsset(manhattan,"Assets/Resources/Prefabs/Manhattan.prefab",out bool success);
        //    if (success) 
        //    { 
        //        Debug.Log("Save Successful!");
        //    }

        //}
    }

    public static void CreateCity(AbstractMap m)
    {
        if (m.MapVisualizer != null) { m.ResetMap(); }
        else { m.MapVisualizer = CreateInstance<MapVisualizer>(); }
        m.Initialize(new Mapbox.Utils.Vector2d(40.764170691358686f, -73.97670925665614f), 16);
    }

}

