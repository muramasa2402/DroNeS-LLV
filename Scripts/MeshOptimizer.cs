using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Utilities.LoadingTools
{
    public static class MeshOptimizer
    {
        public static readonly float EPSILON = 1e-5f;
        // Combines the parent and children meshes and delete children objects
        public static void CombineParentAndChildrenMeshes(Transform parent, bool editMode = false)
        {
            Matrix4x4 parentTransform;
            MeshFilter[] meshFilters;
            CombineInstance[] combine;
            Material[] materials = null;
            MeshCollider meshCollider;
            if (!IsParent(parent)) { return; } //TODO Maybe throw exception?

            Mesh mainMesh = parent.gameObject.transform.GetComponent<MeshFilter>().mesh;
            parentTransform = parent.gameObject.transform.worldToLocalMatrix;
            meshFilters = parent.gameObject.GetComponentsInChildren<MeshFilter>();
            combine = new CombineInstance[meshFilters.Length];

            for (int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = parentTransform * meshFilters[i].transform.localToWorldMatrix;
            }
            MeshRenderer meshRenderer = parent.GetComponent<MeshRenderer>();

            if (meshRenderer != null) { materials = meshRenderer.sharedMaterials; }

            Mesh batchedMesh = parent.gameObject.transform.GetComponent<MeshFilter>().mesh = new Mesh();
            batchedMesh.CombineMeshes(combine);
            batchedMesh.name = parent.name;

            if (materials != null) { meshRenderer.materials = materials; }

            meshCollider = parent.gameObject.GetComponent<MeshCollider>();

            if (meshCollider != null)
            {
                meshCollider.sharedMesh = parent.gameObject.GetComponent<MeshFilter>().sharedMesh;
            }

            parent.gameObject.transform.gameObject.SetActive(true);

            while (parent.childCount > 0) 
            { 
                if (editMode) { Object.DestroyImmediate(parent.GetChild(0).gameObject); }
                else { Object.Destroy(parent.GetChild(0).gameObject); }
            }
        }

        // Turn off unnecessary options in the renderer
        public static void OptimizeRenderer(Transform city)
        {
            Material[] materials = new Material[1];
            materials[0] = Resources.Load("Materials/BuildingMaterial") as Material;
            Mesh mesh;
            MeshRenderer meshRenderer;

            foreach (Transform tile in city)
            {
                meshRenderer = tile.GetComponent<MeshRenderer>();
                meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                meshRenderer.allowOcclusionWhenDynamic = true;
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

                if (tile.childCount == 0) { continue;  }

                foreach (Transform i in tile)
                {
                    meshRenderer = i.GetComponent<MeshRenderer>();
                    mesh = i.GetComponent<MeshFilter>().mesh;
                    mesh.SetTriangles(mesh.triangles, 0);
                    mesh.subMeshCount = 1;
                    meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                    if (i.name.Substring(0,4) == "Road")
                    {
                        i.GetComponent<MeshCollider>().convex = false;
                        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                        meshRenderer.receiveShadows = false;
                        Material[] roadmat = new Material[1];
                        roadmat[0] = meshRenderer.sharedMaterials[0];
                        meshRenderer.materials = roadmat;
                    }
                    else
                    {
                        i.gameObject.layer = 11; // Set layer to buildings
                        meshRenderer.materials = materials;
                    }
                }
            }
        }

        // Makes grouping buildings easier, typically similar names = same group
        public static void SortChildrenByName(Transform parent)
        {
            MinHeap<Transform> sorter = new MinHeap<Transform>((Transform t1, Transform t2) => {
                return string.Compare(t1.name, t2.name, System.StringComparison.Ordinal);
            });

            foreach (Transform child in parent)
            {
                sorter.Add(child);
            }

            while (!sorter.IsEmpty())
            {
                sorter.Remove().SetAsLastSibling();
            }
        }

        public static void SeparateShortAndTallBuildings(GameObject city, float minimumUnityHeight, bool editMode = false)
        {
            foreach (Transform tile in city.transform)
            {
                SortChildrenByName(tile);
                GameObject container = null;
                bool needNewContainer = true;
                for (int i = tile.childCount - 1; i >= 0; i--)
                {
                    Transform component = tile.GetChild(i);
                    if (component.name.Substring(0, 8) != "Building") { continue; }

                    MeshRenderer mr = component.GetComponent<MeshRenderer>();
                    if (mr.bounds.center.y + mr.bounds.extents.y - minimumUnityHeight < -EPSILON)
                    {
                        component.name = component.name.Replace("Building", "Short");
                        if (needNewContainer)
                        {
                            container = component.gameObject;
                            needNewContainer = false;
                        } 
                        else
                        {
                            component.transform.SetParent(container.transform);
                            component.SetAsFirstSibling();
                        }
                        if (editMode) { Object.DestroyImmediate(component.GetComponent<MeshCollider>()); }
                        else { Object.Destroy(component.GetComponent<MeshCollider>()); }
                    } 
                    else
                    {
                        component.name = component.name.Replace("Building", "Tall");
                        Mesh mesh = component.GetComponent<MeshFilter>().sharedMesh;
                        Vector2[] uvs = mesh.uv;
                        for (int j = 0; j < uvs.Length; j++)
                        {
                            uvs[j] -= 0.5f * Vector2.one;
                        }
                        mesh.uv = uvs;
                    }
                }

            }
        }

        // Groups building structures into city blocks
        public static void GroupBlocks(Transform city)
        {
            List<Transform> tallBuildings = new List<Transform>();

            GameObject road = null;
            foreach (Transform tile in city)
            {
                /* Preprocess: putting all the buildings in a list
                since hierarchy changes as we go through the loop */
                foreach (Transform i in tile)
                {
                    if (IsTall(i))
                    {
                        i.GetComponent<MeshCollider>().convex = true;
                        tallBuildings.Add(i);
                    }
                    else if (i.name.Substring(0,4) == "Road") { road = i.gameObject; }
                }

                if (tallBuildings.Count > 1 && road != null)
                {
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
            }
        }

        // Do SplitBlock on the whole city 
        public static void SplitAllBlocks(Transform city)
        {
            foreach (Transform tile in city)
            {
                for (int i = tile.childCount - 1; i >= 0; i--)
                {
                    Transform block = tile.GetChild(i);
                    if (block.childCount > 0) { SplitBlock(block); }
                }
            }
        }

        // Sorts the hierarchy so that parents will be the shortest structure and splits children to limit vertex count
        public static void SortHeirarchy(Transform city)
        {
            foreach (Transform tile in city)
            {
                for (int i = 0; i < tile.childCount; i++)
                {
                    Transform building = tile.GetChild(i);
                    if (building.childCount != 0)
                    {
                        Transform parent = SortBuildingComponents(building);
                        LimitVertex(parent);
                    }
                }

            }
        }

        // Checks if the object name begins with "Tall" //TODO make this more generic?
        private static bool IsTall(Transform building)
        {
            return building.name.Substring(0, 8) == "Tall";
        }

        // Checks if objects are in the same city block
        private static bool InBlock(GameObject blockParent, GameObject i, GameObject road)
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
        private static bool IsAreaOverlap(GameObject o, GameObject i)
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

        // If there are multiple tall buildings in city block split them up
        private static void SplitBlock(Transform block)
        {
            if (!IsTall(block)) { return; }
            // A group represents a set of objects which potentially belong to the same building
            List<List<Transform>> allGroups = new List<List<Transform>>();
            List<List<Transform>> intersects = new List<List<Transform>>();
            Transform currentChild;
            allGroups.Add(new List<Transform>());
            allGroups[0].Add(block);
            for (int i = block.childCount - 1; i >= 0; i--)
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

        // Sets the parent of a building to be the shortest structure and arranges the children in ascending height
        private static Transform SortBuildingComponents(Transform building)
        {
            Transform shortest = building;
            int HeightCompare(Transform t1, Transform t2)
            {
                float val = t1.GetComponent<MeshRenderer>().bounds.center.y - t2.GetComponent<MeshRenderer>().bounds.center.y;
                if (val < 0 && -val > 1e-6) return -1;
                if (val > 0 && val > 1e-6) return 1;
                return 0;
            }
            MinHeap<Transform> sorter = new MinHeap<Transform>(HeightCompare);
            sorter.Add(building);
            foreach (Transform child in building) { sorter.Add(child); }

            shortest = sorter.Remove();
            shortest.SetParent(building.parent, true);
            while (sorter.size > 0)
            {
                sorter.Remove().SetParent(shortest, true);
            }

            return shortest;
        }

        // Limits the total vertex count of a building by splitting up objects in the hierarchy
        private static void LimitVertex(Transform parent)
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
        private static Transform SplitChildren(Transform building, int index)
        {
            Transform newParent = building.GetChild(index);
            int i = index + 1;
            while (i < building.childCount) { building.GetChild(i).SetParent(building.GetChild(index), true); }
            building.GetChild(index).SetParent(building.parent, true);

            return newParent;
        }

        // Adds element of list2 into list1 and clears list2
        private static void MergeLists<T>(List<T> list1, List<T> list2)
        {
            foreach (T element in list2) { list1.Add(element); }

            list2.Clear();
        }

        public static bool IsParent(Transform parent)
        {
            if (parent.childCount == 0) return false;

            return true;
        }

    }
}
