using System;
using Rendering.PostProcess;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline.GrabPass
{
    public enum EGrabTextureMode
    {
        CopyPass,
        Redraw,
    }
    
    [Serializable]
    public struct GrabTextureData
    {
        public string textureName;
        public EGrabTextureMode mode;
        [Foldout(nameof(mode), EGrabTextureMode.Redraw)] public CullingMask renderMask;
        [Foldout(nameof(mode),EGrabTextureMode.CopyPass)] public RenderPassEvent renderPassEvent;
        public DBlurs blurData;
        [Range(1,4)]public int downSample;
        public static readonly GrabTextureData kDefault = new() {
            mode = EGrabTextureMode.CopyPass,
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques,
            blurData = DBlurs.kNone,
            textureName = "_GrabTexture",
            downSample = 4,
        };
    }

    [Serializable]
    public struct GrabTextureConfig
    {
        public bool blurActive;
        public static readonly GrabTextureConfig kDefault = new() { blurActive = true };
    }
}