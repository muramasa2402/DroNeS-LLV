using static LoadingTools.MeshOptimizer;
using UnityEngine;
using UnityEditor;
using Mapbox.Unity.Map;
using System.IO;
using System.Collections.Generic;

namespace Utilities
{
    public class MapboxTools : EditorWindow
    {
        AbstractMap abstractMap;
        GameObject citySimulatorMap;
        float minHeight;
        float maxHeight;

        [MenuItem("Window/Create Map")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            MapboxTools sizeWindow = new MapboxTools
            {
                autoRepaintOnSceneChange = true
            };
            sizeWindow.Show();
        }

        void OnGUI()
        {
            if (citySimulatorMap == null) { citySimulatorMap = GameObject.Find("CitySimulatorMap"); }
            if (abstractMap == null) { abstractMap = citySimulatorMap.GetComponent<AbstractMap>(); }

            if (GUILayout.Button("1. Create Map!"))
            {

                if (citySimulatorMap.transform.childCount == 0)
                {
                    CreateCity(abstractMap);
                }
                else { Debug.LogError("A city already exists, destroy it first!"); }
                if (abstractMap != null && abstractMap.enabled) abstractMap.enabled = false;
                minHeight = abstractMap.VectorData.GetFeatureSubLayerAtIndex(0).filterOptions.GetFilter(0).GetMinValue;
                maxHeight = abstractMap.VectorData.GetFeatureSubLayerAtIndex(0).filterOptions.GetFilter(0).GetMaxValue;
            }

            string heightFolderName = minHeight + " - " + maxHeight;
            if (GUILayout.Button("Print Height Folder Name")) { Debug.Log(heightFolderName); }

            if (GUILayout.Button("2. Setup Objects"))
            {
                OptimizeRenderer(citySimulatorMap.transform);
            }

            if (GUILayout.Button("3. Group buildings"))
            {
                GroupBlocks(citySimulatorMap.transform);
                SplitAllBlocks(citySimulatorMap.transform);
                SortHeirarchy(citySimulatorMap.transform);
            }

            if (GUILayout.Button("4. Combine Building Meshes"))
            {
                Transform road = null;
                foreach (Transform tile in citySimulatorMap.transform)
                {
                    for (int i = 0; i < tile.childCount; i++)
                    {
                        Transform current = tile.GetChild(i);
                        if (current.name == "road")
                        {
                            road = current;
                            continue;
                        }
                        CombineParentAndChildrenMeshes(current);
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

    }

}

