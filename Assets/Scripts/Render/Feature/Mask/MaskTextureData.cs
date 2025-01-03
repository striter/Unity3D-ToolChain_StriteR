using System;
using UnityEngine;

namespace Rendering.Pipeline.Mask
{
    [Serializable]
    public struct MaskTextureData
    {
        public bool collectFromProviders ;
        [MFoldout(nameof(collectFromProviders),false),CullingMask]public int renderMask;
        [MFoldout(nameof(collectFromProviders),true)]public Material overrideMaterial;
        [MFoldout(nameof(collectFromProviders),false)]public Shader overrideShader;
        public bool inheritDepth;

        public static MaskTextureData kDefault = new MaskTextureData()
        {
            collectFromProviders = false,
            renderMask = int.MaxValue,
            overrideMaterial = null,
            inheritDepth = true
        };
    }
}