using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Map.TileProviders;
using Utilities;
using Utilities.LoadingTools;

public class CityBoundary : MonoBehaviour
{
    public static void CreateBoundary()
    {
        GameObject boundary = CreateMap();

        SetOrigin(boundary);
        CombineAllTiles(boundary);

        MeshCollider meshCollider = boundary.transform.GetChild(0).GetComponent<MeshCollider>();
        Mesh mesh = meshCollider.sharedMesh;
        List<Vertex> east = new List<Vertex>();
        List<Vertex> west = new List<Vertex>();

        SplitVertices(GetVertices(mesh), ref east, ref west);
        MeshFilter mf = boundary.AddComponent<MeshFilter>();
        mf.sharedMesh = Instantiate(boundary.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh) as Mesh;
        boundary.AddComponent<MeshCollider>();
        DestroyImmediate(boundary.transform.GetChild(0).gameObject);
        List<Vector3> verts = CombineListEndToEnd(east, west);
        BuildWall(verts, boundary);
    }

    private static List<Vector3> CombineListEndToEnd(List<Vertex> east, List<Vertex> west)
    {
        List<Vector3> output = new List<Vector3>();
        for (int j = 0; j < east.Count; j++)
        {
            if (east[j].Convex) { output.Add(east[j].Position); }
        }
        for (int j = west.Count - 1; j >= 0; j--)
        {
            if (west[j].Convex) { output.Add(west[j].Position); }
        }

        return output;
    }

    private static Vector3[] FillVertices(List<Vector3> verts)
    {
        Vector3[] vertices = new Vector3[2 * (verts.Count + 1)];
        vertices[0] = Vector3.zero;
        for (int j = 0; j < verts.Count; j++)
        {
            vertices[j + 1] = verts[j];
        }
        vertices[verts.Count + 1] = Vector3.down * 30f;
        for (int j = 0; j < verts.Count; j++)
        {
            vertices[j + 2 + verts.Count] = verts[j] + Vector3.down * 30f;
        }
        return vertices;
    }

    private static Vector3[] FillNormals(Vector3[] vertices)
    {
        Vector3[] normals = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length / 2; i++)
        {
            normals[i] = Vector3.up;
        }
        for (int i = vertices.Length / 2; i < vertices.Length; i++)
        {
            normals[i] = Vector3.down;
        }

        return normals;
    }

    private static Vector2[] FillUVs(Vector3[] vertices)
    {
        Vector2[] uvs = new Vector2[vertices.Length];
        uvs[0] = new Vector2(0.5f, 0.5f);
        for (int i = 1; i < vertices.Length / 2; i++)
        {
            float t = (float) (i - 1) / (vertices.Length / 2 - 1) * 2 * Mathf.PI;
            uvs[i] = new Vector2(.5f * Mathf.Cos(t) + .5f, .5f * Mathf.Sin(t) + .5f);
        }
        for (int i = vertices.Length / 2; i < vertices.Length; i++)
        {
            uvs[i] = uvs[i - vertices.Length / 2];
        }

        return uvs;
    }

    private static int[] FillTriangles(Vector3[] vertices)
    {
        int numTris = (vertices.Length - 1) * 4;
        int[] triangles = new int[numTris * 3];
        int k = 0;
        for (int i = 0; i < vertices.Length / 2 - 1; i++)
        {
            int current = i + 1;
            int next = i + 2;

            if (i == vertices.Length / 2 - 2) { next = 1; }

            triangles[k++] = next;
            triangles[k++] = 0;
            triangles[k++] = current;

            triangles[k++] = next + vertices.Length / 2;
            triangles[k++] = vertices.Length / 2;
            triangles[k++] = current + vertices.Length / 2;

            triangles[k++] = next + vertices.Length / 2;
            triangles[k++] = next;
            triangles[k++] = current;

            triangles[k++] = current + vertices.Length / 2;
            triangles[k++] = next + vertices.Length / 2;
            triangles[k++] = current;
        }
        return triangles;
    }

    private static void BuildFloor(List<Vector3> verts, GameObject boundary)
    {

        Vector3[] vertices = FillVertices(verts);

        Vector3[] normals = FillNormals(vertices);

        Vector2[] uvs = FillUVs(vertices);

        int[] triangles = FillTriangles(vertices);

        MeshFilter filter = boundary.AddComponent<MeshFilter>();
        Mesh m = filter.mesh;
        m.Clear();
        m.vertices = vertices;
        m.normals = normals;
        m.uv = uvs;
        m.triangles = triangles;

        m.RecalculateBounds();
        m.RecalculateNormals();
        m.RecalculateTangents();
        MeshUtility.Optimize(m);
        Collider bc = boundary.AddComponent<MeshCollider>();
    }

    private static void BuildWall(List<Vector3> verts, GameObject boundary)
    {
        for (int i = 0; i < verts.Count; i++)
        {
            int next = (i == verts.Count - 1) ? 0 : i + 1;
            float height = 250f;
            float thickness = 30f;
            Vector3 position = verts[i] + 0.5f * (verts[next] - verts[i]);
            Vector3 translation = thickness / 2 * Vector3.Cross(Vector3.up, verts[next] - verts[i]).normalized;
            translation += Vector3.up * height / 2;
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall " + i.ToString();
            DestroyImmediate(wall.GetComponent<MeshRenderer>());
            wall.transform.position = position + translation;
            float length = (verts[next] - verts[i]).magnitude * 1.1f;
            wall.transform.localScale = new Vector3(thickness, height, length);
            float angle = Vector3.Angle(Vector3.forward, verts[next] - verts[i]);
            angle = (Mathf.Sign(Vector3.Dot(Vector3.right, verts[next] - verts[i])) < 0) ? -angle : angle;
            wall.transform.Rotate(0, angle, 0, Space.World);
            wall.transform.SetParent(boundary.transform);
        }
    }

    private static void CreateCity(AbstractMap m)
    {
        if (m.MapVisualizer != null) { m.ResetMap(); }
        m.MapVisualizer = ScriptableObject.CreateInstance<MapVisualizer>();
        m.Initialize(new Mapbox.Utils.Vector2d(40.764170691358686f, -73.97670925665614f), 16);
    }

    private static GameObject CreateMap()
    {
        GameObject map = new GameObject
        {
            name = "City Boundary"
        };
        AbstractMap mapShape = map.AddComponent<AbstractMap>();
        AbstractTileProvider tileProvider = map.AddComponent<Manhattan>();
        mapShape.Options.extentOptions.extentType = MapExtentType.Custom;
        mapShape.TileProvider = tileProvider;
        mapShape.InitializeOnStart = false;
        mapShape.Options.scalingOptions.unityTileSize = 150;
        mapShape.Options.placementOptions.placementType = MapPlacementType.AtTileCenter;
        mapShape.Options.placementOptions.snapMapToZero = true;

        CreateCity(mapShape);
        DestroyImmediate(mapShape);
        DestroyImmediate(tileProvider);

        return map;
    }

    private static void SetOrigin(GameObject map)
    {
        for (int i = 0; i < map.transform.childCount; i++)
        {
            var child = map.transform.GetChild(i);
            if (child.position == Vector3.zero)
            {
                child.SetSiblingIndex(0);
            }
        }
    }

    struct Vertex
    {
        public Vertex(Vector3 position, bool convex)
        {
            Position = position;
            Convex = convex;
        }
        public Vector3 Position { get; set; }
        public bool Convex { get; set; }
    }

    private static void CombineAllTiles(GameObject boundary)
    {
        boundary.transform.GetChild(0).gameObject.AddComponent<MeshCollider>();
        while (boundary.transform.childCount > 1)
        {
            boundary.transform.GetChild(1).gameObject.AddComponent<MeshCollider>();
            boundary.transform.GetChild(1).SetParent(boundary.transform.GetChild(0));
        }
        MeshOptimizer.CombineParentAndChildrenMeshes(boundary.transform.GetChild(0), true);
    }

    private static List<Vector3> GetVertices(Mesh mesh)
    {
        var borders = EdgeHelpers.GetEdges(mesh.triangles, mesh.vertices).FindBoundary();
        List<Vector3> vertices = new List<Vector3>();

        for (int i = 0; i < borders.Count; i++)
        {
            vertices.Add(borders[i].v1);
            vertices.Add(borders[i].v2);
        }
        vertices = RemoveDuplicate(vertices);

        return vertices;
    }

    private static void SplitVertices(List<Vector3> vertices, ref List<Vertex> east, ref List<Vertex> west)
    {
        Vector3[] dir = {
                new Vector3(1,0,1),
                new Vector3(-1, 0, -1),
                new Vector3(-1, 0, 1),
                new Vector3(1, 0, -1) };

        for (int i = vertices.Count - 1; i >= 0; i--)
        {
            int westFlag = 0;
            int count = 0;
            for (int j = 0; j < dir.Length; j++)
            {
                if (IsInside(vertices[i] + dir[j], vertices))
                {
                    westFlag += dir[j].x > 0 ? 1 : 0;
                    count++;
                }
            }

            if (count > 2) // Concave
            {
                if (westFlag > 1)
                {
                    west.Add(new Vertex(vertices[i], false));
                }
                else
                {
                    east.Add(new Vertex(vertices[i], false));
                }
            }
            else if (count < 2) // Convex
            {
                if (westFlag > 0)
                {
                    west.Add(new Vertex(vertices[i], true));
                }
                else
                {
                    east.Add(new Vertex(vertices[i], true));
                }
            }
        }
        vertices.Clear();
        east = SortVertices(east);
        west = SortVertices(west);
    }

    private static List<Vertex> SortVertices(List<Vertex> vertices)
    {
        float epsilon = 1e-5f;
        Vertex anchor = vertices[0];
        int n = 0;
        for (int j = 0; j < vertices.Count; j++)
        {
            if (anchor.Position.x - vertices[j].Position.x >= -epsilon && anchor.Position.z - vertices[j].Position.z >= -epsilon)
            {
                anchor = vertices[j];
                n = j;
            }
        }

        vertices.RemoveAt(n);
        vertices.Insert(0, anchor);

        for (int i = 0; i < vertices.Count - 1; i++)
        {
            anchor = vertices[i];
            Vertex next = vertices[i + 1];
            for (int j = i + 1; j < vertices.Count; j++)
            {
                if (Vector3.Magnitude(vertices[j].Position - anchor.Position) - Vector3.Magnitude(next.Position - anchor.Position) < epsilon)
                {
                    next = vertices[j];
                    n = j;
                }
            }

            vertices.RemoveAt(n);
            vertices.Insert(i + 1, next);
        }
        return vertices;
    }

    private static bool IsInside(Vector3 point, List<Vector3> bounds)
    {
        bool[] crit = { false, false, false, false };

        for (int i = 0; i < bounds.Count; i++)
        {
            if (!crit[0] && point.x > bounds[i].x && point.z < bounds[i].z)
            {
                crit[0] = true;
                continue;
            }
            if (!crit[1] && point.x > bounds[i].x && point.z > bounds[i].z)
            {
                crit[1] = true;
                continue;
            }
            if (!crit[2] && point.x < bounds[i].x && point.z < bounds[i].z)
            {
                crit[2] = true;
                continue;
            }
            if (!crit[3] && point.x < bounds[i].x && point.z > bounds[i].z)
            {
                crit[3] = true;
                continue;
            }
        }

        return crit[0] && crit[1] && crit[2] && crit[3];
    }

    private static List<Vector3> RemoveDuplicate(List<Vector3> points)
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


