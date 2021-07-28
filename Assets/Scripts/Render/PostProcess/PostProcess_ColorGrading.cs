
using UnityEngine;
using System;
using UnityEditor;

namespace Rendering.ImageEffect
{
    public class PostProcess_ColorGrading : PostProcessComponentBase<PPCore_ColorGrading,PPData_ColorGrading>{}

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
    public struct PPData_ColorGrading 
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
        public static readonly PPData_ColorGrading m_Default = new PPData_ColorGrading()
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

    public class PPCore_ColorGrading : PostProcessCore<PPData_ColorGrading>
    {
        #region ShaderProperties
        static readonly int ID_Weight = Shader.PropertyToID("_Weight");

        const string KW_LUT = "_LUT";
        static readonly int ID_LUT = Shader.PropertyToID("_LUTTex");
        readonly int ID_LUTCellCount = Shader.PropertyToID("_LUTCellCount");

        const string KW_BSC = "_BSC";
        static readonly int ID_Brightness = Shader.PropertyToID("_Brightness");
        static readonly int ID_Saturation = Shader.PropertyToID("_Saturation");
        static readonly int ID_Contrast = Shader.PropertyToID("_Contrast");

        const string KW_MixChannel = "_CHANNEL_MIXER";
        static readonly int ID_MixRed = Shader.PropertyToID("_MixRed");
        static readonly int ID_MixGreen = Shader.PropertyToID("_MixGreen");
        static readonly int ID_MixBlue = Shader.PropertyToID("_MixBlue");
        #endregion
        public override void OnValidate(ref PPData_ColorGrading _data)
        {
            base.OnValidate(ref _data);
            m_Material.SetFloat(ID_Weight, _data.m_Weight);

            m_Material.EnableKeyword(KW_LUT, _data.m_LUT);
            m_Material.SetTexture(ID_LUT, _data.m_LUT);
            m_Material.SetInt(ID_LUTCellCount, (int)_data.m_LUTCellCount);

            m_Material.EnableKeyword(KW_BSC, _data.m_brightness != 1 || _data.m_saturation != 1f || _data.m_contrast != 1);
            m_Material.SetFloat(ID_Brightness, _data.m_brightness);
            m_Material.SetFloat(ID_Saturation, _data.m_saturation);
            m_Material.SetFloat(ID_Contrast, _data.m_contrast);

            m_Material.EnableKeyword(KW_MixChannel, _data.m_MixRed != Vector3.zero || _data.m_MixBlue != Vector3.zero || _data.m_MixGreen != Vector3.zero);
            m_Material.SetVector(ID_MixRed, _data.m_MixRed+Vector3.right);
            m_Material.SetVector(ID_MixGreen, _data.m_MixGreen+Vector3.up);
            m_Material.SetVector(ID_MixBlue, _data.m_MixBlue+Vector3.forward);
        }
    }
}