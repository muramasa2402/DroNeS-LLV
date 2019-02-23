using UnityEngine;
using UnityEditor;
using Utilities.LoadingTools;

public class PrefabToolsGUI : EditorWindow
{
    GameObject manhattan;
    public float minHeight = 0;
    public float maxHeight = 30;
    readonly string cityFolder = "Assets/Resources/Prefabs/Manhattan";
    [MenuItem("Window/Prefab Tools")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        PrefabToolsGUI sizeWindow = new PrefabToolsGUI
        {
            autoRepaintOnSceneChange = true
        };
        sizeWindow.Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Test Build"))
        {
            CityBuilder.OptimiseCity(CityBuilder.BuildManhattan(), 41.0f, true);
        }

        if (GUILayout.Button("Instantiate Manhattan"))
        {
            string path = cityFolder + "/Manhattan.prefab";
            manhattan = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            manhattan = (GameObject)PrefabUtility.InstantiatePrefab(manhattan);
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
            foreach (Transform tile in manhattan.transform)
            {
                string tilePath = cityFolder + "/" + tile.name.Replace("/", " ");
                tilePath += "/" + tile.name.Replace("/", " ") + ".prefab";

                PrefabUtility.SaveAsPrefabAsset(tile.gameObject, tilePath);
            }
        }

        if (GUILayout.Button("Delete All Buildings"))
        {
            MapboxToolsGUI.DeleteAllBuildings(manhattan.transform);
        }

        if (GUILayout.Button("Unpack Completely"))
        {
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

        if (GUILayout.Button("Destroy Tiles with No Buildings"))
        {
            MapboxToolsGUI.DeleteEmptyTiles(manhattan.transform);
        }

        minHeight = EditorGUILayout.FloatField("Minimum Building Height:", minHeight);
        maxHeight = EditorGUILayout.FloatField("Maximum Building Height:", maxHeight);
        string heightFolderName = minHeight + " - " + maxHeight;
        if (GUILayout.Button("Verify Height Folder Name"))
        {
            Debug.Log(heightFolderName);
        }

        if (GUILayout.Button("Load Buildings From Height Folder"))
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
                    building.transform.SetParent(tile);
                    building.transform.localPosition = building.transform.position;
                }
            }
        }

    }
}

