using System.Collections;
using System.Collections.Generic;
using Rendering;
using UnityEngine;

[RequireComponent(typeof(CameraRenderEffectManager))]
public class PostEffect_DepthOfField : PostEffectBase
{
    [SerializeField,Tooltip("景深采样参数")]
    public ImageEffectParams_DepthOfField m_DepthOfFieldParams;
    [SerializeField, Tooltip("采样图模糊参数")]
    public ImageEffectParams_Blurs m_BlurParams;
    protected override AImageEffectBase OnGenerateRequiredImageEffects() => new ImageEffect_DepthOfField(() => m_DepthOfFieldParams,()=> m_BlurParams);
}
