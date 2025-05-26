using System;
using Rendering.PostProcess;
using UnityEngine;

namespace Rendering.Pipeline
{
    [Serializable]
    public struct ScreenSpaceReflectionData
    {
        [Range(1,4)] public int downSample;
        public DBlurs blurData;

        public static readonly ScreenSpaceReflectionData kDefault = new ScreenSpaceReflectionData()
        {
            downSample = 1,
            blurData = DBlurs.kDefault,
        };
    }
}