using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEditor.Extensions.ScriptableObjectBundle;
using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle
{
    [CreateAssetMenu(menuName = "AssetPipeline/Executable", fileName = "AssetPipelineExecutable", order = 0)]
    public class EAssetPipelineExecutable : AScriptableObjectBundle
    {
        public override Type GetBaseType() => typeof(EAssetPipelineProcess);
        private Stack<EAssetPipelineProcess> m_ExecutingSteps = new Stack<EAssetPipelineProcess>();
        private IAssetPipelineProcessContinuous m_CurrentStep;
        public void Tick()
        {
            if (m_CurrentStep != null)
            {
                if (m_CurrentStep.Executing()) return;
                m_CurrentStep.End();
                m_CurrentStep = null;
                return;
            }
            
            if (m_ExecutingSteps.Count == 0)
                return;

            var step = m_ExecutingSteps.Pop();
            step.OnExecute();
            m_CurrentStep = step as IAssetPipelineProcessContinuous;
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Execution");
            EditorGUILayout.LabelField( m_CurrentStep == null ? "Awaiting Executing" : 
                                        $"Executing {m_CurrentStep as EAssetPipelineProcess} %{math.floor(m_CurrentStep.process*100)} | {m_ExecutingSteps.Count} steps left");

            if (m_ExecutingSteps.Count == 0 && m_CurrentStep ==null)
            {
                if (GUILayout.Button("Execute"))
                {
                    m_ExecutingSteps.Clear();
                    m_ExecutingSteps.PushRange(m_Objects.Select(p=>p as EAssetPipelineProcess));
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