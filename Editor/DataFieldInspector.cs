using UnityEditor;
using TMPro.EditorUtilities;

namespace Drones.Editor
{
    using UI;
    [CustomEditor(typeof(DataField))]
    public class DataFieldInspector : TMP_UiEditorPanel
    {
        /// <summary>
        /// Draw the standard custom inspector
        /// </summary>
        override public void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }

    [CustomEditor(typeof(StatusSwitch))]
    public class StatusSwitchInspector : DataFieldInspector
    {
        /// <summary>
        /// Draw the standard custom inspector
        /// </summary>
        override public void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}