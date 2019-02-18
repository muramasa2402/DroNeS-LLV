using static LoadingTools.MeshOptimizer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Utilities;

public class TestEditor : EditorWindow {

    [MenuItem("Window/Test Editor")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        TestEditor testWindow = new TestEditor {
            autoRepaintOnSceneChange = true
        };

        testWindow.Show();
    }

    void OnGUI() {
        GameObject thisObject = Selection.activeObject as GameObject;
        MeshFilter mf = thisObject.GetComponent<MeshFilter>();
        Mesh mesh = null;
        if (mf != null)
        {
            mesh = mf.sharedMesh;
        }

        MeshRenderer mr = thisObject.GetComponent<MeshRenderer>();
        Vector3 scale = Vector3.zero;
        Vector3 size = Vector3.zero;
        Vector3 pos = Vector3.zero;

        if (thisObject != null)
        {
            scale = thisObject.transform.localScale;
        }
        if (mf != null)
        {
            size = mesh.bounds.size;
        }
        if (mr != null)
        {
            pos = mr.bounds.center;
        }


        GUILayout.Box("Size\nX: " + size.x * scale.x + "   Y: " +
            size.y * scale.y + "   Z: " + size.z * scale.z +
            "\nGlobal Position\nX: " + pos.x + "   Y: " +
            pos.y + "   Z: " + pos.z,
            GUILayout.ExpandWidth(true));



    }


}
