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
        [SerializeField] private Shader[] m_PostProcesses;
        [SerializeField] private Shader[] m_IncludeShaders;
        [SerializeField] private ComputeShader[] m_ComputeShaders;
        
        private static RenderResources Instance;
        
        public RenderResources()
        {
            Instance = this;
        }
        public static Shader FindPostProcess(string _name)
        {
            var shader=Instance.m_PostProcesses.Find(p => p!=null&&p.name == _name);
            if (shader == null)
                throw new Exception($"Invalid Post Process Shader:{_name} Found!");
            return shader;
        }

        public static ComputeShader FindComputeShader(string _name)
        {            
            var shader=Instance.m_ComputeShaders.Find(p => p!=null&&p.name == _name);
            if (shader == null)
                throw new Exception($"Invalid Compute shader Shader:{_name} Found!");
            return shader;
        }

        public static Shader FindInclude(string _name)
        {            
            var shader=Instance.m_IncludeShaders.Find(p => p!=null&&p.name == _name);
            if (shader == null)
                throw new Exception($"Invalid Include Shader:{_name} Found!");
            return shader;
        }
        
    }   
}
