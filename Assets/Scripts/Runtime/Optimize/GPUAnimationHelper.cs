using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.Optimize
{
    public static class GPUAnimationHelper
    {
        
        #region ShaderProperties
        static readonly int ID_AnimationTex = Shader.PropertyToID("_AnimTex");
        private static readonly string[] KW_Modes ={"_ANIM_VERTEX","_ANIM_BONE"};
        static readonly int ID_FrameBegin = Shader.PropertyToID("_AnimFrameBegin");
        static readonly int ID_FrameEnd = Shader.PropertyToID("_AnimFrameEnd");
        static readonly int ID_FrameInterpolate = Shader.PropertyToID("_AnimFrameInterpolate");
        #endregion
        
        public static void ApplyMaterial(this GPUAnimationData _data,Material _sharedMaterial)
        {
            _sharedMaterial.SetTexture(ID_AnimationTex,_data.m_BakeTexture);
            _sharedMaterial.EnableKeywords(KW_Modes,_data.m_Mode);
        }

        public static void ApplyPropertyBlock(this AnimationTickerOutput _output, MaterialPropertyBlock _block)
        {
            _block.SetInt(ID_FrameBegin, _output.cur);
            _block.SetInt(ID_FrameEnd, _output.next);
            _block.SetFloat(ID_FrameInterpolate, _output.interpolate);
        }
    }
}