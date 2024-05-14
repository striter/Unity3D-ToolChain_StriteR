using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle
{
    [CustomEditor(typeof(EAssetPipelineExecutable))]
    public class EAssetPipelineEditor: AScriptableObjectBundleEditor
    {
        private EAssetPipelineExecutable m_PipelineExecutable;
        private Queue<EAssetPipelineProcess> m_ExecutingSteps = new Queue<EAssetPipelineProcess>();
        private IAssetPipelineProcessContinuous m_CurrentStep;
        protected override void OnEnable()
        {
            base.OnEnable();
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
            
            if (m_CurrentStep != null)
            {
                if (m_CurrentStep.Executing()) return;
                m_CurrentStep.End();
                m_CurrentStep = null;
                return;
            }
            
            if (m_ExecutingSteps.Count == 0)
                return;

            var step = m_ExecutingSteps.Dequeue();
            if (!step.Execute())
            {
                Cancel();
                return;
            }
            m_CurrentStep = step as IAssetPipelineProcessContinuous;
            
        }
    
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginVertical();

            if (m_CurrentStep != null && m_ExecutingSteps.Count > 0)
            {
                EditorGUILayout.LabelField("Executions:");
                if(m_CurrentStep != null)
                    EditorGUILayout.LabelField($"{(m_CurrentStep as EAssetPipelineProcess).name} (%{math.floor(m_CurrentStep.process*100)})");
                foreach (var step in m_ExecutingSteps)
                    EditorGUILayout.LabelField(step.name);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginHorizontal();
            if (m_ExecutingSteps.Count == 0 && m_CurrentStep ==null)
            {
                if (m_ObjectsList.selectedIndices.Count > 0)
                {
                    if(GUILayout.Button("Execute Selected"))
                        Execute(m_PipelineExecutable.m_Objects.CollectIndex(m_ObjectsList.selectedIndices).Select(p=>p as EAssetPipelineProcess));
                }
                
                if (GUILayout.Button("Execute Batch"))
                    Execute(m_PipelineExecutable.m_Objects.Select(p=>p as EAssetPipelineProcess));
            }
            else
            {
                if (m_CurrentStep != null)
                {
                    m_CurrentStep.OnGUI();
                    if (GUILayout.Button("Cancel"))
                        Cancel();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        void Execute(IEnumerable<EAssetPipelineProcess> _steps)
        {
            m_ExecutingSteps.Clear();
            m_ExecutingSteps.EnqueueRange(_steps);
        }
        
        void Cancel()
        {
            m_ExecutingSteps.Clear();
            if (m_CurrentStep == null)
                return;
            
            m_CurrentStep.Cancel();
            m_CurrentStep = null;
        }
    }
}