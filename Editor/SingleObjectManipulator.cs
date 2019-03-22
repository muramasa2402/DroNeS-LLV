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
        Vector3 center = mr.bounds.center;

        GUILayout.Box(center.ToString(), GUILayout.ExpandWidth(true));

        GUILayout.Label("Closest Point", EditorStyles.boldLabel);

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


    }


}
