
using System;
using UnityEngine;
namespace Rendering.ImageEffect
{
    [Serializable]
    public class ImageEffectBase<T> where T: struct
    {
        Material m_Material;
        public ImageEffectBase()
        {
            m_Material = URender.CreateMaterial(this.GetType());
        }
        public virtual void Destroy()
        {
            if (m_Material)
                GameObject.DestroyImmediate(m_Material);

            m_Material = null;
        }


        public void DoImageProcess(RenderTexture _src, RenderTexture _dst,T _data)
        {
            if (m_Material == null)
            {
                Graphics.Blit(_src, _dst);
                return;
            }
            OnImageProcess(_src, _dst, m_Material, _data);
        }
        public void DoValidate(T _data)
        {
            if (m_Material == null)
                return;
            OnValidate(_data, m_Material);
        }

        protected virtual void OnImageProcess(RenderTexture _src,RenderTexture _dst,Material _material, T _param)
        {
            Graphics.Blit(_src, _dst, _material);
        }

        protected virtual void OnValidate(T _params,Material _material)
        {

        }
    }

    [DisallowMultipleComponent,RequireComponent(typeof(Camera))]
    public partial class PostEffectBase<T,Y> : MonoBehaviour where T : ImageEffectBase<Y>, new() where Y:struct
    {
        public Y m_EffectData;
        protected T m_Effect { get; private set; }
        protected Camera m_Camera { get; private set; }
        protected virtual void Awake()=>Init();
        protected virtual void OnDestroy()=>Destroy();
        public virtual void OnValidate() => m_Effect?.DoValidate(m_EffectData);
        void OnDidApplyAnimationProperties() => OnValidate();       //Undocumented Magic Fucntion ,Triggered By AnimationClip
        void Reset() => m_EffectData = (Y) typeof(Y).GetField("m_Default", System.Reflection.BindingFlags.Static| System.Reflection.BindingFlags.Public).GetValue(null);     //Get Default Value By Reflection
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


        protected virtual void OnEffectCreate(T _effect) { }

        void Destroy()
        {
            if (m_Effect == null)
                return;

            m_Effect.Destroy();
            m_Effect = null;
        }

        protected void OnRenderImage(RenderTexture _src, RenderTexture _dst)
        {
            if (m_Effect == null)
            {
                Graphics.Blit(_src, _dst);
                return;
            }

            m_Effect.DoImageProcess(_src, _dst,m_EffectData);
        }
    }

#if UNITY_EDITOR
    #region Editor Preview
    [ExecuteInEditMode]
    public partial class PostEffectBase<T, Y> : MonoBehaviour where T : ImageEffectBase<Y>, new() where Y : struct
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