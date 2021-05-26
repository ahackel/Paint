using Services;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PaintDocument))]
public class PaintServiceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var paintService = (PaintDocument)target;
        EditorGUILayout.HelpBox(new GUIContent($"Image: {paintService.ImageFilename} {paintService.ImageSize}"));
    }
}