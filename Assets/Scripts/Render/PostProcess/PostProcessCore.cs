using System;
using System.Reflection;
using Rendering.Pipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    public enum EPostProcess
    {
        Opaque=0,
        Volumetric=1,
        DepthOfField=2,
        ColorUpgrade=3,
        ColorDegrade=4,
        Stylize=5,
        Default=6,
    }

    public interface IPostProcessPipeline<T> where T:struct
    {
        void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor,ref  T _data);
        void ExecuteContext(ScriptableRenderer _renderer, ScriptableRenderContext _context, ref RenderingData _renderingData,ref T _data);
        void FrameCleanUp(CommandBuffer _buffer,ref T _data);
    }
    
    [ExecuteInEditMode, RequireComponent(typeof(Camera))]
    public abstract class APostProcessBase:MonoBehaviour
    {
        public abstract bool m_OpaqueProcess { get; }
        public abstract EPostProcess Event { get; }
        public abstract void Configure( CommandBuffer _buffer, RenderTextureDescriptor _descriptor);
        public abstract void ExecuteContext(ScriptableRenderer _renderer, ScriptableRenderContext _context, ref RenderingData _renderingData);
        public abstract void ExecuteBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _executeData);
        public abstract void FrameCleanUp(CommandBuffer _buffer);
        public abstract void OnValidate();
    }

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

    public abstract partial class PostProcessComponentBase<T,Y> : APostProcessBase where T : PostProcessCore<Y>, new() where Y:struct
    {
        public bool m_Preview = false;
        [MTitle] public Y m_Data;
        public T m_Effect { get; private set; }
        public IPostProcessPipeline<Y> m_EffectPipeline { get; private set; }
        protected Camera m_Camera { get; private set; }
        protected void Awake()=>Init();
        protected void OnDestroy()=>Destroy();
        public override void OnValidate() => m_Effect?.OnValidate(ref m_Data);
        void OnDidApplyAnimationProperties() => OnValidate();       //Undocumented Magic Fucntion ,Triggered By AnimationClip
        void Reset() => m_Data = UPipeline.GetDefaultPostProcessData<Y>();
        void Init()
        {
            if (m_Effect == null)
            {
                m_Camera = GetComponent<Camera>();
                m_Effect = new T();
                m_EffectPipeline = m_Effect as IPostProcessPipeline<Y>;
            }
            OnValidate();
        }
        void Destroy()
        {
            if (m_Effect == null)
                return;

            m_Effect.Destroy();
            m_Effect = null;
        }
        public override void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor) => m_EffectPipeline?.Configure(_buffer,_descriptor,ref m_Data);
        public override void ExecuteContext(ScriptableRenderer _renderer, ScriptableRenderContext _context, ref RenderingData _renderingData) => m_EffectPipeline?.ExecuteContext(_renderer, _context, ref _renderingData,ref m_Data);
        public override void ExecuteBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,RenderTextureDescriptor _descriptor)  => m_Effect?.ExecutePostProcessBuffer(_buffer, _src, _dst, _descriptor,ref  m_Data);
        public override void FrameCleanUp(CommandBuffer _buffer) => m_EffectPipeline?.FrameCleanUp(_buffer,ref m_Data);
    }


#if UNITY_EDITOR
    #region Editor Preview
    [ExecuteInEditMode, RequireComponent(typeof(Camera))]
    public abstract partial class PostProcessComponentBase<T, Y> : APostProcessBase where T : PostProcessCore<Y>, new() where Y : struct
    {
        PostProcessComponentBase<T, Y> m_SceneComponent = null;
        private FieldInfo[] m_Fields;

        FieldInfo[] GetFields()
        {
            if(m_Fields==null)
                m_Fields=this.GetType().GetFields(BindingFlags.Instance|BindingFlags.Public);
            return m_Fields;
        }
        bool EditorInitAvailable() => UnityEditor.SceneView.lastActiveSceneView && UnityEditor.SceneView.lastActiveSceneView.camera.gameObject != this.gameObject;
        void Update()
        {
            if (!EditorInitAvailable())
                return;
            Init();
            if (m_SceneComponent)
            {
                foreach (var field in GetFields())
                    field.SetValue(m_SceneComponent,field.GetValue(this));
                m_SceneComponent.OnValidate();
            }
            if (m_Preview)
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
            if (m_SceneComponent||UnityEditor.SceneView.lastActiveSceneView==null)
                return;
            m_SceneComponent = UnityEditor.SceneView.lastActiveSceneView.camera.gameObject.AddComponent(this.GetType()) as PostProcessComponentBase<T, Y>;
            m_SceneComponent.hideFlags = HideFlags.HideAndDontSave;
            m_SceneComponent.m_Data = m_Data;
            m_SceneComponent.Init();
        }
        void RemoveSceneCameraEffect()
        {
            if (!m_SceneComponent)
                return;
            GameObject.DestroyImmediate(m_SceneComponent);
            m_SceneComponent = null;
        }
    }
    #endregion
#endif
}
