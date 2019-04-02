using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Drones.UI;
using System.Linq;
using Drones.Utils;

namespace Drones.Editor
{
    [InitializeOnLoad]
    public static class ListItemColourAdjust
    {
        static ListItemColourAdjust()
        {
            EditorApplication.hierarchyChanged += Update;
            EditorApplication.playModeStateChanged += StartStop;
        }

        static void Update()
        {
            var tuples = Object.FindObjectsOfType<MonoBehaviour>().OfType<IListItemColour>();
            foreach (var obj in tuples)
            {
               obj.ItemImage.color = Functions.EditorSet(((MonoBehaviour)obj).transform);
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

