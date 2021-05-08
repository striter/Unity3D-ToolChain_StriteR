
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.ImageEffect
{
    [Serializable]
    public class ImageEffectBase<T> where T: struct
    {
        Material m_Material;
        public ImageEffectBase()
        {
            m_Material = UPipeline.CreateMaterial(this.GetType());
        }
        public virtual void Destroy()
        {
            if (m_Material)
                GameObject.DestroyImmediate(m_Material);

            m_Material = null;
        }

        public void DoValidate(T _data)
        {
            if (m_Material == null)
                return;
            OnValidate(_data, m_Material);
        }
        protected virtual void OnValidate(T _params, Material _material)
        {

        }
        public void ExecuteBuffer(CommandBuffer _buffer,RenderTextureDescriptor _descriptor, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, T _data)
        {
            if (m_Material == null)
            {
                _buffer.Blit(_src, _dst);
                return;
            }
            OnExecuteBuffer(_buffer, _descriptor, _src, _dst, m_Material, _data);
        }
        protected virtual void OnExecuteBuffer(CommandBuffer _buffer, RenderTextureDescriptor _descriptor, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, Material _material, T _param)
        {
            _buffer.Blit(_src, _dst, _material);
        }
    }
    [ExecuteInEditMode,RequireComponent(typeof(Camera))]
    public abstract class APostEffectBase:MonoBehaviour
    {
        public abstract bool m_IsOpaqueProcess { get; }
        public abstract void ExecutePostProcess(CommandBuffer _buffer, RenderTextureDescriptor _descriptor, RenderTargetIdentifier _src, RenderTargetIdentifier _dst);
        public abstract void OnValidate();
    }

    public partial class PostEffectBase<T,Y> : APostEffectBase where T : ImageEffectBase<Y>, new() where Y:struct
    {
        [MTitle] public Y m_EffectData;
        protected T m_Effect { get; private set; }
        protected Camera m_Camera { get; private set; }
        protected void Awake()=>Init();
        protected void OnDestroy()=>Destroy();
        public override void OnValidate() => m_Effect?.DoValidate(m_EffectData);
        void OnDidApplyAnimationProperties() => OnValidate();       //Undocumented Magic Fucntion ,Triggered By AnimationClip
        void Reset() => m_EffectData = UPipeline.GetDefaultPostProcessData<Y>();
        void Init()
        {
            if (m_Effect == null)
            {
                m_Camera = GetComponent<Camera>();
                m_Effect = new T();
                OnEffectCreate(m_Effect);
            }
            OnValidate();
        }
        public override bool m_IsOpaqueProcess => false;
        public override void ExecutePostProcess(CommandBuffer _buffer,RenderTextureDescriptor _descriptor, RenderTargetIdentifier _src, RenderTargetIdentifier _dst)
        {
            if (m_Effect == null)
            {
                _buffer.Blit(_src, _dst);
                return;
            }

            m_Effect.ExecuteBuffer(_buffer,_descriptor, _src, _dst, m_EffectData);
        }
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
    public partial class PostEffectBase<T, Y> : APostEffectBase where T : ImageEffectBase<Y>, new() where Y : struct
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
            if (m_SceneCameraEffect)
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
