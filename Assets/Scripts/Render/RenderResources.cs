using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering.Pipeline
{
    [Serializable,CreateAssetMenu(fileName = "Render Resources",menuName = "Rendering/Render Resources",order = 0)]
    public class RenderResources : ScriptableObject
    {
        public Shader[] m_PostProcesses;
        public Shader[] m_HiddenShaders;
        public ComputeShader[] m_ComputeShaders;
        
        public Shader FindPostProcess(string _name)
        {
            return m_PostProcesses.Find(p => p.name == _name);
        }

        public ComputeShader FindComputeShader(string _name)
        {
            return m_ComputeShaders.Find(p => p.name == _name);
        }

        public Shader FindHiddenShader(string _name)
        {
            return m_HiddenShaders.Find(p => p.name == _name);
        }
    }   
}
