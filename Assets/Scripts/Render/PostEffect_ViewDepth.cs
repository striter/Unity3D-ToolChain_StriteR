using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.ImageEffect
{
    public class PostEffect_ViewDepth : PostEffectBase<CameraEffect_ViewDepth>
    {
        protected override CameraEffect_ViewDepth OnGenerateRequiredImageEffects() => new CameraEffect_ViewDepth();
    }
    public class CameraEffect_ViewDepth : ImageEffectBase<ImageEffectParamBase>
    {
        public CameraEffect_ViewDepth() : base(() => ImageEffectParamBase.m_Default)
        {
        }
    }
}
