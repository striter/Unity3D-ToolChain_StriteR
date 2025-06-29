﻿using System;
using System.Reflection;
using Rendering.Pipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    [Serializable]
    public class PostProcessCore<Data> where Data: struct , IPostProcessParameter
    {
        public Material m_Material { get; private set; }
        public PostProcessCore()
        {
            var srcName = GetType().Name;
            var lastname = srcName.Substring(1,srcName.Length-5);
            var name ="Hidden/PostProcess/"+lastname;
            var shader = RenderResources.FindPostProcess(name);

            if (shader == null)
            {
                Debug.LogError("Invalid ImageEffect Shader Found:" + name);
                return;
            }

            if (!shader.isSupported)
            {
                Debug.LogError("Shader Not Supported:" + GetType().Name);
                return;
            }
            
            m_Material = new Material(shader){name = lastname,hideFlags = HideFlags.HideAndDontSave};
        }
        public virtual void Destroy()
        {
            if (m_Material)
                GameObject.DestroyImmediate(m_Material);

            m_Material = null;
        }

        public virtual void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor,ref  Data _data) { }
        public virtual void FrameCleanUp(CommandBuffer _buffer,ref Data _data) { }
        public virtual bool Validate(ref RenderingData _renderingData,ref Data _data) => m_Material != null && _data.Validate();
        public virtual void Execute(RenderTextureDescriptor _descriptor,ref Data _data,
            CommandBuffer _buffer,  RenderTargetIdentifier _src, RenderTargetIdentifier _dst,
            ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            _buffer.Blit(_src, _dst, m_Material,0);
        }
    }
}
