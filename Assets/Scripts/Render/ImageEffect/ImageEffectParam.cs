using UnityEngine;

namespace Rendering
{
    public class ImageEffectParamBase
    {

    }

    [System.Serializable]
    public class ImageEffectParams_ColorGrading : ImageEffectParamBase
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
        public Vector3 m_MixRed=Vector3.zero;
        [Tooltip("绿色通道混合")]
        public Vector3 m_MixGreen = Vector3.zero;
        [Tooltip("蓝色通道混合")]
        public Vector3 m_MixBlue = Vector3.zero;
    }

    [System.Serializable]
    public class ImageEffectParams_DepthOfField:ImageEffectParamBase
    {
        [Tooltip("景深起始深度"), Range(0.01f, 1f)]
        public float m_DOFStart = 0.1f;
        [Tooltip("景深渐淡插值深度"), Range(.01f, .3f)]
        public float m_DOFLerp = .1f;
        [Tooltip("遮罩 1 深度")]
        public bool m_FullDepthClip = true;
        [Tooltip("深度取值模糊")]
        public bool m_UseBlurDepth = true;
        [Tooltip("深度取值模糊像素偏差"),Range(.25f,1.25f)]
        public float m_BlurSize = .5f;
    }

    [System.Serializable]
    public class ImageEffectParams_Blurs : ImageEffectParamBase
    {
        [Tooltip("模糊像素偏差"),Range(0.25f, 1.5f)]
        public float blurSize = 1.0f;
        [Tooltip("贴图降采样"), Range(1, 4)]
        public int downSample = 2;
        [Tooltip("迭代次数"),Range(1, 4)]
        public int iteration = 1;
        [Tooltip("模糊方式")]
        public ImageEffect_Blurs.enum_BlurType blurType = ImageEffect_Blurs.enum_BlurType.AverageSinglePass;
    }


    [System.Serializable]
    public class ImageEffectParams_Bloom : ImageEffectParamBase
    {
        [Tooltip("LDR 亮度采样阈值"), Range(0.0f, 1f)]
        public float threshold = 0.25f;
        [Tooltip("采样后增强"), Range(0.0f, 2.5f)]
        public float intensity = 0.3f;
        [Tooltip("启动贴图模糊")]
        public bool enableBlur = false;
    }
}