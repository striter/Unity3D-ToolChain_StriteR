using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    [ExecuteInEditMode]
    public class PostProcessBehaviour<T,Y> : MonoBehaviour,IPostProcessBehaviour where T : PostProcessCore<Y>, new() where Y:struct,IPostProcessParameter
    {
        [Title] public Y m_Data;
        protected T m_Effect { get; private set; }
        private static readonly string kDefaultName = typeof(T).Name;
        public string m_Name => kDefaultName;
        public virtual bool m_OpaqueProcess => throw new NotImplementedException();
        public virtual EPostProcess Event => throw new NotImplementedException();
        public bool m_Enabled { get; private set;}
        protected void Awake()
        {
            m_Effect = new T();
            ValidateParameters();
        }
        void OnDidApplyAnimationProperties() => ValidateParameters();       //Undocumented Magic Fucntion ,Triggered By AnimationClip

        void Reset()
        {
            m_Data = UReflection.GetDefaultData<Y>();
            ValidateParameters();
        }
        void OnValidate()
        {
            #if UNITY_EDITOR
            if (m_Effect == null)
                m_Effect = new T();
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
        
        public void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor) => m_Effect.Configure(_buffer,_descriptor,ref m_Data);
        public virtual void Execute(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,RenderTextureDescriptor _executeData, 
            ScriptableRenderer _renderer, ScriptableRenderContext _context,ref RenderingData _renderingData)=>
            m_Effect.Execute(_executeData,ref  m_Data,_buffer, _src, _dst ,_renderer, _context, ref _renderingData);

        public void FrameCleanUp(CommandBuffer _buffer) =>  m_Effect.FrameCleanUp(_buffer,ref m_Data);
    }
}