using System;
using System.Reflection;
using Rendering.Pipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    [Serializable]
    public class PostProcessCore<T> where T: struct
    {
        protected Material m_Material { get; private set; }
        public PostProcessCore()
        {
            string lastname = GetType().Name.Split('_')[1];
            string name ="Hidden/PostProcess/"+lastname;
            Shader shader = RenderResources.FindPostProcess(name);

            if (shader == null)
                throw new NullReferenceException("Invalid ImageEffect Shader Found:" + name);

            if (!shader.isSupported)
                throw new NullReferenceException("Shader Not Supported:" + GetType().Name);

            m_Material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        }
        public virtual void Destroy()
        {
            if (m_Material)
                GameObject.DestroyImmediate(m_Material);

            m_Material = null;
        }

        public virtual void OnValidate(ref T _data) 
        {  
        }
        public virtual void ExecutePostProcessBuffer( CommandBuffer _buffer,  RenderTargetIdentifier _src, RenderTargetIdentifier _dst,RenderTextureDescriptor _descriptor,ref T _data)
        {
            _buffer.Blit(_src, _dst, m_Material);
        }
    }
}
