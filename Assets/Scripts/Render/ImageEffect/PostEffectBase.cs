using System;
using Rendering;
using UnityEngine;

namespace Rendering
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CameraRenderEffectManager))]
    public class PostEffectBase : MonoBehaviour      //Only Use Serialization!
    {
        protected AImageEffectBase m_Effects { get; private set; }
        protected virtual AImageEffectBase OnGenerateRequiredImageEffects() => throw new Exception("Override This Please");

        private void Awake() => Init();
        private void OnDestroy() => Destroy();
        public void OnValidate()
        {
            Init();
        }
        void Init()
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
            if(m_Effects==null)
            {
                Graphics.Blit(src,dst);
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