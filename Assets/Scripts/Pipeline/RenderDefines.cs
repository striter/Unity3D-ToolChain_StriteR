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

    public enum EShadowMapResolution
    {
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192
    }

    public static class KShaderTagId
    {
        public static ShaderTagId kSRPDefaultUnlit = new ShaderTagId("SRPDefaultUnlit");
        public static ShaderTagId kSRPDefaultLit = new ShaderTagId("SRPDefaultLit");
        public static ShaderTagId kLightweightForward = new ShaderTagId("LightweightForward");
        public static ShaderTagId kUniversalForwardOnly = new ShaderTagId("UniversalForwardOnly");
        public static ShaderTagId kUniversalForward = new ShaderTagId("UniversalForward");
        public static ShaderTagId kShadowCaster = new ShaderTagId("ShadowCaster");
        public static ShaderTagId kDepthOnly = new ShaderTagId("DepthOnly");
        public static ShaderTagId kMeta = new ShaderTagId("Meta");
        public static ShaderTagId kWorldNormal = new ShaderTagId("WorldNormal");
        public static ShaderTagId kWorldPosition = new ShaderTagId("WorldPosition");
        public static ShaderTagId kSceneSelectionPassTag = new ShaderTagId("SceneSelectionPass");
        public static ShaderTagId kAlbedoAlpha = new ShaderTagId("AlbedoAlpha");
        public static ShaderTagId kDepthNormals = new ShaderTagId("DepthNormals");
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