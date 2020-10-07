using UnityEngine;
using System;
namespace Rendering.ImageEffect
{
    public class PostEffect_ColorGrading : PostEffectBase<ImageEffect_ColorGrading>
    {
        [SerializeField,Tooltip("颜色分级参数")]
        public ImageEffectParam_ColorGrading m_Params;
        protected override ImageEffect_ColorGrading OnGenerateRequiredImageEffects() => new ImageEffect_ColorGrading(()=>m_Params);
    }

    [System.Serializable]
    public class ImageEffectParam_ColorGrading : ImageEffectParamBase
    {
        [Tooltip("总权重"), Range(0, 1)]
        public float m_Weight = 1;

        [Header("LUT_颜色对照表")]
        [Tooltip("颜色对照表")]
        public Texture m_LUT = null;
        [Tooltip("32格/16格")]
        public ImageEffect_ColorGrading.enum_LUTCellCount m_LUTCellCount = ImageEffect_ColorGrading.enum_LUTCellCount._16;

        [Header("BSC_亮度 饱和度 对比度")]
        [Tooltip("亮度"), Range(0, 2)]
        public float m_brightness = 1;
        [Tooltip("饱和度"), Range(0, 2)]
        public float m_saturation = 0;
        [Tooltip("对比度"), Range(0, 2)]
        public float m_contrast = 1;

        [Header("Channel Mixer_通道混合器")]
        [Tooltip("红色通道混合")]
        public Vector3 m_MixRed = Vector3.zero;
        [Tooltip("绿色通道混合")]
        public Vector3 m_MixGreen = Vector3.zero;
        [Tooltip("蓝色通道混合")]
        public Vector3 m_MixBlue = Vector3.zero;
    }

    public class ImageEffect_ColorGrading : ImageEffectBase<ImageEffectParam_ColorGrading>
    {
        public ImageEffect_ColorGrading(Func<ImageEffectParam_ColorGrading> _GetParams) : base(_GetParams) { }
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
        protected override void OnValidate(ImageEffectParam_ColorGrading _params)
        {
            base.OnValidate(_params);
            m_Material.SetFloat(ID_Weight, _params.m_Weight);

            m_Material.EnableKeyword(KW_LUT, _params.m_LUT);
            m_Material.SetTexture(ID_LUT, _params.m_LUT);
            m_Material.SetInt(ID_LUTCellCount, (int)_params.m_LUTCellCount);

            m_Material.EnableKeyword(KW_BSC, _params.m_brightness != 1 || _params.m_saturation != 1f || _params.m_contrast != 1);
            m_Material.SetFloat(ID_Brightness, _params.m_brightness);
            m_Material.SetFloat(ID_Saturation, _params.m_saturation);
            m_Material.SetFloat(ID_Contrast, _params.m_contrast);

            m_Material.EnableKeyword(KW_MixChannel, _params.m_MixRed != Vector3.zero || _params.m_MixBlue != Vector3.zero || _params.m_MixGreen != Vector3.zero);
            m_Material.SetVector(ID_MixRed, _params.m_MixRed);
            m_Material.SetVector(ID_MixGreen, _params.m_MixGreen);
            m_Material.SetVector(ID_MixBlue, _params.m_MixBlue);
        }
    }
}