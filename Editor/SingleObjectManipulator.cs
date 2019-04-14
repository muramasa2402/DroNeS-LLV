using UnityEngine;
using UnityEditor;

public class SingleObjectManipulator : EditorWindow
{
    GameObject selection;
    GameObject cube;
    float size = 5;
    Vector3 outside;
    Vector3 point;

    [MenuItem("Window/Object Info")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        SingleObjectManipulator sizeWindow = new SingleObjectManipulator
        {
            autoRepaintOnSceneChange = true
        };
        sizeWindow.Show();
    }

    void OnGUI()
    {
        selection = Selection.activeGameObject;
        if (selection == null) { return; }
        GUILayout.Label("Center Position", EditorStyles.boldLabel);
        Collider mc = selection.GetComponent<Collider>();
        MeshRenderer mr = selection.GetComponent<MeshRenderer>();
        if (mr == null) { return; }
        MeshFilter mf = selection.GetComponent<MeshFilter>();
        Mesh mesh = mf.sharedMesh;
        Vector3 center = mr.bounds.center;

        GUILayout.Box(center.ToString(), GUILayout.ExpandWidth(true));

        GUILayout.Label("Closest Point", EditorStyles.boldLabel);
        if (GUILayout.Button("Paste Position"))
        {
            outside = center;
        }
        outside = EditorGUILayout.Vector3Field("Outside Point", outside);

        if (GUILayout.Button("Get Closest Point"))
        {
            point = mc.ClosestPoint(outside);
        }
        GUILayout.Box(point.ToString(), GUILayout.ExpandWidth(true));
        GUILayout.Label("Cube Size: ");
        size = EditorGUILayout.FloatField(size, GUILayout.ExpandWidth(true));
        if (GUILayout.Button("Make a Cube Here"))
        {
            if (cube == null) { cube = GameObject.Find("PointCube"); }
            if (cube != null) { DestroyImmediate(cube); }
            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "PointCube";
            cube.transform.SetSiblingIndex(0);
            cube.transform.position = point;
            cube.transform.localScale = size * Vector3.one;
        }

        if (GUILayout.Button("Flip Normals"))
        {
            mesh = mf.mesh;
            Vector3[] norms = mesh.normals;

            for (int i = 0; i < norms.Length; i++)
            {
                norms[i] = -norms[i];
            }
            mesh.normals = norms;
            mf.mesh = mesh;
            for (int m = 0; m < mesh.subMeshCount; m++)
            {
                int[] triangles = mesh.GetTriangles(m);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int temp = triangles[i + 0];
                    triangles[i + 0] = triangles[i + 1];
                    triangles[i + 1] = temp;
                }
                mesh.SetTriangles(triangles, m);
            }
        }

        if (GUILayout.Button("Show Verts"))
        {
            int i = 1;
            foreach (Vector3 v in mesh.vertices)
            {
                Debug.Log(i++ + ". " + v);
            }
        }
        if (GUILayout.Button("Show uv"))
        {
            int i = 1;
            foreach (Vector3 v in mesh.uv)
            {
                Debug.Log(i++ + ". " + v);
            }
        }
        if (GUILayout.Button("Save Mesh"))
        {
            mf.sharedMesh.name = "InsideOutCylinder";
            AssetDatabase.CreateAsset(mf.sharedMesh, "Assets/Resources/Meshes/InsideOutCylinder.asset");
        }

    }


}
