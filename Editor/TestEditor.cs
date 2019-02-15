using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;

public class TestEditor : EditorWindow {
    GameObject boundary;
    AbstractMap mapShape;


    [MenuItem("Window/Test Editor")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        TestEditor testWindow = new TestEditor {
            autoRepaintOnSceneChange = true
        };

        testWindow.Show();
    }

    void OnGUI() {
        if (boundary == null) { boundary = GameObject.Find("Boundary"); }
        if (boundary == null) return;
        if (mapShape == null) { mapShape = boundary.GetComponent<AbstractMap>(); }
        if (mapShape == null) return;

        /*
        GameObject thisObject = Selection.activeObject as GameObject;
        if (thisObject == null) return;
        MeshFilter mf = thisObject.GetComponent<MeshFilter>();
        if (mf == null) return;
        Mesh mesh = mf.sharedMesh;
        if (mesh == null) return;
        Vector3 size = mesh.bounds.size;
        Vector3 scale = thisObject.transform.localScale;
        Vector3 pos = thisObject.GetComponent<MeshCollider>().bounds.center;
        GUILayout.Box("Size\nX: " + size.x * scale.x + "   Y: " +
            size.y * scale.y + "   Z: " + size.z * scale.z +
            "\nGlobal Position\nX: " + pos.x + "   Y: " +
            pos.y + "   Z: " + pos.z,
            GUILayout.ExpandWidth(true));
            */

        if (GUILayout.Button("Build map shape")) {
            if (boundary.transform.childCount > 0) { return; }
            CreateCity(mapShape);
            boundary.transform.GetChild(0).gameObject.AddComponent<MeshCollider>();
            while (boundary.transform.childCount > 1) {
                boundary.transform.GetChild(1).gameObject.AddComponent<MeshCollider>();
                boundary.transform.GetChild(1).SetParent(boundary.transform.GetChild(0));
            }
            CombineTallMeshes(boundary.transform);
            foreach (Transform child in boundary.transform) {
                DestroyImmediate(child.GetComponent<MeshRenderer>());
                DestroyImmediate(child.GetComponent<UnityTile>());
            }
        }
        
        if (GUILayout.Button("Find Edges")) {
            MeshCollider meshCollider = boundary.transform.GetChild(0).GetComponent<MeshCollider>();
            Mesh mesh = meshCollider.sharedMesh;
            var borders = EdgeHelpers.GetEdges(mesh.triangles, mesh.vertices).FindBoundary();
            HashSet<Vector3> verts = new HashSet<Vector3>();
            for (int i = 0; i < borders.Count; i++) {
                verts.Add(borders[i].v1);
                verts.Add(borders[i].v2);
            }
            var points = RemoveDuplicate(verts.ToArray());
            verts.Clear();
            Vector3[] dir = {
                new Vector3(1,0,1),
                new Vector3(-1, 0, -1),
                new Vector3(-1, 0, 1),
                new Vector3(1, 0, -1)
            };

            int count = 0;
            int index = 0;
            for (int i = 0; i < points.Length; i++) {
                count = 0;
                for (int j = 0; j < dir.Length; j++) {
                    if (IsInside(points[i] + dir[j], points)) { count++; }
                }
                if (count == 2) { continue; }
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = "Sphere - " + index++;
                sphere.transform.localScale = Vector3.one * 50;
                sphere.transform.position = points[i] + boundary.transform.GetChild(0).position;
                sphere.transform.SetParent(boundary.transform);
            }

        }

        if (GUILayout.Button("Destroy Spheres")) {
            while (boundary.transform.childCount > 1) {
                DestroyImmediate(boundary.transform.GetChild(1).gameObject);
            }
        }
        if (GUILayout.Button("Try")) {
            Vector3 po = new Vector3(-285.78f+Random.value, 32.6f + Random.value, -1511.34f + Random.value);
            Ray ray = new Ray(po, Vector3.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 550)) {
                //Mesh mesh = hitInfo.collider.gameObject.GetComponent<MeshCollider>().sharedMesh;
                //Vector2[] uvs = mesh.uv;
                //for (int i = 0; i < uvs.Length; i++) {
                //    uvs[i] += new Vector2(0.5f, 0.5f);
                //}
                //mesh.uv = uvs;
                //hitInfo.collider.gameObject.GetComponent<MeshFilter>().mesh = mesh;
                Debug.Log(hitInfo.point);

                GameObject sphere = GameObject.Find("Sphere");
                GameObject cube = GameObject.Find("Cube");
                sphere.transform.position = hitInfo.collider.gameObject.GetComponent<MeshCollider>().ClosestPoint(po);
                cube.transform.position = po;
                Debug.Log(Vector3.Distance(po, sphere.transform.position));
            };

        }

    }

    private void CreateCity(AbstractMap m) {
        m.MapVisualizer = ScriptableObject.CreateInstance<MapVisualizer>();
        m.Initialize(new Mapbox.Utils.Vector2d(40.74856, -74), 16);
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
    bool IsInside(Vector3 point, Vector3 [] bounds) {
        bool[] crit = { false, false, false, false };

        for (int i = 0; i < bounds.Length; i++) {
            if (!crit[0] && point.x > bounds[i].x && point.z < bounds[i].z) {
                crit[0] = true;
                continue;
            }
            if (!crit[1] && point.x > bounds[i].x && point.z > bounds[i].z) {
                crit[1] = true;
                continue;
            }
            if (!crit[2] && point.x < bounds[i].x && point.z < bounds[i].z) {
                crit[2] = true;
                continue;
            }
            if (!crit[3] && point.x < bounds[i].x && point.z > bounds[i].z) {
                crit[3] = true;
                continue;
            }
        }

        return crit[0] && crit[1] && crit[2] && crit[3];
    }

    Vector3[] RemoveDuplicate(Vector3[] points) {
        var list = points.ToList();
        for (int i = list.Count - 1; i > 0; i--) {
            for (int j = i - 1; j >= 0; j--) {
                if (Vector3.Magnitude(list[i] - list[j]) < 0.1f) {
                    list.RemoveAt(j);
                    i--;
                }
            }
        }
        points = list.ToArray();
        return points;
    }

    void MakeCylinder(GameObject o) {
        MeshFilter filter = o.GetComponent<MeshFilter>();
        if (filter != null) { DestroyImmediate(filter); }
        MeshCollider collider = o.GetComponent<MeshCollider>();
        if (collider != null) { DestroyImmediate(collider); }
        MeshRenderer renderer = o.GetComponent<MeshRenderer>();
        if (renderer != null) { DestroyImmediate(renderer); }
        filter = o.AddComponent<MeshFilter>();
        Mesh mesh = filter.mesh;
        mesh.Clear();


        int nbSides = 4;
        float radius = 2f;

        int nbVerticesSides = nbSides + 1;
        #region Vertices
        Vector3[] vertices = new Vector3[nbVerticesSides * 2];
        int vert = 0;
        float _2pi = Mathf.PI * 2f;
        float height = _2pi * radius / 36.5f;
        int sideCounter = 0;
        while (vert < vertices.Length) {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);

            vertices[vert] = new Vector3(cos * (radius), height/2, sin * (radius));
            vertices[vert + 1] = new Vector3(cos * (radius), - height/2, sin * (radius));
            vert += 2;
        }
        #endregion
        //vert = 0;
        //while (vert < vertices.Length) {
        //    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    sphere.name = "Sphere - " + vert;
        //    sphere.transform.localScale = Vector3.one * 0.05f;
        //    sphere.transform.position = vertices[vert] + o.transform.position;
        //    sphere.transform.SetParent(o.transform);
        //    vert++;
        //}

        #region Normals

        Vector3[] normals = new Vector3[vertices.Length];
        vert = 0;

        // Sides (in)
        sideCounter = 0;
        while (vert < vertices.Length) {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;

            normals[vert] = -(new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1)));
            normals[vert + 1] = normals[vert];
            vert += 2;
        }
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];

        vert = vertices.Length - 1;
        // Sides (in)
        sideCounter = 0;
        while (vert >= 0) {
            float t = (float)(sideCounter++) / nbSides;
            uvs[vert--] = new Vector2(t, 0f);
            uvs[vert--] = new Vector2(t, 1f);
        }
        #endregion

        #region Triangles
        int nbFace = nbSides;
        int nbTriangles = nbFace * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];


        int i = 0;
        // Sides (in)
        sideCounter = 0;
        while (sideCounter < nbSides) {
            int current = sideCounter * 2;
            int next = sideCounter * 2 + 2;
            triangles[i++] = next + 1;
            triangles[i++] = next;
            triangles[i++] = current;

            triangles[i++] = current + 1;
            triangles[i++] = next + 1;
            triangles[i++] = current; 

            sideCounter++;
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        renderer = o.AddComponent<MeshRenderer>();
        renderer.material = Resources.Load("Materials/Compass") as Material;
    }

}
