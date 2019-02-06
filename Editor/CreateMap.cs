using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Mapbox.Unity.Map;
using Mapbox.Unity;
using Mapbox.Unity.Map.TileProviders;

public class CreateMap : EditorWindow {
    AbstractMap map;
    GameObject city;
    float maxBuildingDimension;

    [MenuItem("Window/Create Map")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        CreateMap sizeWindow = new CreateMap {
            autoRepaintOnSceneChange = true
        };
        sizeWindow.Show();
    }

    void OnGUI() {
        if (city == null) { city = GameObject.Find("CitySimulatorMap"); }
        if (map == null) { map = city.GetComponent<AbstractMap>(); }
        if (map != null && map.enabled) map.enabled = false;

        if (GUILayout.Button("0. Set Filter Height")) {
            var val = 70.0f;
            Debug.Log(map.VectorData.GetFeatureSubLayerAtIndex(0).filterOptions.GetFilter(0).GetMinValue);
            var key = map.VectorData.GetFeatureSubLayerAtIndex(0).filterOptions.GetFilter(0).GetKey;
            map.VectorData.GetFeatureSubLayerAtIndex(0).filterOptions.GetFilter(0).SetNumberIsGreaterThan(key, val);
            Debug.Log(map.VectorData.GetFeatureSubLayerAtIndex(0).filterOptions.GetFilter(0).GetMinValue);
            map.VectorData.GetFeatureSubLayerAtIndex(1).filterOptions.GetFilter(0).SetNumberIsLessThan(key, val);
            map.VectorData.GetFeatureSubLayerAtIndex(1).filterOptions.GetFilter(1).SetNumberIsEqual(key, val);
        }

        if (GUILayout.Button("1. Create Map!")) {
            if (city.transform.childCount==0) {
                CreateManhattan();
            } else {
                Debug.LogError("A city already exists, destroy it first!");
            }

        }

        if (GUILayout.Button("2. Nonconvexify Road Collider")) {
            GameObject road;
            foreach (Transform tile in city.transform) {
                if (tile.childCount > 0) {
                    road = GiveRoad(tile);
                    if (road != null)
                        road.transform.localScale = new Vector3( 1, 3, 1);
                        road.GetComponent<MeshCollider>().convex = false;
                }
            }
        }

        if (GUILayout.Button("3. Rearrange Hierarchy")) {
            maxBuildingDimension = maxBuildingSize();
            
            List<Transform> buildings = new List<Transform>();
            GameObject road;
            Material [] materials = new Material[1];
            Mesh mesh;
            foreach (Transform tile in city.transform) {
                foreach (Transform i in tile) {
                    mesh = i.GetComponent<MeshFilter>().mesh;
                    materials[0] = i.GetComponent<MeshRenderer>().sharedMaterial;
                    i.GetComponent<MeshRenderer>().materials = materials;
                    mesh.SetTriangles(mesh.triangles, 0);
                    mesh.subMeshCount = 1;
                    if (i.name.Substring(0, 4) == "Tall") { buildings.Add(i); }
                }
                road = GiveRoad(tile);
                if (buildings.Count >= 2 && road != null) {
                    buildings = listSort(buildings);
                    foreach (Transform i in buildings) {
                        if (i.parent == tile) {
                            foreach (Transform j in buildings) {
                                if (j != i && j.parent == tile && IsContains(i.gameObject, j.gameObject, road)) {
                                    j.SetParent(i, true);
                                    while (j.childCount > 0) { j.GetChild(0).SetParent(i, true); }
                                }
                            }
                        }
                    }
                }
                buildings.Clear();
            }
        }

        if (GUILayout.Button("4. Delete Roads")) {
            GameObject road;
            foreach (Transform tile in city.transform) {
                if (tile.childCount > 0) {
                    road = GiveRoad(tile);
                    DestroyImmediate(road);
                }
            }
        }

        if (GUILayout.Button("5. Combine Tall Meshes")) {
            //Transform tile = city.transform.GetChild(1);
            foreach (Transform tile in city.transform) {
                CombineTallMeshes(tile);
            }
        }

        if (GUILayout.Button("Reset Hierarchy")) {
            foreach (Transform tile in city.transform) {
                foreach (Transform building in tile) {
                    while (building.childCount > 0) {
                        building.GetChild(0).SetParent(tile);
                    }
                }
            }
        }

        if (GUILayout.Button("Destroy Everything")) {
            while (city.transform.childCount > 0) {
                DestroyImmediate(city.transform.GetChild(0).gameObject);
            }
        }

        //if (GUILayout.Button("Save Meshes")) {
        //    int i = 0;
        //    Mesh mesh;
        //    foreach (Transform tile in city.transform) {
        //        foreach (Transform tall in tile) {
        //            foreach (Transform subtall in tall) {
        //                mesh = subtall.gameObject.GetComponent<MeshFilter>().sharedMesh;
        //                AssetDatabase.CreateAsset(mesh, "Assets/Meshes/" + i.ToString() + "/" + subtall.name + ".asset");
        //                AssetDatabase.SaveAssets();
        //            }
        //        }
        //        i++;
        //    }
        //}
    }

    GameObject GiveRoad(Transform tile) {
        foreach (Transform i in tile) {
            if (i.name == "road") return i.gameObject;
        }
        return null;
    }

    void CreateManhattan() {
        map.MapVisualizer = ScriptableObject.CreateInstance<MapVisualizer>();
        map.Initialize(new Mapbox.Utils.Vector2d(40.74856, -74), 16);
    }

    // Does object o contain object i?
    private bool IsContains(GameObject o, GameObject i, GameObject r) {
        var meshO = o.GetComponent<MeshCollider>();
        var meshI = i.GetComponent<MeshCollider>();
        var road = r.GetComponent<MeshCollider>();
        bool val;
        Bounds outside;
        Bounds inside;
        Ray ray;
        Vector3 dir;


        outside = new Bounds(new Vector3(meshO.bounds.center.x, road.bounds.center.y, meshO.bounds.center.z), meshO.bounds.size);
        inside = new Bounds(new Vector3(meshI.bounds.center.x, road.bounds.center.y, meshI.bounds.center.z), meshI.bounds.size);
        dir = (inside.center - outside.center).normalized;
        float dist = (inside.center - outside.center).magnitude;
        if (dist + new Vector2(inside.extents.x, inside.extents.z).magnitude + 
        new Vector2(outside.extents.x, outside.extents.z).magnitude > maxBuildingDimension) return false;
        if (dist < 3.0f) return true;

        ray = new Ray(outside.center, dir);
        val = road.Raycast(ray, out RaycastHit hit, dist);
        return !val;
    }

    private void CombineTallMeshes(Transform tile) {
        Matrix4x4 parentTrans;
        MeshFilter[] meshFilters;
        CombineInstance[] combine;
        Material[] materials;

        foreach (Transform tall in tile) {
            if (tall.childCount > 0) {

                Mesh mainMesh = tall.gameObject.transform.GetComponent<MeshFilter>().mesh;

                parentTrans = tall.gameObject.transform.worldToLocalMatrix;
                meshFilters = tall.gameObject.GetComponentsInChildren<MeshFilter>();
                combine = new CombineInstance[meshFilters.Length];

                for (int i = 0; i < meshFilters.Length; i++) {
                    combine[i].mesh = meshFilters[i].sharedMesh;
                    combine[i].transform = parentTrans * meshFilters[i].transform.localToWorldMatrix;
                }

                materials = tall.GetComponent<MeshRenderer>().sharedMaterials;
                Mesh batchedMesh = tall.gameObject.transform.GetComponent<MeshFilter>().mesh = new Mesh();
                batchedMesh.CombineMeshes(combine);
                batchedMesh.name = mainMesh.name;
                tall.gameObject.GetComponent<MeshRenderer>().materials = materials;
                tall.gameObject.GetComponent<MeshCollider>().sharedMesh = tall.gameObject.GetComponent<MeshFilter>().sharedMesh;
                
                tall.gameObject.transform.gameObject.SetActive(true);

                while (tall.childCount > 0) {
                    foreach (Transform child in tall) {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }
        }
    }

    private float maxBuildingSize() {
        float[] max = { 0, 0, 0, 0 };
        float dim = 0;
        Vector3 size;
        string bd = "name";
        foreach (Transform tile in city.transform) {
            foreach (Transform tall in tile) {
                if (tall.name.Substring(0, 4) == "Tall") {
                    size = tall.GetComponent<MeshCollider>().bounds.size;
                    dim = Mathf.Max(size.x, size.z);
                    if (dim > max[0]) {
                        max[3] = max[2];
                        max[2] = max[1];
                        max[1] = max[0];
                        max[0] = dim;
                        bd = tall.name;
                    }
                }
            }
        }
        Debug.Log(bd);
        Debug.Log(max[3]);
        return max[3];
    }

    private List<Transform> listSort(List<Transform> list) {
        Transform temp = list[1];
        for (int i = 0; i < list.Count - 1; i++) {
            for (int j = i + 1; j < list.Count; j++) {
                if (list[j].localPosition.x < temp.localPosition.x &&
                list[j].localPosition.z < temp.localPosition.z) {
                    temp = list[j];
                }
            }
            list[list.IndexOf(temp)] = list[i];
            list[i] = temp;
        }
        return list;
    }

}

