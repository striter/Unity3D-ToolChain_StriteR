using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Rendering.ImageEffect
{

    public class PostProcess_DistortVortex : PostProcessComponentBase<PPCore_DistortVortex, PPData_DistortVortex>
    {
    }

    [Serializable]
    public struct PPData_DistortVortex
    {
        [Range(0, 1)] public float m_OriginViewPort_X;
        [Range(0, 1)] public float m_OriginViewPort_Y;
        [Range(-5, 5)] public float m_OffsetFactor;
        public Texture2D m_NoiseTex;
        public float m_NoiseStrength;
        public static readonly PPData_DistortVortex m_Default = new PPData_DistortVortex()
        {
            m_OriginViewPort_X = .5f,
            m_OriginViewPort_Y = .5f,
            m_OffsetFactor = .1f,
            m_NoiseStrength = .5f,
        };
    }

    public class PPCore_DistortVortex :PostProcessCore<PPData_DistortVortex>
    {
        #region ShaderProperties
        static readonly int ID_NoiseTex = Shader.PropertyToID("_NoiseTex");
        static readonly int ID_NoiseStrength = Shader.PropertyToID("_NoiseStrength");
        static readonly int ID_DistortParam = Shader.PropertyToID("_DistortParam");
        #endregion
        public override void OnValidate(PPData_DistortVortex _data)
        {
            base.OnValidate(_data);
            m_Material.SetVector(ID_DistortParam, new Vector4(_data.m_OriginViewPort_X, _data.m_OriginViewPort_Y, _data.m_OffsetFactor));
            m_Material.SetTexture(ID_NoiseTex, _data.m_NoiseTex);
            m_Material.SetFloat(ID_NoiseStrength, _data.m_NoiseStrength);
        }
    }
}
