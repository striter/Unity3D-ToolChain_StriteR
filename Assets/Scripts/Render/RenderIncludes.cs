using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Rendering.Pipeline
{
    public class RenderIncludes : ScriptableObject
    {
        public Shader[] m_PostProcesses;
        public ComputeShader[] m_ComputeShaders;

        [NonSerialized] public Dictionary<string, int> m_ShaderIndexes;
        [NonSerialized] public Dictionary<string, int> m_ComputShaderIndexes;
    }   
}
