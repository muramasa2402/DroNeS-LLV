using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Utilities;

public class TestEditor : EditorWindow {
    GameObject boundary;
    Transform city;
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
        if (city == null) { city = GameObject.Find("CitySimulatorMap").transform; }
        if (city == null) return;
        Mesh mesh;
        //GameObject thisObject = Selection.activeObject as GameObject;
        //MeshFilter mf = thisObject.GetComponent<MeshFilter>();
        //Mesh mesh = null;
        //if (mf != null)
        //{
        //    mesh = mf.sharedMesh;
        //}

        //MeshRenderer mr = thisObject.GetComponent<MeshRenderer>();
        //Vector3 scale = Vector3.zero;
        //Vector3 size = Vector3.zero;
        //Vector3 pos = Vector3.zero;

        //if (thisObject != null)
        //{
        //    scale = thisObject.transform.localScale;
        //}
        //if (mf != null)
        //{
        //    size = mesh.bounds.size;
        //}
        //if (mr != null)
        //{
        //    pos = mr.bounds.center;
        //}


        //GUILayout.Box("Size\nX: " + size.x * scale.x + "   Y: " +
            //size.y * scale.y + "   Z: " + size.z * scale.z +
            //"\nGlobal Position\nX: " + pos.x + "   Y: " +
            //pos.y + "   Z: " + pos.z,
            //GUILayout.ExpandWidth(true));

        if (GUILayout.Button("Build map shape")) {
            if (boundary.transform.childCount > 0) { return; }
            CreateMap.CreateCity(mapShape);
            boundary.transform.GetChild(0).gameObject.AddComponent<MeshCollider>();
            while (boundary.transform.childCount > 1) {
                boundary.transform.GetChild(1).gameObject.AddComponent<MeshCollider>();
                boundary.transform.GetChild(1).SetParent(boundary.transform.GetChild(0));
            }
            CreateMap.CombineMeshesInTile(boundary.transform);
            foreach (Transform child in boundary.transform) {
                DestroyImmediate(child.GetComponent<MeshRenderer>());
                DestroyImmediate(child.GetComponent<UnityTile>());
            }
        }
        
        if (GUILayout.Button("Build Vertex Spheres")) {
            MeshCollider meshCollider = boundary.transform.GetChild(0).GetComponent<MeshCollider>();
            mesh = meshCollider.sharedMesh;
            var borders = EdgeHelpers.GetEdges(mesh.triangles, mesh.vertices).FindBoundary();
            List<Vector3> verts = new List<Vector3>();
            for (int i = 0; i < borders.Count; i++) {
                verts.Add(borders[i].v1);
                verts.Add(borders[i].v2);
            }
            verts = RemoveDuplicate(verts);
            Vector3[] dir = {
                new Vector3(1,0,1),
                new Vector3(-1, 0, -1),
                new Vector3(-1, 0, 1),
                new Vector3(1, 0, -1)
            };

            int count = 0;
            int index = 0;
            for (int i = 0; i < verts.Count; i++) {
                count = 0;
                for (int j = 0; j < dir.Length; j++) 
                {
                    if (IsInside(verts[i] + dir[j], verts.ToArray())) { count++; }
                }
                if (count == 2) { continue; }
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = "Sphere - " + index++;
                sphere.transform.localScale = Vector3.one * 50;
                sphere.transform.position = verts[i] + boundary.transform.GetChild(0).position;
                sphere.transform.SetParent(boundary.transform);
            }
        }

        if (GUILayout.Button("Destroy Spheres"))
        {
            for (int i = boundary.transform.childCount - 1; i >= 0; i--)
            {
                Transform target = boundary.transform.GetChild(i);
                if (target.name.Substring(0, 6) == "Sphere")
                {
                    DestroyImmediate(target.gameObject);
                }
            }
        }

        if (GUILayout.Button("Join Convex Vertex"))
        {
            MeshCollider meshCollider = boundary.transform.GetChild(0).GetComponent<MeshCollider>();
            mesh = meshCollider.sharedMesh;
            var borders = EdgeHelpers.GetEdges(mesh.triangles, mesh.vertices).FindBoundary();
            List<Vector3> vertices = new List<Vector3>();
            for (int i = 0; i < borders.Count; i++)
            {
                vertices.Add(borders[i].v1);
                vertices.Add(borders[i].v2);
            }
            vertices = RemoveDuplicate(vertices);
            Vector3[] dir = {
                new Vector3(1,0,1),
                new Vector3(-1, 0, -1),
                new Vector3(-1, 0, 1),
                new Vector3(1, 0, -1)
            };

            List<Verts> verts = new List<Verts>();

            for (int i = vertices.Count - 1; i >= 0; i--)
            {
                int left = 0;
                int count = 0;
                for (int j = 0; j < dir.Length; j++)
                {
                    if (IsInside(vertices[i] + dir[j], vertices.ToArray())) 
                    {
                        left += dir[j].x > 0 ? 1 : 0;
                        count++; 
                    }
                }
                if (count > 2) { verts.Add(new Verts(vertices[i], false, left > 2)); }
                else if (count < 2) { verts.Add(new Verts(vertices[i], true, left > 0)); } 
            }

            verts = SortVertices(verts);
            foreach (Verts vert in verts)
            {
                Debug.Log(vert.Position - new Vector3(600, 0, 1650));
            }
            //List<int> tris = mesh.triangles.ToList();
            //List<Vector3> meshVerts = mesh.vertices.ToList();
            //int k = 0;
            //while (k < verts.Count - 2)
            //{
            //    if (verts[k].IsConvex  && !verts[k + 1].IsConvex)
            //    {
            //        tris.Add(meshVerts.IndexOf(verts[k++].Position));
            //        tris.Add(meshVerts.IndexOf(verts[k++].Position));
            //        tris.Add(meshVerts.IndexOf(verts[k].Position));
            //    } else { k++; }

            //}

            //mesh.triangles = tris.ToArray();
            //meshCollider.transform.GetComponent<MeshFilter>().sharedMesh = mesh;
            //boundary.transform.GetChild(0).GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }

    private bool IsInside(Vector3 point, Vector3 [] bounds) {
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

    private struct Verts
    {
        public Verts(Vector3 position, bool isConvex, bool isLeft = true)
        {
            Position = position;
            IsConvex = isConvex;
            IsLeft = isLeft;
        }
        public Vector3 Position { get; set; }
        public bool IsConvex { get; set; }
        public bool IsLeft { get; set; }
    }
    // TODO 
    // Split vertices into left and right, makes sure that each subsequent vertex is closest to the previous vertex
    private List<Verts> SortVertices(List<Verts> vertices)
    {
        float epsilon = 1e-5f;
        var first = vertices[0].Position;
        var last = vertices[vertices.Count - 1].Position;
        foreach (Verts vert in vertices)
        {
            if (Vector3.Magnitude(vert.Position - first) > Vector3.Magnitude(last - first))
            {
                last = vert.Position;
            }
        }

        var perp = Vector3.Cross(Vector3.up, last - first).normalized;

        int MinComparer(Verts v1, Verts v2) 
        {
            Vector3 dir1 = v1.Position - first;
            Vector3 dir2 = v2.Position - first;
            if (Vector3.Dot(dir1, last - first) - Vector3.Dot(dir2, last - first) < -epsilon)
            {
                return -1;
            }
            if (Vector3.Dot(dir1, last - first) - Vector3.Dot(dir2, last - first) > epsilon)
            {
                return 1;
            }
            return 0;
        }

        int MaxComparer(Verts v1, Verts v2)
        {
            Vector3 dir1 = v1.Position - first;
            Vector3 dir2 = v2.Position - first;
            if (Vector3.Dot(dir1, last - first) - Vector3.Dot(dir2, last - first) < -epsilon)
            {
                return 1;
            }
            if (Vector3.Dot(dir1, last - first) - Vector3.Dot(dir2, last - first) > epsilon)
            {
                return -1;
            }
            return 0;
        }

        MinHeap<Verts> rightHeap = new MinHeap<Verts>(MaxComparer);
        MinHeap<Verts> leftHeap = new MinHeap<Verts>(MinComparer);
        for (int i = 0; i < vertices.Count; i++)
        {
            if (Vector3.Dot(vertices[i].Position - first, perp) >= -epsilon) 
            {
                leftHeap.Add(vertices[i]);
            }
            else
            {
                rightHeap.Add(vertices[i]);

            }
        }
        vertices.Clear();
        while (rightHeap.size > 0) { vertices.Add(rightHeap.Remove()); }
        while (leftHeap.size > 0) { vertices.Add(leftHeap.Remove()); }

        return vertices;
    }

    List<Vector3> RemoveDuplicate(List<Vector3> points)
    {
        for (int i = points.Count - 1; i > 0; i--)
        {
            for (int j = i - 1; j >= 0; j--)
            {
                if (Vector3.Magnitude(points[i] - points[j]) < 0.1f)
                {
                    points.RemoveAt(j);
                    i--;
                }
            }
        }
        return points;
    }


}
