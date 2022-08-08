using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    [ExecuteInEditMode]
    public class PostProcessBehaviour<T,Y> : MonoBehaviour,IPostProcessBehaviour where T : PostProcessCore<Y>, new() where Y:struct,IPostProcessParameter
    {
        [MTitle] public Y m_Data;
        protected T m_Effect { get; private set; }
        private static readonly string kDefaultName = typeof(T).Name;
        public string m_Name => kDefaultName;
        public virtual bool m_OpaqueProcess => throw new NotImplementedException();
        public virtual EPostProcess Event => throw new NotImplementedException();
        public bool m_Enabled { get; private set;}
        private IPostProcessPipelineCallback<Y> m_EffectPipeline { get; set; }
        protected void Awake()
        {
            m_Effect = new T();
            m_EffectPipeline = m_Effect as IPostProcessPipelineCallback<Y>;
            ValidateParameters();
        }
        void OnDidApplyAnimationProperties() => ValidateParameters();       //Undocumented Magic Fucntion ,Triggered By AnimationClip

        void Reset()
        {
            m_Data = UPipeline.GetDefaultPostProcessData<Y>();
            ValidateParameters();
        }
        void OnValidate()
        {
            #if UNITY_EDITOR
            if (m_Effect == null)
            {
                m_Effect = new T();
                m_EffectPipeline = m_Effect as IPostProcessPipelineCallback<Y>;
            }
            #endif
            ValidateParameters();
        }
        protected void OnDestroy()
        {
            m_Effect.Destroy();
            m_Effect = null;
        }

        public void ValidateParameters()
        {
            m_Enabled = m_Data.Validate();
            if (!m_Enabled)
                return;
            ApplyParameters();
        }
        protected virtual void ApplyParameters()
        {
            m_Effect.OnValidate(ref m_Data);
        }
        public void ExecuteBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,RenderTextureDescriptor _descriptor)  => m_Effect.ExecutePostProcessBuffer(_buffer, _src, _dst, _descriptor,ref  m_Data);

        public void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor) => m_EffectPipeline?.Configure(_buffer,_descriptor,ref m_Data);
        public void ExecuteContext(ScriptableRenderer _renderer, ScriptableRenderContext _context, ref RenderingData _renderingData) => m_EffectPipeline?.ExecuteContext(_renderer, _context, ref _renderingData,ref m_Data);
        public void FrameCleanUp(CommandBuffer _buffer) => m_EffectPipeline?.FrameCleanUp(_buffer,ref m_Data);
    }
}