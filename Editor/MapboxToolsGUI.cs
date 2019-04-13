using static Drones.LoadingTools.MeshOptimizer;
using UnityEngine;
using UnityEditor;
using Mapbox.Unity.Map;
using Drones.LoadingTools;
using Drones.Utils;
using Drones;
using System.Collections.Generic;
using TMPro;

public class MapboxToolsGUI : EditorWindow
{
    AbstractMap abstractMap;
    GameObject citySimulatorMap;
    public float minHeight;
    public float maxHeight;

    [MenuItem("Window/Create Map")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MapboxToolsGUI sizeWindow = new MapboxToolsGUI
        {
            autoRepaintOnSceneChange = true
        };
        sizeWindow.Show();
    }

    void OnGUI()
    {
        if (citySimulatorMap == null) { citySimulatorMap = GameObject.Find("CitySimulatorMap"); }
        if (citySimulatorMap == null) { return; }
        if (abstractMap == null) { abstractMap = citySimulatorMap.GetComponent<AbstractMap>(); }

        minHeight = EditorGUILayout.FloatField("Minimum Building Height:", minHeight);
        maxHeight = EditorGUILayout.FloatField("Maximum Building Height:", maxHeight);

        if (GUILayout.Button("Edit Mode Build")) 
        {
            //if (abstractMap.MapVisualizer != null) { abstractMap.ResetMap(); }
            //abstractMap.MapVisualizer = ScriptableObject.CreateInstance<MapVisualizer>();
            abstractMap.Initialize(new Mapbox.Utils.Vector2d(40.764170691358686f, -73.97670925665614f), 16);
        }

        if (GUILayout.Button("1. Setup Objects"))
        {
            //RenameRoads(citySimulatorMap.transform);
            OptimizeRenderer(citySimulatorMap.transform);
            //foreach(Transform tile in citySimulatorMap.transform)
            //{
            //    SortChildrenByName(tile);
            //}
            //GroupAllByBlocks(citySimulatorMap.transform);
            //SplitAllBlocks(citySimulatorMap.transform);
            //SortHeirarchy(citySimulatorMap.transform);
        }

        if (GUILayout.Button("2. Combine Building Meshes"))
        {
            Transform road = null;
            foreach (Transform tile in citySimulatorMap.transform)
            {
                for (int i = 0; i < tile.childCount; i++)
                {
                    Transform current = tile.GetChild(i);
                    if (current.name.Substring(0,4) == "Road")
                    {
                        road = current;
                        continue;
                    }
                    CombineParentAndChildrenMeshes(current, !EditorApplication.isPlaying);
                }
                if (road != null && EditorApplication.isPlaying) { Destroy(road.gameObject); }
                else if (road != null && !EditorApplication.isPlaying) { DestroyImmediate(road.gameObject); }
            }
        }

        if (GUILayout.Button("3. Box buildings"))
        {
            Material material = Resources.Load("Materials/WhiteLOD") as Material;

            foreach (Transform tile in citySimulatorMap.transform)
            {
                foreach (Transform building in tile)
                {
                    if (building.childCount > 0) { continue; }
                    if (IsTall(building))
                    {
                        new BoxBuilder(building).Build(material, Building.Tall);
                    }
                    else
                    {
                        new BoxBuilder(building).Build(material, Building.Short);
                    }
                    DestroyImmediate(building.GetComponent<MeshCollider>());
                }
            }
        }

        if (GUILayout.Button("4. Combine Mesh in Tiles"))
        {
            foreach (Transform tile in citySimulatorMap.transform)
            {
                GroupByTile(tile);
            }
        }

        if (GUILayout.Button("Count Buildings"))
        {
            Debug.Log(BuildingCounter(citySimulatorMap.transform));
        }

        if (GUILayout.Button("Build City Boundaries"))
        {
            var go = GameObject.Find("Tile Boundary");
            int i = 0;
            foreach(Transform tile in go.transform)
            {
                tile.name = "Tile " + i++;
            }
        }



    }

    public static void BuildTorus()
    {
        MeshFilter filter = new GameObject().AddComponent<MeshFilter>();
        filter.transform.position = new Vector3(67, 100, 37);
        Mesh mesh = filter.mesh;
        mesh.Clear();

        float radius1 = 1f;
        float radius2 = .05f;
        int nbRadSeg = 24;
        int nbSides = 18;

        #region Vertices        
        Vector3[] vertices = new Vector3[(nbRadSeg + 1) * (nbSides + 1)];
        float _2pi = Mathf.PI * 2f;
        for (int seg = 0; seg <= nbRadSeg; seg++)
        {
            int currSeg = seg == nbRadSeg ? 0 : seg;

            float t1 = (float)currSeg / nbRadSeg * _2pi;
            Vector3 r1 = new Vector3(Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1);

            for (int side = 0; side <= nbSides; side++)
            {
                int currSide = side == nbSides ? 0 : side;

                Vector3 normale = Vector3.Cross(r1, Vector3.up);
                float t2 = (float)currSide / nbSides * _2pi;
                Vector3 r2 = Quaternion.AngleAxis(-t1 * Mathf.Rad2Deg, Vector3.up) * new Vector3(Mathf.Sin(t2) * radius2, Mathf.Cos(t2) * radius2);

                vertices[side + seg * (nbSides + 1)] = r1 + r2;
            }
        }
        #endregion

        #region Normales        
        Vector3[] normales = new Vector3[vertices.Length];
        for (int seg = 0; seg <= nbRadSeg; seg++)
        {
            int currSeg = seg == nbRadSeg ? 0 : seg;

            float t1 = (float)currSeg / nbRadSeg * _2pi;
            Vector3 r1 = new Vector3(Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1);

            for (int side = 0; side <= nbSides; side++)
            {
                normales[side + seg * (nbSides + 1)] = (vertices[side + seg * (nbSides + 1)] - r1).normalized;
            }
        }
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int seg = 0; seg <= nbRadSeg; seg++)
            for (int side = 0; side <= nbSides; side++)
                uvs[side + seg * (nbSides + 1)] = new Vector2((float)seg / nbRadSeg, (float)side / nbSides);
        #endregion

        #region Triangles
        int nbFaces = vertices.Length;
        int nbTriangles = nbFaces * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        int i = 0;
        for (int seg = 0; seg <= nbRadSeg; seg++)
        {
            for (int side = 0; side <= nbSides - 1; side++)
            {
                int current = side + seg * (nbSides + 1);
                int next = side + (seg < (nbRadSeg) ? (seg + 1) * (nbSides + 1) : 0);

                if (i < triangles.Length - 6)
                {
                    triangles[i++] = current;
                    triangles[i++] = next;
                    triangles[i++] = next + 1;

                    triangles[i++] = current;
                    triangles[i++] = next + 1;
                    triangles[i++] = current + 1;
                }
            }
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
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
            foreach (Transform building in tile) { count++; }
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

    public static void DeleteEmptyTiles(Transform city)
    {
        for (int i = city.childCount - 1; i >= 0; i--)
        {
            Transform tile = city.GetChild(i);
            if (tile.childCount == 0) { DestroyImmediate(tile.gameObject); }
        }
    }

    private static void RenameRoads(Transform city)
    {
        foreach(Transform tile in city)
        {
            foreach (Transform component in tile)
            {
                if (component.name == "road")
                {
                    component.name = "Road - " + tile.name.Replace("/", " ");
                }
            }
        }
    }

}

