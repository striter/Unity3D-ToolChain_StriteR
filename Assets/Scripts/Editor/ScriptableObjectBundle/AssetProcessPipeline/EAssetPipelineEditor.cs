using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle
{
    [CustomEditor(typeof(EAssetPipelineExecutable))]
    public class EAssetPipelineEditor: AScriptableObjectBundleEditor
    {
        private EAssetPipelineExecutable m_PipelineExecutable;
        protected override void OnEnable()
        {
            base.OnEnable();
            m_PipelineExecutable = target as EAssetPipelineExecutable;
            EditorApplication.update += Tick;
        }


        protected override void DrawElement(Rect _rect,int _index, SerializedProperty _property, bool _isActive, bool _isFocused)
        {
            base.DrawElement(_rect, _index,_property, _isActive, _isFocused);
            if(!UEGUI.IsExpanded(_property))return;

            if (GUI.Button(_rect.Move(14f,EditorGUI.GetPropertyHeight(_property)).Resize(_rect.size.x - 14f,EditorGUIUtility.singleLineHeight), "Execute"))
            {
                if (m_PipelineExecutable.m_Objects[_index] is IAssetPipelineProcessContinuous)
                {
                    Debug.LogWarning("IAssetPipelineProcessContinuous Execution Not Supported");
                    return;
                }
                
                (m_PipelineExecutable.m_Objects[_index] as EAssetPipelineProcess).OnExecute();
            }
        }

        protected override float GetElementHeight(SerializedProperty _property)
        {
            return base.GetElementHeight(_property) + (UEGUI.IsExpanded(_property)? 20f : 0f);
        }

        void OnDisable()
        {
            m_PipelineExecutable = null;
            EditorApplication.update -= Tick;
        }
    
        void Tick()
        {
            if (m_PipelineExecutable == null)
                return;
            
            m_PipelineExecutable?.Tick();
        }
    
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            m_PipelineExecutable.OnGUI();
        }
    }
}