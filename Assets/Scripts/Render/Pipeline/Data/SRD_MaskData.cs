using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.Pipeline
{
    [Serializable]
    public struct SRD_MaskData
    {
        [CullingMask]public int cullingMask;

        public static readonly SRD_MaskData kDefault = new SRD_MaskData()
        {
            cullingMask=int.MaxValue,
        };

    }
}