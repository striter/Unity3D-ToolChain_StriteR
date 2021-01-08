
using System;
using UnityEditor;
using UnityEngine;
namespace Rendering.ImageEffect
{
    public abstract class AImageEffectBase
    {
        public abstract void DoImageProcess(RenderTexture src, RenderTexture dst);
        public abstract void DoValidate();
        public virtual void OnDestroy() { }
        public static Material CreateMaterial(Type _type)
        {
            Shader _shader = Shader.Find("Hidden/" + _type.Name);

            if (_shader == null)
                throw new NullReferenceException("Invalid ImageEffect Shader Found:" + _type.Name);

            if (!_shader.isSupported)
                throw new NullReferenceException("Shader Not Supported:" + _type.Name);

            return new Material(_shader) { name = _type.Name, hideFlags = HideFlags.DontSave };
        }
    }

    [Serializable]
    public class ImageEffectBase<T>:AImageEffectBase where T:ImageEffectParamBase
    {
        Material m_Material;
        Func<T> GetParamsFunc;
        public T GetParams() => GetParamsFunc();
        public ImageEffectBase(Func<T> _GetParams)
        {
            m_Material = CreateMaterial(this.GetType()); 
            GetParamsFunc = _GetParams;
        }
        
        public virtual void OnDestory()
        {
            if (m_Material)
                GameObject.DestroyImmediate(m_Material);

            m_Material = null;
            GetParamsFunc = null;
        }

        public override void DoImageProcess(RenderTexture _src, RenderTexture _dst)
        {
            T param = GetParams();
            if (m_Material != null && param != null)
                OnImageProcess(_src, _dst, m_Material, param);
            else
                Graphics.Blit(_src, _dst);
        }


        protected virtual void OnImageProcess(RenderTexture _src,RenderTexture _dst,Material _material, T _param)
        {
            Graphics.Blit(_src, _dst, _material);
        }

        public override void DoValidate()
        {
            T param = GetParams();
            if (param==null||m_Material==null)
                return;
            OnValidate(GetParams(),m_Material);
        }
        protected virtual void OnValidate(T _params,Material _material)
        {

        }
    }

    [Serializable]
    public class ImageEffectParamBase
    {
        public static readonly ImageEffectParamBase m_Default = new ImageEffectParamBase();
    }

    [ExecuteInEditMode,DisallowMultipleComponent,RequireComponent(typeof(Camera))]
    public partial class PostEffectBase<T> : MonoBehaviour where T:AImageEffectBase
    {
        protected T m_Effect { get; private set; }
        protected virtual T OnGenerateRequiredImageEffects() => throw new Exception("Override This Please");
        protected virtual void Awake()
        {
            Init();
        }
        protected virtual void OnDestroy()
        {
            Destroy();
        }

        public virtual void OnValidate()
        {
            Init();
        }

        void Init()
        {
            if (m_Effect == null)
                m_Effect = OnGenerateRequiredImageEffects();

            m_Effect.DoValidate();
        }

        void Destroy()
        {
            if (m_Effect == null)
                return;

            m_Effect.OnDestroy();
            m_Effect = null;
        }

        public void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (m_Effect == null)
            {
                Graphics.Blit(src, dst);
                return;
            }

            m_Effect.DoImageProcess(src, dst);
        }

#if UNITY_EDITOR
        public bool m_SceneViewPreview = false;
        PostEffectBase<T> m_SceneCameraEffect = null;
        bool EditorInitAvailable() => SceneView.lastActiveSceneView && SceneView.lastActiveSceneView.camera.gameObject != this.gameObject;
        private void Update()
        {
            if (!EditorInitAvailable())
                return;

            m_Effect?.DoValidate();
            m_SceneCameraEffect?.OnValidate();
            if (m_SceneCameraEffect)
            {
                m_SceneCameraEffect.OnValidate();
                m_SceneCameraEffect.enabled = m_SceneViewPreview;
                return;
            }
            m_SceneCameraEffect = SceneView.lastActiveSceneView.camera.GetComponent(this.GetType()) as PostEffectBase<T>;
            if (!m_SceneCameraEffect)
                m_SceneCameraEffect = SceneView.lastActiveSceneView.camera.gameObject.AddComponent(this.GetType()) as PostEffectBase<T>;
            m_SceneCameraEffect.hideFlags = HideFlags.HideAndDontSave;
            m_SceneCameraEffect.m_Effect = m_Effect;
        }
        private void OnDisable()
        {
            if (!EditorInitAvailable())
                return;
            if (!m_SceneCameraEffect)
                return;

            GameObject.DestroyImmediate(m_SceneCameraEffect);
        }
#endif
    }
}