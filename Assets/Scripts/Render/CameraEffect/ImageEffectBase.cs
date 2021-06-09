using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.ImageEffect
{
    [Serializable]
    public class ImageEffectBase<T> where T: struct
    {
        protected Material m_Material { get; private set; }
        public ImageEffectBase()
        {
            Create();
        }
        public virtual void Create()
        {
            m_Material = UPipeline.CreateMaterial(this.GetType());
        }
        public virtual void Destroy()
        {
            if (m_Material)
                GameObject.DestroyImmediate(m_Material);

            m_Material = null;
        }

        public virtual void OnValidate(T _data) 
        {  
        
        }
        public virtual void ExecutePostProcessBuffer( CommandBuffer _buffer,  RenderTargetIdentifier _src, RenderTargetIdentifier _dst,RenderTextureDescriptor _descriptor, T _data)
        {
            _buffer.Blit(_src, _dst, m_Material);
        }
    }

    [ExecuteInEditMode, RequireComponent(typeof(Camera))]
    public abstract class APostProcessBase:MonoBehaviour
    {
        public abstract bool m_IsOpaqueProcess { get; }
        public abstract void Configure(ScriptableRenderer _renderer, CommandBuffer _buffer, RenderTextureDescriptor _descriptor,ScriptableRenderPass _pass);
        public abstract void Execute(ScriptableRenderer _renderer, ScriptableRenderContext _context, ref RenderingData _renderingData);
        public abstract void ExecuteBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _executeData);
        public abstract void FrameCleanUp(CommandBuffer _buffer);
        public abstract void OnValidate();
    }
    public interface ImageEffectPipeline<T> where T:struct
    {
        public abstract void Configure(ScriptableRenderer _renderer, CommandBuffer _buffer, RenderTextureDescriptor _descriptor, ScriptableRenderPass _pass, T _data);
        public abstract void Execute(ScriptableRenderer _renderer, ScriptableRenderContext _context, ref RenderingData _renderingData,T _data);
        public abstract void FrameCleanUp(CommandBuffer _buffer,T _data);
    }

    public partial class PostEffectBase<T,Y> : APostProcessBase where T : ImageEffectBase<Y>, new() where Y:struct
    {
        [MTitle] public Y m_EffectData;
        public T m_Effect { get; private set; }
        public ImageEffectPipeline<Y> m_EffectPipeline { get; private set; }
        protected Camera m_Camera { get; private set; }
        protected void Awake()=>Init();
        protected void OnDestroy()=>Destroy();
        public override void OnValidate() => m_Effect?.OnValidate(m_EffectData);
        void OnDidApplyAnimationProperties() => OnValidate();       //Undocumented Magic Fucntion ,Triggered By AnimationClip
        void Reset() => m_EffectData = UPipeline.GetDefaultPostProcessData<Y>();
        void Init()
        {
            if (m_Effect == null)
            {
                m_Camera = GetComponent<Camera>();
                m_Effect = new T();
                m_EffectPipeline = m_Effect as ImageEffectPipeline<Y>;
                OnEffectCreate(m_Effect);
            }
            OnValidate();
        }
        public override bool m_IsOpaqueProcess => false;
        public override void Configure(ScriptableRenderer _renderer, CommandBuffer _buffer, RenderTextureDescriptor _descriptor, ScriptableRenderPass _pass) => m_EffectPipeline?.Configure(_renderer, _buffer,_descriptor,_pass,m_EffectData);
        public override void Execute(ScriptableRenderer _renderer, ScriptableRenderContext _context, ref RenderingData _renderingData) => m_EffectPipeline?.Execute(_renderer, _context, ref _renderingData,m_EffectData);
        public override void ExecuteBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,RenderTextureDescriptor _descriptor)  => m_Effect?.ExecutePostProcessBuffer(_buffer, _src, _dst, _descriptor, m_EffectData);
        public override void FrameCleanUp(CommandBuffer _buffer) => m_EffectPipeline?.FrameCleanUp(_buffer,m_EffectData);
        void Destroy()
        {
            if (m_Effect == null)
                return;

            m_Effect.Destroy();
            OnEffectDestroy();
            m_Effect = null;
        }
        protected virtual void OnEffectCreate(T _effect) { }
        protected virtual void OnEffectDestroy() { }
    }


#if UNITY_EDITOR
    #region Editor Preview
    [ExecuteInEditMode]
    public partial class PostEffectBase<T, Y> : APostProcessBase where T : ImageEffectBase<Y>, new() where Y : struct
    {
        public bool m_SceneViewPreview = false;
        PostEffectBase<T, Y> m_SceneCameraEffect = null;
        bool EditorInitAvailable() => UnityEditor.SceneView.lastActiveSceneView && UnityEditor.SceneView.lastActiveSceneView.camera.gameObject != this.gameObject;
        void Update()
        {
            if (!EditorInitAvailable())
                return;
            Init();
            if (m_SceneCameraEffect)
            {
                m_SceneCameraEffect.m_EffectData = m_EffectData;
                m_SceneCameraEffect.OnValidate();
            }
            if (m_SceneViewPreview)
                InitSceneCameraEffect();
            else
                RemoveSceneCameraEffect();
        }
        void OnDisable()
        {
            RemoveSceneCameraEffect();
        }
        void InitSceneCameraEffect()
        {
            if (m_SceneCameraEffect||UnityEditor.SceneView.lastActiveSceneView==null)
                return;
            m_SceneCameraEffect = UnityEditor.SceneView.lastActiveSceneView.camera.gameObject.AddComponent(this.GetType()) as PostEffectBase<T, Y>;
            m_SceneCameraEffect.hideFlags = HideFlags.HideAndDontSave;
            m_SceneCameraEffect.m_EffectData = m_EffectData;
            m_SceneCameraEffect.Init();
        }
        void RemoveSceneCameraEffect()
        {
            if (!m_SceneCameraEffect)
                return;
            GameObject.DestroyImmediate(m_SceneCameraEffect);
            m_SceneCameraEffect = null;
        }
    }
    #endregion
#endif
}
