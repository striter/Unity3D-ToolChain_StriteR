using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline
{
    [CreateAssetMenu(menuName = "AssetPipeline/Executable", fileName = "AssetPipelineExecutable", order = 0)]
    public class EAssetPipelineExecutable : ScriptableObject
    {
        public EAssetPipelineProcess[] m_Steps;
        private Stack<EAssetPipelineProcess> m_ExecutingSteps = new Stack<EAssetPipelineProcess>();
        private EAssetPipelineProcess m_CurrentStep;
        public void Tick()
        {
            if (m_CurrentStep == null && m_ExecutingSteps.Count == 0)
                return;

            if (m_CurrentStep == null)
            {
                m_CurrentStep = m_ExecutingSteps.Pop();
                m_CurrentStep.Begin();
                return;
            }
            
            if (m_CurrentStep.Executing())
                return;
            
            m_CurrentStep.End();
            m_CurrentStep = null;
        }
        
        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField( m_CurrentStep == null ? "Awaiting Executing" : 
                                        $"Executing {m_CurrentStep.name} %{math.floor(m_CurrentStep.process*100)} | {m_ExecutingSteps.Count} steps left");

            if (m_CurrentStep == null)
            {
                if (GUILayout.Button("Execute"))
                {
                    m_ExecutingSteps.Clear();
                    m_ExecutingSteps.PushRange(m_Steps);
                }
            }
            else
            {
                if (m_CurrentStep != null)
                {
                    m_CurrentStep.OnGUI();
                    if (GUILayout.Button("Cancel"))
                    {
                        m_ExecutingSteps.Clear();
                        m_CurrentStep.Cancel();
                        m_CurrentStep = null;
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
    }
}