
using System;
using UnityEngine;
namespace Rendering
{
    public abstract class AImageEffectBase
    {
        public virtual void OnImageProcess(RenderTexture src, RenderTexture dst) { }
        public abstract void DoValidate();
        public virtual void OnDestroy() { }
    }
    public class ImageEffectBase<T>:AImageEffectBase where T:ImageEffectParamBase
    {
        public Material m_Material { get; private set; }
        protected virtual string m_ShaderLocation => "Override This Please";
        Func<T> GetParamsFunc;
        public T GetParams() => GetParamsFunc();
        public ImageEffectBase(Func<T> _GetParams)
        {
            Type _type=this.GetType();
            Shader _shader = Shader.Find("Hidden/" + _type.Name);

            if (_shader == null)
                throw new NullReferenceException("Invalid ImageEffect Shader Found:" + _type.Name);

            if (!_shader.isSupported)
                throw new NullReferenceException("Shader Not Supported:" + _type.Name);

            m_Material =   new Material(_shader) { name=_type.Name,hideFlags =  HideFlags.DontSave};
            GetParamsFunc = _GetParams;
        }

        public virtual void OnDestory()
        {
            if (m_Material)
                GameObject.DestroyImmediate(m_Material);

            m_Material = null;
            GetParamsFunc = null;
        }
        public override void OnImageProcess(RenderTexture src, RenderTexture dst)
        {
            if (m_Material != null)
                Graphics.Blit(src, dst, m_Material);
            else
                Graphics.Blit(src, dst);
        }

        public override void DoValidate()
        {
            T param = GetParams();
            if (param==null)
                return;
            OnValidate(GetParams());
        }
        protected virtual void OnValidate(T _params)
        {

        }
    }
    public class ImageEffect_ColorGrading : ImageEffectBase<ImageEffectParams_ColorGrading>
    {
        public ImageEffect_ColorGrading(Func< ImageEffectParams_ColorGrading> _GetParams) : base(_GetParams) {  }
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
            None=0,
            Red=1,
            Green=2,
            Blue=3,
        }
        public enum enum_LUTCellCount
        {
            _16=16,
            _32=32,
            _64=64,
            _128=128,
        }
        protected override void OnValidate(ImageEffectParams_ColorGrading _params)
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

    public class ImageEffect_Blurs : ImageEffectBase<ImageEffectParams_Blurs>
    {
        public ImageEffect_Blurs(Func<ImageEffectParams_Blurs> _GetParams) : base(_GetParams)
        { 
        
        }
        #region ShaderProperties
        const string KW_ClipAlpha = "CLIP_ZERO_ALPHA";
        static readonly int ID_BlurSize = Shader.PropertyToID("_BlurSize");
        #endregion
        public enum enum_BlurType
        {
            AverageSinglePass=0,
            Average=1,
            Gaussian=2,
        }

        public override void OnImageProcess(RenderTexture src, RenderTexture dst)
        {
            if (m_Material.passCount <= 1)
            {
                Debug.LogWarning("Invalid Material Pass Found Of Blur!");
                Graphics.Blit(src, dst);
                return;
            }

            ImageEffectParams_Blurs m_Params = GetParams();

            int rtW = src.width / m_Params.downSample;
            int rtH = src.height / m_Params.downSample;
            RenderTexture rt1 = src;

            for (int i = 0; i < m_Params.iteration; i++)
            {
                m_Material.SetFloat(ID_BlurSize, m_Params.blurSize * (1 + i));
                if (m_Params.blurType == enum_BlurType.AverageSinglePass)
                {
                    int pass = (int)m_Params.blurType;
                    if (i != m_Params.iteration - 1)
                    {
                        RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, src.format);
                        Graphics.Blit(rt1, rt2, m_Material, pass);
                        RenderTexture.ReleaseTemporary(rt1);
                        rt1 = rt2;
                        continue;
                    }
                    Graphics.Blit(rt1, dst, m_Material, (int)m_Params.blurType);
                }
                else 
                {
                    int horizontalPass = (int)(m_Params.blurType-1)*2;
                    int verticalPass = horizontalPass + 1;

                    // vertical blur
                    RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, src.format);
                    Graphics.Blit(rt1, rt2, m_Material, verticalPass);
                    RenderTexture.ReleaseTemporary(rt1);
                    rt1 = rt2;

                    if (i != m_Params.iteration - 1)
                    {
                        // horizontal blur
                        rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, src.format);
                        Graphics.Blit(rt1, rt2, m_Material, horizontalPass);
                        RenderTexture.ReleaseTemporary(rt1);
                        rt1 = rt2;
                        continue;
                    }
                    Graphics.Blit(rt1, dst, m_Material, horizontalPass);
                }
            }
            RenderTexture.ReleaseTemporary(rt1);
        }
    }

    public class ImageEffect_Bloom : ImageEffectBase<ImageEffectParams_Bloom>
    {
#region ShaderProperties
        static int ID_Threshold = Shader.PropertyToID("_Threshold");
        static int ID_Intensity = Shader.PropertyToID("_Intensity");
#endregion

        public enum enum_Pass
        {
            SampleLight=0,
            AddBloomTex=1,
            FastBloom=2,
        }

        ImageEffect_Blurs m_Blur;
        public ImageEffect_Bloom(Func<ImageEffectParams_Bloom> _GetParams, Func<ImageEffectParams_Blurs> _GetBlurParams) : base(_GetParams)
        {
            m_Blur = new ImageEffect_Blurs(_GetBlurParams);
        }
        protected override void OnValidate(ImageEffectParams_Bloom _params)
        {
            base.OnValidate(_params);

            m_Material.SetFloat(ID_Threshold, _params.threshold);
            m_Material.SetFloat(ID_Intensity, _params.intensity);

            m_Blur.DoValidate();
        }
        public override void OnDestory()
        {
            base.OnDestory();
            m_Blur.OnDestory();
        }

        public override void OnImageProcess(RenderTexture src, RenderTexture dst)
        {

            ImageEffectParams_Bloom _params = GetParams();
            if(!_params.enableBlur)
            {
                Graphics.Blit(src, dst, m_Material, (int)enum_Pass.FastBloom);
                return;
            }

            src.filterMode = FilterMode.Bilinear;
            var rtW = src.width;
            var rtH = src.height;

            // downsample
            RenderTexture rt1 = RenderTexture.GetTemporary(rtW, rtH, 0, src.format);
            rt1.filterMode = FilterMode.Bilinear;

            RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, src.format);
            rt1.filterMode = FilterMode.Bilinear;


            Graphics.Blit(src, rt1, m_Material, (int)enum_Pass.SampleLight);
            m_Blur.OnImageProcess(rt1, rt2);
            m_Material.SetTexture("_Bloom", rt2);
            Graphics.Blit(src, dst, m_Material, (int)enum_Pass.AddBloomTex);

            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
        }
    }
    public class ImageEffect_DepthOfField : ImageEffectBase<ImageEffectParams_DepthOfField>
    {
#region ShaderID
        static int ID_FocalStart = Shader.PropertyToID("_FocalStart");
        static int ID_FocalLerp = Shader.PropertyToID("_FocalLerp");
        static int ID_BlurTexture = Shader.PropertyToID("_BlurTex");
        static int ID_BlurSize = Shader.PropertyToID("_BlurSize");
        const string KW_FullDepthClip = "_FullDepthClip";
        const string KW_UseBlurDepth = "_UseBlurDepth";
#endregion

        ImageEffect_Blurs m_Blur;
        public ImageEffect_DepthOfField(Func<ImageEffectParams_DepthOfField> _GetParams, Func<ImageEffectParams_Blurs> _GetBlurParams) : base(_GetParams) { m_Blur = new ImageEffect_Blurs(_GetBlurParams); }
        protected override void OnValidate(ImageEffectParams_DepthOfField _params)
        {
            base.OnValidate(_params);

            m_Material.SetFloat(ID_FocalStart, _params.m_DOFStart);
            m_Material.SetFloat(ID_FocalLerp, _params.m_DOFLerp);
            m_Material.EnableKeyword(KW_FullDepthClip, _params.m_FullDepthClip);
            m_Material.EnableKeyword(KW_UseBlurDepth, _params.m_UseBlurDepth);
            m_Material.SetFloat(ID_BlurSize, _params.m_BlurSize);
            m_Blur.DoValidate();
        }
        public override void OnImageProcess(RenderTexture src, RenderTexture dst)
        {
            RenderTexture _tempBlurTex = RenderTexture.GetTemporary(src.width,src.height,0,src.format);
            m_Blur.OnImageProcess(src, _tempBlurTex);
            m_Material.SetTexture(ID_BlurTexture, _tempBlurTex);
            Graphics.Blit(src, dst, m_Material);
            RenderTexture.ReleaseTemporary(_tempBlurTex);
        }
        public override void OnDestory()
        {
            base.OnDestory();
            m_Blur.OnDestory();
        }

    }
}