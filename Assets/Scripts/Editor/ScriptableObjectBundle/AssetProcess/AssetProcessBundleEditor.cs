using UnityEditor.Extensions.EditorExecutable;
using UnityEngine;

namespace UnityEditor.Extensions.AssetProcess
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
            var bundle = target as AssetProcessBundle;;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_EnableProperty);
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            if (bundle.m_Enable && GUILayout.Button("Refresh Assets"))
            {
                bundle.ManualRefreshAssets();
                this.SetBundleDirty();
            }
        }
    }
}