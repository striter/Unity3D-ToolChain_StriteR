using UnityEditor.Extensions.ScriptableObjectBundle;

namespace UnityEditor.Extensions.AssetPipeline
{
    [CustomEditor(typeof(AssetProcessBundle))]
    public class AssetProcessBundleEditor: AScriptableObjectBundleEditor
    {
        protected SerializedProperty m_EnableProperty;
        protected override void OnEnable()
        {
            base.OnEnable();
            m_EnableProperty = serializedObject.FindProperty(nameof(AssetProcessBundle.m_Enable));
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_EnableProperty);
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                (target as AssetProcessBundle).OnManualChange();
            }
        }
    }
}