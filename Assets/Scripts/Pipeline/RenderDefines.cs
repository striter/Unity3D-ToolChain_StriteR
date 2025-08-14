using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering.Pipeline
{
    public enum EDownSample
    {
        None = -1,
        Half = 2,
        Quarter = 4,
        Eighth = 8
    }

    public static class KRenderTextures
    {
        public static readonly int kCameraNormalTex = Shader.PropertyToID("_CameraNormalTexture");
        public static readonly RenderTargetIdentifier kRTCameraNormalTex = new RenderTargetIdentifier(kCameraNormalTex);

        public static readonly int kCameraMotionVector = Shader.PropertyToID("_CameraMotionVectorTexture");
        public static readonly RenderTargetIdentifier kCameraMotionVectorRT = new RenderTargetIdentifier(kCameraMotionVector);
        
        public static readonly int kCameraLightMask = Shader.PropertyToID("_CameraLightMaskTexture");
        public static readonly RenderTargetIdentifier kCameraLightMaskRT = new RenderTargetIdentifier(kCameraLightMask);
        
        public static readonly int kCameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
        public static readonly RenderTargetIdentifier kCameraDepthTextureRT = new RenderTargetIdentifier(kCameraDepthTexture);
    }
}