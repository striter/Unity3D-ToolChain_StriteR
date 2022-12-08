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

        public virtual void Setup(CommandBuffer _buffer,ref RenderingData _renderingData){}
        public virtual void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor,ref  T _data){}
        public virtual void FrameCleanUp(CommandBuffer _buffer,ref T _data){}
        public virtual void OnValidate(ref T _data) 
        {  
        }
        public virtual void Execute(RenderTextureDescriptor _descriptor,ref T _data,
            CommandBuffer _buffer,  RenderTargetIdentifier _src, RenderTargetIdentifier _dst,
            ScriptableRenderer _renderer, ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            _buffer.Blit(_src, _dst, m_Material,0);
        }
    }
}
