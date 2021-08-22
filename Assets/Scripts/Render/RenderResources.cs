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
        [SerializeField][PreloadAssets("Shaders/PostProcess")] private Shader[] m_PostProcesses;
        [SerializeField][PreloadAssets("Shaders/Hidden")] private Shader[] m_HiddenShaders;
        [SerializeField][PreloadAssets("Shaders/Compute")] private ComputeShader[] m_ComputeShaders;
        
        private static RenderResources Instance;
        
        public RenderResources()
        {
            Instance = this;
        }
        public static Shader FindPostProcess(string _name)
        {
            return Instance.m_PostProcesses.Find(p => p.name == _name);
        }

        public static ComputeShader FindComputeShader(string _name)
        {
            return Instance.m_ComputeShaders.Find(p => p.name == _name);
        }

        public static Shader FindHiddenShader(string _name)
        {
            return Instance.m_HiddenShaders.Find(p => p.name == _name);
        }
        
    }   
}
