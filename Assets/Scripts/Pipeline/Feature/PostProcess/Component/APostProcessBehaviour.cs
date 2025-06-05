using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    [ExecuteAlways]
    public abstract class APostProcessBehaviour<EffectCore,EffectData> : MonoBehaviour , IPostProcessBehaviour  where EffectCore : PostProcessCore<EffectData>, new() where EffectData:struct,IPostProcessParameter
    {
        public EffectData m_Data;
        protected EffectCore m_Effect { get; private set; }
        private static readonly string kDefaultName = typeof(EffectCore).Name;
        public string m_Name => kDefaultName;
        public abstract bool m_OpaqueProcess { get; }
        public abstract EPostProcess Event { get; }
        public bool m_Enabled { get; private set;}
        private bool m_Dirty;
        public void SetDirty() => m_Dirty = true;

        protected virtual void Awake()
        {
            m_Effect = new ();
            SetDirty();
        }
        
        protected virtual void OnDestroy()
        {
            m_Effect.Destroy();
            m_Effect = null;
        }
        
        void OnDidApplyAnimationProperties() => SetDirty();       //Undocumented Magic Function ,Triggered By AnimationClip

        void Reset()
        {
            m_Data = UReflection.GetDefaultData<EffectData>();
            SetDirty();
        }

        void OnValidate()
        {
#if UNITY_EDITOR
            m_Effect ??= new();
#endif
            SetDirty();
        }
        
        public void ValidateParameters()
        {
            if(!m_Dirty)
                return;
            m_Dirty = false;
            m_Enabled = m_Data.Validate();
            if (!m_Enabled)
                return;
            ApplyParameters();
        }
        protected virtual void ApplyParameters()
        {
            m_Effect.OnValidate(ref m_Data);
        }
        
        public virtual void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor) => m_Effect.Configure(_buffer,_descriptor,ref m_Data);
        
        public virtual void Execute(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,RenderTextureDescriptor _executeData, 
            ScriptableRenderContext _context,ref RenderingData _renderingData)=>
            m_Effect.Execute(_executeData,ref  m_Data,_buffer, _src, _dst , _context, ref _renderingData);

        public virtual void FrameCleanUp(CommandBuffer _buffer) =>  m_Effect.FrameCleanUp(_buffer,ref m_Data);
        
        #if UNITY_EDITOR
        private void OnEnable()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += OnSceneSaving;
        }

        private void OnDisable()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaving -= OnSceneSaving;
        }
        void OnSceneSaving(UnityEngine.SceneManagement.Scene scene, string path) => SetDirty();
        #endif
    }
}