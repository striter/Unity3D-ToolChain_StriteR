using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline
{
    [CustomEditor(typeof(EAssetPipelineExecutable))]
    public class EAssetPipelineEditor:Editor
    {
        private EAssetPipelineExecutable m_PipelineExecutable;
        void OnEnable()
        {
            m_PipelineExecutable = target as EAssetPipelineExecutable;
            EditorApplication.update += Tick;
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