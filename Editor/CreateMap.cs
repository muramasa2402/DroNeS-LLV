using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Mapbox.Unity.Map;
using Mapbox.Unity;
using Mapbox.Unity.Map.TileProviders;
using UnityEngine.Rendering;
using Mapbox.Map;
using Mapbox.Unity.MeshGeneration.Data;

public class CreateMap : EditorWindow {
    AbstractMap map;
    AbstractMap brooklynMap;
    AbstractMap mapShape;
    GameObject city;
    GameObject brooklyn;
    GameObject boundary;
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
        if (brooklyn == null) { brooklyn = GameObject.Find("Brooklyn"); }
        if (boundary == null) { boundary = GameObject.Find("Boundary"); }
        if (mapShape == null) { mapShape = boundary.GetComponent<AbstractMap>(); }
        if (brooklynMap == null) { brooklynMap = brooklyn.GetComponent<AbstractMap>(); }
        if (map == null) { map = city.GetComponent<AbstractMap>(); }

        if (GUILayout.Button("0. Set Filter Height")) {
            var val = 70.0f;
            var key = map.VectorData.GetFeatureSubLayerAtIndex(0).filterOptions.GetFilter(0).GetKey;
            map.VectorData.GetFeatureSubLayerAtIndex(0).filterOptions.GetFilter(0).SetNumberIsGreaterThan(key, val);
            map.VectorData.GetFeatureSubLayerAtIndex(1).filterOptions.GetFilter(0).SetNumberIsLessThan(key, val);
            map.VectorData.GetFeatureSubLayerAtIndex(1).filterOptions.GetFilter(1).SetNumberIsEqual(key, val);
        }

        if (GUILayout.Button("1. Create Map!")) {
            if (city.transform.childCount == 0) {
                CreateCity(map);
                CreateCity(brooklynMap);
                CreateCity(mapShape);
                while (boundary.transform.childCount > 1) {
                    boundary.transform.GetChild(1).SetParent(boundary.transform.GetChild(0));
                }
                CombineTallMeshes(boundary.transform);
                foreach (Transform child in boundary.transform) {
                    DestroyImmediate(child.GetComponent<MeshRenderer>());
                    DestroyImmediate(child.GetComponent<UnityTile>());
                }
            }
            else { Debug.LogError("A city already exists, destroy it first!"); }
            if (map != null && map.enabled) map.enabled = false;
        }

        if (GUILayout.Button("2. Setup Objects")) {
            Debug.Log(city.transform.childCount);
            Material[] materials = new Material[1];
            materials[0] = Resources.Load("Materials/BuildingMaterial") as Material;
            Mesh mesh;
            MeshRenderer meshRenderer;


            foreach (Transform tile in city.transform) {
                meshRenderer = tile.GetComponent<MeshRenderer>();
                meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                meshRenderer.allowOcclusionWhenDynamic = true;
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                GameObjectUtility.SetStaticEditorFlags(tile.gameObject, StaticEditorFlags.BatchingStatic |
                StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic);


                if (tile.childCount > 0) {
                    foreach (Transform i in tile) {
                        if (i.name == "road") {
                            i.GetComponent<MeshCollider>().convex = false;
                        } else {
                            GameObjectUtility.SetStaticEditorFlags(i.gameObject, StaticEditorFlags.BatchingStatic |
                            StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic);
                            mesh = i.GetComponent<MeshFilter>().mesh;
                            mesh.SetTriangles(mesh.triangles, 0);
                            mesh.subMeshCount = 1;
                            meshRenderer = i.GetComponent<MeshRenderer>();
                            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                            meshRenderer.materials = materials;
                        }
                    }
                }
            }
        }


        if (GUILayout.Button("3. Rearrange Hierarchy")) {
            maxBuildingDimension = MaxBuildingSize();
            List<Transform> tallBuildings = new List<Transform>();
            List<Transform> shortBuidlings = new List<Transform>();
            GameObject road = null;
            foreach (Transform tile in city.transform) {
                /* Preprocess: putting all the buildings in a 'static' list
                since hierarchy changes as we go through the loop */
                foreach (Transform i in tile) {
                    if (i.name.Substring(0, 4) == "Tall") {
                        i.GetComponent<MeshCollider>().convex = true;
                        tallBuildings.Add(i);
                    } else if (i.name != "road") { shortBuidlings.Add(i); }
                    else { road = i.gameObject; }
                }
                /* Begin grouping tall buildins */
                if (tallBuildings.Count >= 2 && road != null) {
                    tallBuildings = ListSort(tallBuildings);
                    foreach (Transform i in tallBuildings) {
                        if (i.parent == tile) {
                            foreach (Transform j in tallBuildings) {
                                if (j != i && j.parent == tile && IsContains(i.gameObject, j.gameObject, road)) {
                                    j.SetParent(i, true);
                                    while (j.childCount > 0) { j.GetChild(0).SetParent(i, true); }
                                }
                            }
                        }
                    }
                }
                tallBuildings.Clear();

                if (shortBuidlings.Count > 0) {
                    int num = 1;
                    foreach (Transform i in shortBuidlings) {
                        i.name = "Short - " + num++;
                    }
                    shortBuidlings.Clear();
                }
            }
        }

        if (GUILayout.Button("4. Sort Children")) {
            SortChildrenByHeight();
            VertexLimit();
        }

        if (GUILayout.Button("5. Combine Tall Meshes")) {
            GameObject road;
            foreach (Transform tile in city.transform) {
                if (tile.childCount > 0) {
                    road = GiveRoad(tile);
                    DestroyImmediate(road);
                }
                CombineTallMeshes(tile);
            }
        }

        if (GUILayout.Button("6. Bake Occlusion")) {
            StaticOcclusionCulling.backfaceThreshold = 70;
            StaticOcclusionCulling.smallestHole = 1;
            StaticOcclusionCulling.smallestOccluder = 15;
            StaticOcclusionCulling.Clear();
            StaticOcclusionCulling.GenerateInBackground();
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
            while (brooklyn.transform.childCount > 0) {
                DestroyImmediate(brooklyn.transform.GetChild(0).gameObject);
            }
            while (boundary.transform.childCount > 0) {
                DestroyImmediate(boundary.transform.GetChild(0).gameObject);
            }
        }

        if (GUILayout.Button("Save Meshes")) {
            //int i = 0;
            //Mesh mesh;
            //foreach (Transform tile in city.transform) {
            //    foreach (Transform tall in tile) {
            //        foreach (Transform subtall in tall) {
            //            mesh = subtall.gameObject.GetComponent<MeshFilter>().sharedMesh;
            //            AssetDatabase.CreateAsset(mesh, "Assets/Meshes/" + i.ToString() + "/" + subtall.name + ".asset");
            //            AssetDatabase.SaveAssets();
            //        }
            //    }
            //    i++;
            //}

        }
    }

    GameObject GiveRoad(Transform tile) {
        foreach (Transform i in tile) {
            if (i.name == "road") return i.gameObject;
        }
        return null;
    }

    private void CreateCity(AbstractMap m) {
        m.MapVisualizer = ScriptableObject.CreateInstance<MapVisualizer>();
        m.Initialize(new Mapbox.Utils.Vector2d(40.74856, -74), 16);
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
                batchedMesh.name = tall.name;
                tall.gameObject.GetComponent<MeshRenderer>().materials = materials;
                tall.gameObject.GetComponent<MeshCollider>().sharedMesh = tall.gameObject.GetComponent<MeshFilter>().sharedMesh;

                tall.gameObject.transform.gameObject.SetActive(true);

                while (tall.childCount > 0) {
                    DestroyImmediate(tall.GetChild(0).gameObject);
                }
            }
        }
    }

    private float MaxBuildingSize() {
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
        return max[3];
    }
   

    private List<Transform> ListSort(List<Transform> list) {
        int minIndex = 1;
        Transform mini = list[minIndex];
        float min = Vector3.SqrMagnitude(list[1].position - list[0].position);
        list.Sort((Transform t1, Transform t2) => { return string.Compare(t1.name, t2.name, System.StringComparison.Ordinal); });

        return list;
    }

    private void SortChildrenByHeight() {
        Transform low;
        List<Transform> parents = new List<Transform>();
        foreach (Transform tile in city.transform) {
            // Makes sure the parents are the lowest structure
            foreach (Transform tall in tile) {
                if (tall.name.Substring(0,4) == "Tall") {
                    parents.Add(tall);
                }
            }

            foreach (Transform tall in parents) {
                low = tall;
                if (tall.childCount == 0) continue;

                foreach (Transform child in tall) {
                    if (child.GetComponent<MeshCollider>().bounds.center.y <
                    low.GetComponent<MeshCollider>().bounds.center.y) {
                        low = child;
                    }
                }
                if (tall != low) {
                    low.SetParent(tile);
                    while (tall.childCount > 0) {
                        tall.GetChild(0).SetParent(low);
                    }
                    tall.SetParent(low);
                }
                int min = 0;
                for (int i = 0; i < low.childCount - 1; i++) {
                    for (int j = i + 1; j < low.childCount; j++) {
                        if (low.GetChild(j).GetComponent<MeshCollider>().bounds.center.y <
                            low.GetChild(min).GetComponent<MeshCollider>().bounds.center.y) {
                            min = j;
                        }
                    }
                    low.GetChild(min).SetSiblingIndex(i);
                }
            }
            parents.Clear();
        }
    }
    // Limits combined mesh vertex count to 65000 (Unity limit)
    private void VertexLimit() {
        int vertices;
        bool repeat = false; ;
        MeshFilter meshFilter;
        List<Transform> parents = new List<Transform>();
        foreach (Transform tile in city.transform) {
            // Makes sure the parents are the lowest structure
            foreach (Transform tall in tile) {
                if (tall.name.Substring(0, 4) == "Tall") {
                    parents.Add(tall);
                }
            }
            for (int j = 0; j < parents.Count; j++) {
                Transform tall = parents[j];
                 do {
                    repeat = false;
                    vertices = tall.GetComponent<MeshFilter>().sharedMesh.vertexCount;
                    int children = tall.childCount;
                    for (int i = 0; i < children; i++) {
                        meshFilter = tall.GetChild(i).GetComponent<MeshFilter>();
                        vertices += meshFilter.sharedMesh.vertexCount;
                        if (vertices > 65000) {
                            parents.Add(SplitChildren(tall, i));
                            repeat = true;
                            break;
                        }
                    }

                } while (repeat) ;

            }

            parents.Clear();
        }
    }
    // Splits the children from index onwards into a new parent in the hierarchy
    private Transform SplitChildren(Transform tall, int index) {
        Transform newTall = tall.GetChild(index);
        int i = index + 1;
        while (i < tall.childCount) {
            tall.GetChild(i).SetParent(tall.GetChild(index));
        }
        tall.GetChild(index).SetParent(tall.parent);
        return newTall;
    }

}

