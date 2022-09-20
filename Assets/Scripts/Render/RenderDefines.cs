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
}