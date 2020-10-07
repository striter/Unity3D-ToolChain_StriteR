
using System;
using UnityEditor;
using UnityEngine;
namespace Rendering.ImageEffect
{
    public abstract class AImageEffectBase
    {
        public virtual void OnImageProcess(RenderTexture src, RenderTexture dst) { }
        public abstract void DoValidate();
        public virtual void OnDestroy() { }
    }
    public class ImageEffectBase<T>:AImageEffectBase where T:ImageEffectParamBase
    {
        public Material m_Material { get; private set; }
        protected virtual string m_ShaderLocation => "Override This Please";
        Func<T> GetParamsFunc;
        public T GetParams() => GetParamsFunc();
        public ImageEffectBase(Func<T> _GetParams)
        {
            Type _type=this.GetType();
            Shader _shader = Shader.Find("Hidden/" + _type.Name);

            if (_shader == null)
                throw new NullReferenceException("Invalid ImageEffect Shader Found:" + _type.Name);

            if (!_shader.isSupported)
                throw new NullReferenceException("Shader Not Supported:" + _type.Name);

            m_Material =   new Material(_shader) { name=_type.Name,hideFlags =  HideFlags.DontSave};
            GetParamsFunc = _GetParams;
        }

        public virtual void OnDestory()
        {
            if (m_Material)
                GameObject.DestroyImmediate(m_Material);

            m_Material = null;
            GetParamsFunc = null;
        }
        public override void OnImageProcess( RenderTexture src, RenderTexture dst)
        {
            if (m_Material != null)
                Graphics.Blit(src, dst, m_Material);
            else
                Graphics.Blit(src, dst);
        }

        public override void DoValidate()
        {
            T param = GetParams();
            if (param==null)
                return;
            OnValidate(GetParams());
        }
        protected virtual void OnValidate(T _params)
        {

        }
    }

    public class ImageEffectParamBase
    {
        public static readonly ImageEffectParamBase m_Default = new ImageEffectParamBase();
    }


    [ExecuteInEditMode]
    public class PostEffectBase<T> : MonoBehaviour where T:AImageEffectBase
    {
        protected T m_Effects { get; private set; }
        protected virtual T OnGenerateRequiredImageEffects() => throw new Exception("Override This Please");

        protected virtual void Awake() 
        {
            DoInit();
        }
        protected virtual void OnDestroy()
        {
            Destroy();
        }
        public virtual void OnValidate()
        {
            DoInit();
        }
        void DoInit()
        {
            Destroy();
            m_Effects = OnGenerateRequiredImageEffects();
            m_Effects.DoValidate();
        }

        void Destroy()
        {
            if (m_Effects == null)
                return;

            m_Effects.OnDestroy();
            m_Effects = null;
        }
        public void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (m_Effects == null)
            {
                Graphics.Blit(src, dst);
                return;
            }

#if UNITY_EDITOR
            //保存场景时Material会重置 需要重新设置属性
            m_Effects.DoValidate();
#endif

            m_Effects.OnImageProcess(src, dst);
        }
    }
}