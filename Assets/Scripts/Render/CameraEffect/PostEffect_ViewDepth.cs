using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.ImageEffect
{
    public class PostEffect_ViewDepth : PostEffectBase<CameraEffect_ViewDepth, ImageEffectParam_ViewDepth>
    {
        [ImageEffectOpaque]
        protected new void OnRenderImage(RenderTexture source, RenderTexture destination)=>base.OnRenderImage(source, destination);
    }
    public struct ImageEffectParam_ViewDepth
    {
        public static readonly ImageEffectParam_ViewDepth m_Default = new ImageEffectParam_ViewDepth();
    }
    public class CameraEffect_ViewDepth : ImageEffectBase<ImageEffectParam_ViewDepth> { }
}
