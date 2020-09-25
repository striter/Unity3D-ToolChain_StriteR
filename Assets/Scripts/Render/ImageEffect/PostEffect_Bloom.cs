using UnityEngine;
namespace Rendering
{
    [RequireComponent(typeof(CameraRenderEffectManager))]
    public class PostEffect_Bloom : PostEffectBase
    {
        [Tooltip("Bloom采样参数")]
        public ImageEffectParams_Bloom m_BloomParams;
        [Tooltip("采样模糊参数")]
        public ImageEffectParams_Blurs m_BlurParams;

        protected override AImageEffectBase OnGenerateRequiredImageEffects() => new ImageEffect_Bloom(()=>m_BloomParams,()=>m_BlurParams);
    }
}