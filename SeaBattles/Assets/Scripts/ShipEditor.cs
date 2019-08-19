using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShipEditorTools))]
public class ShipEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ShipEditorTools script = (ShipEditorTools)target;
        if (GUILayout.Button("Remove duplicates"))
        {
            script.RemoveDuplicates();
        }

        if (GUILayout.Button("Beautify names"))
        {
            script.BeautifyNames();
        }

        if (GUILayout.Button("Create mirror"))
        {
            script.CreateMirror();
        }

        if (GUILayout.Button("Remove mirror"))
        {
            script.RemoveMirror();
        }
    }
}
