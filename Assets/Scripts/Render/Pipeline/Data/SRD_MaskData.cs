using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.Pipeline
{
    [Serializable]
    public struct SRD_MaskData
    {
        [CullingMask]public int renderMask;
        [Header("Misc")]
        public Color color;
        [Range(0,1)]public float extendWidth;
        public EOutlineVertex outlineVertex;
        public static readonly SRD_MaskData kDefault = new SRD_MaskData()
        {
            renderMask=int.MaxValue,
            color = Color.white,
            extendWidth = 0.1f,
            outlineVertex  = EOutlineVertex._NORMALSAMPLE_NORMAL,
        };
    }
    
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

}