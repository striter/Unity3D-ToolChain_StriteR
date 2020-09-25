using UnityEngine;
namespace Rendering
{
    [RequireComponent(typeof(CameraRenderEffectManager))]
    public class PostEffect_ColorGrading : PostEffectBase
    {
        [SerializeField,Tooltip("颜色分级参数")]
        public ImageEffectParams_ColorGrading m_Params;
        protected override AImageEffectBase OnGenerateRequiredImageEffects() => new ImageEffect_ColorGrading(()=>m_Params);
    }
}