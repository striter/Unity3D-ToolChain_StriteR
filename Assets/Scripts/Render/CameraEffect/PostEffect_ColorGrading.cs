
using UnityEngine;
using System;
using UnityEditor;

namespace Rendering.ImageEffect
{
    public class PostEffect_ColorGrading : PostEffectBase<ImageEffect_ColorGrading,ImageEffectParam_ColorGrading>{}

    public enum enum_MixChannel
    {
        None = 0,
        Red = 1,
        Green = 2,
        Blue = 3,
    }
    public enum enum_LUTCellCount
    {
        _16 = 16,
        _32 = 32,
        _64 = 64,
        _128 = 128,
    }

    [System.Serializable]
    public struct ImageEffectParam_ColorGrading 
    {
        [Range(0, 1)]public float m_Weight;

        [Header("LUT")]
        public Texture2D m_LUT ;
        public enum_LUTCellCount m_LUTCellCount ;

        [Header("BSC")]
        [Range(0, 2)]public float m_brightness ;
        [Range(0, 2)] public float m_saturation ;
        [Range(0, 2)]public float m_contrast ;

        [Header("Channel Mixer")]
        [RangeVector(-1, 1)] public Vector3 m_MixRed;
        [RangeVector(-1, 1)] public Vector3 m_MixGreen;
        [RangeVector(-1, 1)] public Vector3 m_MixBlue;
        public static readonly ImageEffectParam_ColorGrading m_Default = new ImageEffectParam_ColorGrading()
        {
            m_Weight=1f ,
            
            m_LUTCellCount = enum_LUTCellCount._16,
            m_brightness = 1,
            m_saturation = 1,
            m_contrast = 1,
            
            m_MixRed = Vector3.zero,
            m_MixGreen = Vector3.zero,
            m_MixBlue = Vector3.zero,
    };
    }

    public class ImageEffect_ColorGrading : ImageEffectBase<ImageEffectParam_ColorGrading>
    {
        #region ShaderProperties
        readonly int ID_Weight = Shader.PropertyToID("_Weight");

        const string KW_LUT = "_LUT";
        readonly int ID_LUT = Shader.PropertyToID("_LUTTex");
        readonly int ID_LUTCellCount = Shader.PropertyToID("_LUTCellCount");

        const string KW_BSC = "_BSC";
        readonly int ID_Brightness = Shader.PropertyToID("_Brightness");
        readonly int ID_Saturation = Shader.PropertyToID("_Saturation");
        readonly int ID_Contrast = Shader.PropertyToID("_Contrast");

        const string KW_MixChannel = "_CHANNEL_MIXER";
        readonly int ID_MixRed = Shader.PropertyToID("_MixRed");
        readonly int ID_MixGreen = Shader.PropertyToID("_MixGreen");
        readonly int ID_MixBlue = Shader.PropertyToID("_MixBlue");
        #endregion
        protected override void OnValidate(ImageEffectParam_ColorGrading _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetFloat(ID_Weight, _params.m_Weight);

            _material.EnableKeyword(KW_LUT, _params.m_LUT);
            _material.SetTexture(ID_LUT, _params.m_LUT);
            _material.SetInt(ID_LUTCellCount, (int)_params.m_LUTCellCount);

            _material.EnableKeyword(KW_BSC, _params.m_brightness != 1 || _params.m_saturation != 1f || _params.m_contrast != 1);
            _material.SetFloat(ID_Brightness, _params.m_brightness);
            _material.SetFloat(ID_Saturation, _params.m_saturation);
            _material.SetFloat(ID_Contrast, _params.m_contrast);

            _material.EnableKeyword(KW_MixChannel, _params.m_MixRed != Vector3.zero || _params.m_MixBlue != Vector3.zero || _params.m_MixGreen != Vector3.zero);
            _material.SetVector(ID_MixRed, _params.m_MixRed+Vector3.right);
            _material.SetVector(ID_MixGreen, _params.m_MixGreen+Vector3.up);
            _material.SetVector(ID_MixBlue, _params.m_MixBlue+Vector3.forward);
        }
    }
}