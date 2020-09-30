using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Rendering.ImageEffect
{

    public class PostEffect_DistortVortex : PostEffectBase
    {
        public ImageEffectParam_DistortVortex m_Param;
        protected override AImageEffectBase OnGenerateRequiredImageEffects() => new ImageEffect_DistortVortex(()=>m_Param);
    }

    [System.Serializable]
    public class ImageEffectParam_DistortVortex:ImageEffectParamBase
    {
        [Range(0,1)]
        public float m_OriginViewPort_X=.5f;
        [Range(0,1)]
        public float m_OriginViewPort_Y=.5f;
        [Range(-5,5)]
        public float m_OffsetFactor=.1f;
        public Texture2D m_NoiseTex;
        public float m_NoiseStrength=.5f;
    }

    public class ImageEffect_DistortVortex :ImageEffectBase<ImageEffectParam_DistortVortex>
    {
        #region ShaderProperties
        static readonly int ID_NoiseTex = Shader.PropertyToID("_NoiseTex");
        static readonly int ID_NoiseStrength = Shader.PropertyToID("_NoiseStrength");
        static readonly int ID_DistortParam = Shader.PropertyToID("_DistortParam");
        #endregion
        public ImageEffect_DistortVortex(Func<ImageEffectParam_DistortVortex> _GetParam):base(_GetParam)
        {

        }
        protected override void OnValidate(ImageEffectParam_DistortVortex _params)
        {
            base.OnValidate(_params);
            m_Material.SetVector(ID_DistortParam, new Vector4(_params.m_OriginViewPort_X, _params.m_OriginViewPort_Y, _params.m_OffsetFactor));
            m_Material.SetTexture(ID_NoiseTex, _params.m_NoiseTex);
            m_Material.SetFloat(ID_NoiseStrength, _params.m_NoiseStrength);
        }
    }
}
