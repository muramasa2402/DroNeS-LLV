using UnityEngine;
using UnityEditor;

namespace Drones.Editor
{
    using UI;

    [InitializeOnLoad]
    public static class ListContentHeightAdjustEditor
    {
        static ListContentHeightAdjustEditor()
        {
            EditorApplication.hierarchyChanged += Update;
            EditorApplication.playModeStateChanged += StartStop;
        }

        static void Update()
        {
            ListContent[] listlinks = Object.FindObjectsOfType<ListContent>();
            foreach (ListContent lcha in listlinks)
            {
                lcha.GetHeight();
                lcha.GetSeparation();
                lcha.SetHeight();
            }
        }

        static void StartStop(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    EditorApplication.update -= Update;
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    EditorApplication.update += Update;
                    break;
            }
        }
    }

}
