using System.IO;
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
using Utilities;

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
            Material[] materials = new Material[1];
            materials[0] = Resources.Load("Materials/BuildingMaterial") as Material;
            Mesh mesh;
            MeshRenderer meshRenderer;

            foreach (Transform tile in manhattan.transform) 
            {
                meshRenderer = tile.GetComponent<MeshRenderer>();
                meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                meshRenderer.allowOcclusionWhenDynamic = true;
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                //GameObjectUtility.SetStaticEditorFlags(tile.gameObject, StaticEditorFlags.BatchingStatic |
                //StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic);


                if (tile.childCount > 0) {
                    foreach (Transform i in tile) 
                    {
                        if (i.name == "road") 
                        {
                            i.GetComponent<MeshCollider>().convex = false;
                        } 
                        else
                        {
                            //GameObjectUtility.SetStaticEditorFlags(i.gameObject, StaticEditorFlags.BatchingStatic |
                            //StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic);
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

        if (GUILayout.Button("3. Group buildings by city block")) 
        {
            List<Transform> tallBuildings = new List<Transform>();
            List<Transform> shortBuidlings = new List<Transform>();
            MinHeap<Transform> sorter = new MinHeap<Transform>( (Transform t1, Transform t2) => { 
            return string.Compare(t1.name, t2.name, System.StringComparison.Ordinal); } );

            GameObject road = null;
            foreach (Transform tile in manhattan.transform) {
                /* Preprocess: putting all the buildings in a list
                since hierarchy changes as we go through the loop */
                foreach (Transform i in tile) 
                {
                    if (IsTall(i)) 
                    {
                        i.GetComponent<MeshCollider>().convex = true;
                        sorter.Add(i);
                    } 
                    else if (i.name != "road") { shortBuidlings.Add(i); }
                    else { road = i.gameObject; }
                }
                /* Sorts the buildings by name */
                while (sorter.size > 0) { tallBuildings.Add(sorter.Remove()); }

                if (tallBuildings.Count >= 2 && road != null) {
                    /* Begin grouping tall buildings by city block */
                    foreach (Transform i in tallBuildings) 
                    {
                        if (i.parent != tile) { continue; }

                        foreach (Transform j in tallBuildings) 
                        {
                            if (j != i && InBlock(i.gameObject, j.gameObject, road)) 
                            {
                                if (j.parent == tile)
                                {
                                    j.SetParent(i, true);
                                    while (j.childCount > 0) { j.GetChild(0).SetParent(i, true); }
                                }
                                else if (j.parent != i)
                                {
                                    Transform k = j.parent;
                                    k.SetParent(i, true);
                                    while (k.childCount > 0) { k.GetChild(0).SetParent(i, true); }
                                }

                            }
                        }
                        
                    }
                }
                tallBuildings.Clear();
                sorter.Clear();

                if (shortBuidlings.Count > 0) 
                {
                    int num = 1;
                    foreach (Transform i in shortBuidlings) { i.name = "Short - " + num++; }
                    shortBuidlings.Clear();
                }
            }
        }

        if (GUILayout.Button("4. Split buildings within city blocks"))
        {
            foreach (Transform tile in manhattan.transform) 
            {
                for (int i = tile.childCount - 1; i >= 0 ; i--)
                {
                    Transform block = tile.GetChild(i);
                    if (block.childCount > 0) { SplitBlock(block); }
                }
            }
        }

        if (GUILayout.Button("5. Sort Children")) {
            foreach (Transform tile in manhattan.transform)
            {
                for (int i = 0; i < tile.childCount; i++)
                {
                    Transform building = tile.GetChild(i);
                    if (IsTall(building) && building.childCount != 0)
                    {
                        Transform parent = SortChildrenByHeight(building);
                        VertexLimit(parent);
                    }
                }

            }
        }

        if (GUILayout.Button("6. Combine Tall Meshes")) 
        {
            GameObject road;
            foreach (Transform tile in manhattan.transform) 
            {
                if (tile.childCount > 0) 
                {
                    road = GiveRoad(tile);
                    DestroyImmediate(road);
                }
                CombineMeshesInTile(tile);
            }
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

        //if (GUILayout.Button("Save Meshes")) {
        //int i = 0;
        //Mesh mesh;
        //foreach (Transform tile in city.transform) {
        //    /* Saving Meshes */
        //    mesh = tile.gameObject.GetComponent<MeshFilter>().sharedMesh;
        //    AssetDatabase.CreateAsset(mesh, "Assets/" + tile.name.Replace("/"," ") + ".asset");
        //    AssetDatabase.SaveAssets();
        //    /* Saving Textures */
        //    {
        //        Texture2D texture = tile.gameObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture as Texture2D;
        //        var data = texture.EncodeToPNG();
        //        string path = "/Users/bryanwong/Documents/Unity/city_v1/Assets/";
        //        File.WriteAllBytes(path + tile.name.Replace("/", " ") + ".png", data);
        //    }

        //    i++;
        //}
        //}
    }

    bool IsTall(Transform building)
    {
        return building.name.Substring(0, 4) == "Tall";
    }

    GameObject GiveRoad(Transform tile) 
    {
        foreach (Transform i in tile) 
        {
            if (i.name == "road") { return i.gameObject; }
        }
        return null;
    }

    public static void CreateCity(AbstractMap m) 
    {
        m.MapVisualizer = ScriptableObject.CreateInstance<MapVisualizer>();
        m.Initialize(new Mapbox.Utils.Vector2d(40.74856, -74), 16);
    }

    // are building i and building o in the same city block where road is the road object in the tile
    private bool InBlock(GameObject blockParent, GameObject i, GameObject road) 
    {
        var meshO = blockParent.GetComponent<MeshRenderer>();
        var meshI = i.GetComponent<MeshRenderer>();
        var roadMesh = road.GetComponent<MeshCollider>();
        float roadLevel = road.GetComponent<MeshRenderer>().bounds.center.y;
        Bounds oBounds = meshO.bounds;
        Bounds iBounds = meshI.bounds;
        Vector3 dir;

        oBounds.center = new Vector3(oBounds.center.x, roadLevel, oBounds.center.z);
        iBounds.center = new Vector3(iBounds.center.x, roadLevel, iBounds.center.z);
        dir = (iBounds.center - oBounds.center).normalized;

        // Horizontal distance between centres
        float dist = (iBounds.center - oBounds.center).magnitude;

        return dist < 2.0f || !roadMesh.Raycast(new Ray(oBounds.center, dir), out RaycastHit hit, dist);
    }

    // Does building i and building o overlap (horizontally)
    private bool IsAreaOverlap(GameObject o, GameObject i) 
    {
        var meshO = o.GetComponent<MeshRenderer>();
        var meshI = i.GetComponent<MeshRenderer>();

        Vector3 iTranslate = new Vector3(0, meshO.bounds.center.y - meshI.bounds.center.y, 0);
        // Move object i so that o & i have the same centre level
        i.transform.position += iTranslate;
        Vector3 oFace = o.GetComponent<MeshCollider>().ClosestPoint(meshI.bounds.center);
        Vector3 iFace = i.GetComponent<MeshCollider>().ClosestPoint(meshO.bounds.center);
        Vector3 c2c = meshO.bounds.center - meshI.bounds.center;
        i.transform.position -= iTranslate;

        if (Vector3.Dot(oFace - iFace, c2c) > 0) 
        {
            return Vector3.Magnitude(oFace - iFace) < 2.0f;
        } 

        return true;
    }

    // block is the parent building of the city block
    private void SplitBlock(Transform block) 
    {
        // A group represents a set of objects which potentially belong to the same building
        List<List<Transform>> allGroups = new List<List<Transform>>();
        List<List<Transform>> intersects = new List<List<Transform>>();
        Transform currentChild;
        allGroups.Add(new List<Transform>());
        allGroups[0].Add(block);
        for (int i = block.childCount - 1; i >= 0 ; i--) 
        {
            currentChild = block.GetChild(i);
            // For each building
            foreach (List<Transform> group in allGroups) 
            {
                // Check if the current object is part of a building
                foreach (Transform member in group) 
                {
                    if (IsAreaOverlap(member.gameObject, currentChild.gameObject)) 
                    {
                        intersects.Add(group); // The groups that intersect with current child
                        break;
                    }
                }
            }

            /* If the current object is intersects with multiple buildings then these buildings are the same */
            while (intersects.Count > 1)
            {
                MergeLists(intersects[0], intersects[1]);
                allGroups.Remove(intersects[1]);
                intersects.RemoveAt(1);
            }

            /* If the current object is not part of any known buildings create a new group */
            if (intersects.Count == 0)
            { 
                allGroups.Add(new List<Transform>());
                allGroups[allGroups.Count - 1].Add(currentChild);
            }
            else
            {
                intersects[0].Add(currentChild);
            }

            intersects.Clear();
        }
        /* If there are multiple buildings in the block, rearrange the tile hierarchy */
        if (allGroups.Count > 1) 
        {
            foreach (List<Transform> list in allGroups) 
            {
                // Set every other element to be a child of the first element
                for (int i = 1; i < list.Count; i++) { list[i].SetParent(list[0], true); }
                // Set the first element to be a child of the tile (i.e. the block's parent)
                list[0].SetParent(block.parent, true);
            }
        }

    }

    public static void CombineMeshesInTile(Transform tile) {
        Matrix4x4 parentTransform;
        MeshFilter[] meshFilters;
        CombineInstance[] combine;
        Material[] materials;
        MeshCollider meshCollider;
        foreach (Transform building in tile) 
        {
            if (building.childCount < 1) { continue; }

            Mesh mainMesh = building.gameObject.transform.GetComponent<MeshFilter>().mesh;
            parentTransform = building.gameObject.transform.worldToLocalMatrix;
            meshFilters = building.gameObject.GetComponentsInChildren<MeshFilter>();
            combine = new CombineInstance[meshFilters.Length];

            for (int i = 0; i < meshFilters.Length; i++) 
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = parentTransform * meshFilters[i].transform.localToWorldMatrix;
            }

            materials = building.GetComponent<MeshRenderer>().sharedMaterials;
            Mesh batchedMesh = building.gameObject.transform.GetComponent<MeshFilter>().mesh = new Mesh();
            batchedMesh.CombineMeshes(combine);
            batchedMesh.name = building.name;
            building.gameObject.GetComponent<MeshRenderer>().materials = materials;
            meshCollider = building.gameObject.GetComponent<MeshCollider>();
            if (meshCollider != null) 
            {
                meshCollider.sharedMesh = building.gameObject.GetComponent<MeshFilter>().sharedMesh;
            }
            building.gameObject.transform.gameObject.SetActive(true);

            while (building.childCount > 0) { DestroyImmediate(building.GetChild(0).gameObject); }
        }
    }

    private int HeightCompare(Transform t1, Transform t2)
    {
        float val = t1.GetComponent<MeshRenderer>().bounds.center.y - t2.GetComponent<MeshRenderer>().bounds.center.y;
        if (val < 0 && -val > 1e-6) return -1;
        if (val > 0 && val > 1e-6) return 1;
        return 0;
    }

    private Transform SortChildrenByHeight(Transform building)
    {
        Transform shortest = building;
        MinHeap<Transform> sorter = new MinHeap<Transform>(HeightCompare);
        sorter.Add(building);
        foreach(Transform child in building) { sorter.Add(child); }

        shortest = sorter.Remove();
        shortest.SetParent(building.parent, true);
        while (sorter.size > 0)
        {
            sorter.Remove().SetParent(shortest, true);
        }

        return shortest;

    }

    // Limits the total vertex count (parent + children) of a parent building
    private void VertexLimit(Transform parent) 
    {
        int vertices;
        MeshFilter meshFilter;
        vertices = parent.GetComponent<MeshFilter>().sharedMesh.vertexCount;
        for (int i = 0; i < parent.childCount; i++) 
        {
            meshFilter = parent.GetChild(i).GetComponent<MeshFilter>();
            vertices += meshFilter.sharedMesh.vertexCount;
            if (vertices > 65000) { SplitChildren(parent, i); }
        }
    }

    // Splits the children of building from index onwards into a new parent in the hierarchy
    private Transform SplitChildren(Transform building, int index) 
    {
        Transform newParent = building.GetChild(index);
        int i = index + 1;
        while (i < building.childCount) { building.GetChild(i).SetParent(building.GetChild(index), true); }
        building.GetChild(index).SetParent(building.parent, true);

        return newParent;
    }

    // Adds element of list2 into list1 and clears list2
    private void MergeLists<T>(List<T> list1, List<T> list2)
    {
        foreach (T element in list2) { list1.Add(element); }

        list2.Clear();
    }

}

