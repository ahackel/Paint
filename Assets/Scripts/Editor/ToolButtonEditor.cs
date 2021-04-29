using UnityEditor;
using UnityEditor.UI;

namespace Editor
{
    [CustomEditor(typeof(ToolButton))]
    public class ToolButtonEditor : SelectableEditor
    {
        private SerializedProperty _contentImageProperty;
        private SerializedProperty _contentNormalColorProperty;
        private SerializedProperty _contentCheckedColorProperty;
        private SerializedProperty _checkedColorProperty;
        private SerializedProperty _isCheckedProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            _contentImageProperty = serializedObject.FindProperty("ContentImage");
            _contentNormalColorProperty = serializedObject.FindProperty("ContentNormalColor");
            _contentCheckedColorProperty = serializedObject.FindProperty("ContentCheckedColor");
            _checkedColorProperty = serializedObject.FindProperty("CheckedColor");
            _isCheckedProperty = serializedObject.FindProperty("IsChecked");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(_contentImageProperty);
            EditorGUILayout.PropertyField(_contentNormalColorProperty);
            EditorGUILayout.PropertyField(_contentCheckedColorProperty);
            EditorGUILayout.PropertyField(_checkedColorProperty);
            EditorGUILayout.PropertyField(_isCheckedProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}