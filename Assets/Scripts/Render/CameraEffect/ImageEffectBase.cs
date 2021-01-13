
using System;
using UnityEditor;
using UnityEngine;
namespace Rendering.ImageEffect
{
    [Serializable]
    public class ImageEffectBase<T> where T:ImageEffectParamBase
    {
        Material m_Material;
        public ImageEffectBase()
        {
            m_Material = TRender.CreateMaterial(this.GetType());
        }
        public virtual void Destroy()
        {
            if (m_Material)
                GameObject.DestroyImmediate(m_Material);

            m_Material = null;
        }

        public void DoImageProcess(RenderTexture _src, RenderTexture _dst,T _data)
        {
            if (_data==null||m_Material == null)
            {
                Graphics.Blit(_src, _dst);
                return;
            }
            OnImageProcess(_src, _dst, m_Material, _data);
        }
        public void DoValidate(T _data)
        {
            if (_data==null||m_Material == null)
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
    public class ImageEffectParamBase  {  }

    [ExecuteInEditMode,DisallowMultipleComponent,RequireComponent(typeof(Camera))]
    public partial class PostEffectBase<T,Y> : MonoBehaviour where T : ImageEffectBase<Y>, new() where Y:ImageEffectParamBase
    {
        #region EditorPreview
#if UNITY_EDITOR
        public bool m_SceneViewPreview = false;
        PostEffectBase<T, Y> m_SceneCameraEffect = null;
        bool EditorInitAvailable() => SceneView.lastActiveSceneView && SceneView.lastActiveSceneView.camera.gameObject != this.gameObject;
        void Update()
        {
            m_Effect?.DoValidate(m_EffectData);
            if (!EditorInitAvailable())
                return;

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
            m_SceneCameraEffect = SceneView.lastActiveSceneView.camera.gameObject.AddComponent(this.GetType()) as PostEffectBase<T, Y>;
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
#endif
        #endregion
        public Y m_EffectData;
        protected T m_Effect { get; private set; }
        protected Camera m_Camera { get; private set; }
        protected virtual void Awake()
        {
            Init();
        }
        protected virtual void OnDestroy()
        {
            Destroy();
        }
        public void OnValidate()
        {
            Init();
        }


        void Init()
        {
            if (!m_Camera) m_Camera = GetComponent<Camera>();

            if (m_Effect == null)
            {
                m_Effect = new T();
                OnEffectCreate(m_Effect);
            }

            m_Effect.DoValidate(m_EffectData);
        }

        protected virtual void OnEffectCreate(T _effect) { }

        void Destroy()
        {
            if (m_Effect == null)
                return;

            m_Effect.Destroy();
            m_Effect = null;
        }

        public void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (m_Effect == null)
            {
                Graphics.Blit(src, dst);
                return;
            }

            m_Effect.DoImageProcess(src, dst,m_EffectData);
        }

    }
}