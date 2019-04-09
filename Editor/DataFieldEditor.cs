using UnityEditor;
using Drones.UI;
using UnityEngine;

namespace Drones.Editor
{
    using Utils.Extensions;
    [InitializeOnLoad]
    public static class DataFieldEditor
    {
        public static void Update()
        {
            DataField[] fields = Object.FindObjectsOfType<DataField>();
            foreach(DataField field in fields)
            {
                if (field.gameObject.tag != "AutoExpand") { continue; }
                int index = field.transform.GetSiblingIndex();
                Debug.Log("AutoExpand");
                float parentsize = field.transform.parent.ToRect().sizeDelta.x;
                float siblingsize = field.transform.parent.GetChild(index - 1).ToRect().sizeDelta.x;
                if (field.transform.parent.childCount > 2)
                {
                    siblingsize += field.transform.parent.GetChild(index + 1).ToRect().sizeDelta.x;
                }
                Vector2 size = field.rectTransform.sizeDelta;
                size.x = parentsize - siblingsize;

                field.rectTransform.sizeDelta = size;
                field.overflowMode = TMPro.TextOverflowModes.Ellipsis;
                field.margin = new Vector4(5, 0, 0, 0);
            }

        }
    }
}