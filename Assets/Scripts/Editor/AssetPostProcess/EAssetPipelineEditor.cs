using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline
{
    public class EAssetPipelineExecutable
    {
        private Stack<Func<bool>> m_ActionStack;
        private Action<bool> m_ActionOnFinished;
        public int Count => m_ActionStack.Count;
        
        public static EAssetPipelineExecutable Create(IEnumerable<Func<bool>> _actionStack,Action<bool> _actionOnCancel)
        {
            return new EAssetPipelineExecutable()
            {
                m_ActionStack = new Stack<Func<bool>>().PushRange(_actionStack),
                m_ActionOnFinished = _actionOnCancel,
            };
        }

        public void Execute()
        {
            while (m_ActionStack.Count > 0)
            {
                var action = m_ActionStack.Peek();
                if (action())
                    m_ActionStack.Pop();
                if (m_ActionStack.Count == 0)
                    m_ActionOnFinished(true);
                break;
            }
        }

        public void Cancel()
        {
            m_ActionStack.Clear();
            m_ActionOnFinished(false);
        }
    }
    
    public class EAssetPipelineEditor:EditorWindow
    {
        private EAssetPipelineExecutable m_PipelineExecutable;
        void OnEnable()
        {
            EditorApplication.update += Tick;
        }

        void OnDisable()
        {
            m_PipelineExecutable = null;
            EditorApplication.update -= Tick;
        }

        public void Initialize(EAssetPipelineExecutable _pipelineExecutable)
        {
            Debug.Assert(_pipelineExecutable!=null);
            m_PipelineExecutable = _pipelineExecutable;
        }
        
        void Tick()
        {
            m_PipelineExecutable?.Execute();
        }
        private void OnGUI()
        {
            if (m_PipelineExecutable == null)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("No Valid Action Found");
                if(GUILayout.Button("Exit"))
                    Close();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.BeginVertical();
            var count = m_PipelineExecutable.Count;
            
            EditorGUILayout.LabelField($"Executing:{count} steps left");

            if (GUILayout.Button("Cancel"))
            {
                m_PipelineExecutable.Cancel();
                m_PipelineExecutable = null;
            }
            
            if (count == 0)
                m_PipelineExecutable = null;
            
            EditorGUILayout.EndVertical();
        }

    }
}