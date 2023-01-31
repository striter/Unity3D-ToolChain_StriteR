using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering.Pipeline
{
    public static class KRenderTextures
    {
        public static readonly int kCameraNormalTex = Shader.PropertyToID("_CameraNormalTexture");
        public static readonly RenderTargetIdentifier kRTCameraNormalTex = new RenderTargetIdentifier(kCameraNormalTex);
        
        public static readonly int kCameraMaskTexture = Shader.PropertyToID("_CameraMaskTexture");
        public static readonly RenderTargetIdentifier kCameraMaskTextureRT = new RenderTargetIdentifier(kCameraMaskTexture);

        public static readonly int kCameraMotionVector = Shader.PropertyToID("_CameraMotionVectorTexture");
        public static readonly RenderTargetIdentifier kCameraMotionVectorRT = new RenderTargetIdentifier(kCameraMotionVector);
        
        public static readonly int kCameraLightMask = Shader.PropertyToID("_CameraLightMaskTexture");
        public static readonly RenderTargetIdentifier kCameraLightMaskRT = new RenderTargetIdentifier(kCameraLightMask);
    }
}