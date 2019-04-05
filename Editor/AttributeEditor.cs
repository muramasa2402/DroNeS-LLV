using UnityEditor;
using TMPro;
using UnityEngine;

namespace Drones.Editor
{
    using Utils.Extensions;
    [InitializeOnLoad]
    public static class AttributeEditor
    {
        static AttributeEditor()
        {
            EditorApplication.hierarchyChanged += Update;
            EditorApplication.playModeStateChanged += StartStop;
        }


        private static void Update()
        {
            GameObject[] atts = GameObject.FindGameObjectsWithTag("AttributeAutoExpand");
            foreach (GameObject att in atts)
            {
                att.transform.ToRect().sizeDelta -= Vector2.right * att.transform.ToRect().sizeDelta.x;
                TextMeshProUGUI tmp = att.GetComponent<TextMeshProUGUI>();
                tmp.text = att.transform.parent.name + ":";
                do
                {
                    att.transform.ToRect().sizeDelta += Vector2.right;
                    tmp.ForceMeshUpdate();
                } while (tmp.isTextTruncated);
            }
            DataFieldEditor.Update();

        }

        private static void StartStop(PlayModeStateChange state)
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