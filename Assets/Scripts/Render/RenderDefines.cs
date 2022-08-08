using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering.Pipeline
{
    public static class DRenderTextures
    {
        public static readonly int kCameraMaskTexture = Shader.PropertyToID("_CameraMaskTexture");
        public static readonly RenderTargetIdentifier kCameraMaskTextureRT = new RenderTargetIdentifier(kCameraMaskTexture);
        
        public static readonly int kCameraNormalTex = Shader.PropertyToID("_CameraNormalTexture");
        
    }
    
    public static class DShaderProperties
    {
        public static readonly int kColor=Shader.PropertyToID("_Color");
        public static readonly int kColorMask=Shader.PropertyToID("_ColorMask");
        public static readonly int kZTest=Shader.PropertyToID("_ZTest");
        public static readonly int kCull=Shader.PropertyToID("_Cull");
    }
}