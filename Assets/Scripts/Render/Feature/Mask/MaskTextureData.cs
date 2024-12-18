using System;
using UnityEngine;

namespace Rendering.Pipeline.Mask
{
    public enum EOutlineVertex
    {
        _NORMALSAMPLE_NORMAL,
        _NORMALSAMPLE_TANGENT,
        _NORMALSAMPLE_UV1,
        _NORMALSAMPLE_UV2,
        _NORMALSAMPLE_UV3,
        _NORMALSAMPLE_UV4,
        _NORMALSAMPLE_UV5,
        _NORMALSAMPLE_UV6,
        _NORMALSAMPLE_UV7
    }

    [Serializable]
    public struct SRD_MaskData
    {
        [CullingMask]public int renderMask;
        [Header("Misc")]
        public Color color;

        public bool inheritDepth;
        public bool outline;
        [MFoldout(nameof(outline),true)] [Range(0,1)]public float extendWidth;
        [MFoldout(nameof(outline),true)] public EOutlineVertex outlineVertex;
        public static readonly SRD_MaskData kDefault = new SRD_MaskData()
        {
            renderMask=int.MaxValue,
            color = Color.white,
            outline = false,
            extendWidth = 0.1f,
            inheritDepth = true,
            outlineVertex  = EOutlineVertex._NORMALSAMPLE_NORMAL,
        };
    }
}