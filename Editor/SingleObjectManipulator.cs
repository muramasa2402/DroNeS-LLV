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

        if (GUILayout.Button("Do UV"))
        {
            mesh = mf.mesh;
            Vector2[] uv = mesh.uv;
            int j = 0;
            // N
            Vector2 x = Vector2.right * selection.transform.lossyScale.x/2;
            Vector2 y = Vector2.up * selection.transform.lossyScale.y/2;
            Vector2 z = Vector2.zero;
            uv[j++] = Vector2.zero;
            uv[j++] = y;
            uv[j++] = x + y;
            uv[j++] = x;
            // U
            x = Vector2.right;// * selection.transform.lossyScale.x;
            z = Vector2.up;// * selection.transform.lossyScale.z;

            uv[j++] = x + z;
            uv[j++] = x;
            uv[j++] = Vector2.zero;
            uv[j++] = z;

            // S
            x = Vector2.right * selection.transform.lossyScale.x/2;
            y = Vector2.up * selection.transform.lossyScale.y/2;
            uv[j++] = x + y;
            uv[j++] = x;
            uv[j++] = Vector2.zero;
            uv[j++] = y;

            // D
            x = Vector2.right;// * selection.transform.lossyScale.x;
            z = Vector2.up;// * selection.transform.lossyScale.z;
            uv[j++] = x + z;
            uv[j++] = x;
            uv[j++] = Vector2.zero;
            uv[j++] = z;

            // W
            y = Vector2.right * selection.transform.lossyScale.y/2;
            z = Vector2.up * selection.transform.lossyScale.z/2;

            uv[j++] = Vector2.zero;
            uv[j++] = z;
            uv[j++] = y + z;
            uv[j++] = y;

            // E
            y = Vector2.right * selection.transform.lossyScale.y/2;
            z = Vector2.up * selection.transform.lossyScale.z/2;
            uv[j++] = Vector2.zero;
            uv[j++] = z;
            uv[j++] = y + z;
            uv[j++] = y;

            mesh.uv = uv;
            mf.mesh = mesh;
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
            mf.sharedMesh.name = "AltCube";
            AssetDatabase.CreateAsset(mf.sharedMesh, "Assets/Resources/Meshes/AltCube.asset");
        }

    }


}
