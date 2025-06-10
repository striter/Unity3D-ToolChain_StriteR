using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    [ExecuteAlways]
    public abstract class APostProcessBehaviour<EffectCore,EffectData> : MonoBehaviour , IPostProcessBehaviour  where EffectCore : PostProcessCore<EffectData>, new() where EffectData:struct,IPostProcessParameter
    {
        [SerializeField] protected EffectData m_Data;
        public EffectData GetData() => m_Data;
        public void SetEffectData(EffectData _data)
        {
            m_Data = _data;
        }
        protected EffectCore m_Effect { get; private set; }
        private static readonly string kDefaultName = typeof(EffectCore).Name;
        public string m_Name => kDefaultName;
        public abstract bool OpaqueProcess { get; }
        public abstract EPostProcess Event { get; }
        protected virtual void Awake()
        {
            m_Effect = new ();
        }
        
        protected virtual void OnDestroy()
        {
            m_Effect.Destroy();
            m_Effect = null;
        }
        
        void Reset() => SetEffectData(UReflection.GetDefaultData<EffectData>());
#if UNITY_EDITOR
        void OnValidate()
        {
            m_Effect ??= new();
        }
#endif

        public virtual bool Validate(ref RenderingData _renderingData) => m_Effect.Validate(ref _renderingData,ref m_Data);
        public virtual void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor) => m_Effect.Configure(_buffer,_descriptor,ref m_Data);
        
        public virtual void Execute(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,RenderTextureDescriptor _executeData, 
            ScriptableRenderContext _context,ref RenderingData _renderingData)=>
            m_Effect.Execute(_executeData,ref  m_Data,_buffer, _src, _dst , _context, ref _renderingData);

        public virtual void FrameCleanUp(CommandBuffer _buffer) =>  m_Effect.FrameCleanUp(_buffer,ref m_Data);
    }
}