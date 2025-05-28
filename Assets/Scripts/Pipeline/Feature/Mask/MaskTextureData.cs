using System;
using UnityEngine;

namespace Rendering.Pipeline.Mask
{
    public enum EMaskTextureMode
    {
        Redraw = 0,
        ShaderReplacement,
        MaterialReplacement,
        ProviderMaterialReplacement,
    }
    
    [Serializable]
    public struct MaskTextureData
    {
        public EMaskTextureMode mode;
        [Fold(nameof(mode),EMaskTextureMode.ProviderMaterialReplacement),CullingMask] public int renderMask;
        [Foldout(nameof(mode),EMaskTextureMode.MaterialReplacement,EMaskTextureMode.ProviderMaterialReplacement)]public Material overrideMaterial;
        [Foldout(nameof(mode),EMaskTextureMode.ShaderReplacement)] public Shader overrideShader;
        public bool inheritDepth;

        public static MaskTextureData kDefault = new()
        {
            mode = EMaskTextureMode.ShaderReplacement,
            renderMask = -1,
            overrideMaterial = null,
            inheritDepth = true
        };
    }
}