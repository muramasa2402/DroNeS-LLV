using UnityEngine;
using UnityEditor;
using Mapbox.Unity.Map;
using System.IO;
using System.Collections.Generic;

namespace Utilities
{
    public class PrefabTools : EditorWindow
    {
        GameObject manhattan;
        readonly static string cityFolder = "Assets/Resources/Prefabs/Manhattan";
        public float minHeight = 0;
        public float maxHeight = 30;

        [MenuItem("Window/Prefab Tools")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            PrefabTools sizeWindow = new PrefabTools
            {
                autoRepaintOnSceneChange = true
            };
            sizeWindow.Show();
        }

        void OnGUI()
        {
            if (GUILayout.Button("Instantiate Manhattan"))
            {
                string path = cityFolder + "/Manhattan.prefab";
                manhattan = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                manhattan = (GameObject)PrefabUtility.InstantiatePrefab(manhattan);
            }

            if (GUILayout.Button("Save Manhattan as Prefab"))
            {
                manhattan = GameObject.Find("Manhattan");
                string path = cityFolder + "/Manhattan.prefab";
                PrefabUtility.SaveAsPrefabAsset(manhattan, path);
            }

            if (GUILayout.Button("Build Manhattan from Tiles"))
            {
                GameObject city = new GameObject { name = "Manhattan" };
                city.transform.position = Vector3.zero;
                GameObject reference = GameObject.Find("CitySimulatorMap");
                foreach (Transform tile in reference.transform)
                {
                    string tilePath = cityFolder + "/" + tile.name.Replace("/", " ");
                    tilePath += "/" + tile.name.Replace("/", " ") + ".prefab";
                    GameObject tileObject = AssetDatabase.LoadAssetAtPath(tilePath, typeof(GameObject)) as GameObject;
                    tileObject = (GameObject)PrefabUtility.InstantiatePrefab(tileObject);
                    tileObject.transform.SetParent(city.transform);
                }
            }


            if (GUILayout.Button("Save Tiles as Prefab"))
            {
                for (int i = 0; i < manhattan.transform.childCount; i++)
                {
                    Transform tile = manhattan.transform.GetChild(i);
                    string tilePath = cityFolder + "/" + tile.name.Replace("/", " ");
                    tilePath += "/" + tile.name.Replace("/", " ") + ".prefab";

                    PrefabUtility.SaveAsPrefabAsset(tile.gameObject, tilePath);
                }
            }

            if (GUILayout.Button("Delete All Buildings"))
            {
                MapboxTools.DeleteAllBuildings(manhattan.transform);
            }
            if (GUILayout.Button("Unpack Completely")) {
                PrefabUtility.UnpackPrefabInstance(manhattan, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
            if (GUILayout.Button("Unpack Tiles"))
            {
                foreach (Transform tile in manhattan.transform)
                {
                    PrefabUtility.UnpackPrefabInstance(tile.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                }

            }
            if (GUILayout.Button("Unpack Root"))
            {
                PrefabUtility.UnpackPrefabInstance(manhattan, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }
            if (GUILayout.Button("Delete Manhattan"))
            {
                DestroyImmediate(manhattan);
            }

            minHeight = EditorGUILayout.FloatField("Minimum Building Height:", minHeight);
            maxHeight = EditorGUILayout.FloatField("Maximum Building Height:", maxHeight);
            string heightFolderName = minHeight + " - " + maxHeight;
            if (GUILayout.Button("Print Height Folder Name"))
            {
                Debug.Log(heightFolderName);
            }

            if (GUILayout.Button("Load Unpacked Buildings From Height Folder"))
            {
                if (manhattan == null) 
                {
                    Debug.LogError("No City Found!");
                    return;
                }

                foreach (Transform tile in manhattan.transform)
                {
                    string loadPath = "Prefabs/Manhattan/" + tile.name.Replace("/", " ") + "/" + heightFolderName;
                    Object[] buildings = Resources.LoadAll(loadPath, typeof(GameObject));
                    GameObject building = null;

                    for (int j = 0; j < buildings.Length; j++)
                    {
                        building = (GameObject)buildings[j];
                        building = (GameObject)PrefabUtility.InstantiatePrefab(building);
                        PrefabUtility.UnpackPrefabInstance(building, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                        building.transform.SetParent(tile);
                        building.transform.localPosition = building.transform.position;
                    }
                }
            }

        }
    }
}

