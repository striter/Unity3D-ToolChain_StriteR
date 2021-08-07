using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.ImageEffect
{
    public enum EStylize
    {
        Pixel=0,
        OilPaint=1,
        BilateralFilter=2,
        ObraDithering = 3,
    }
    public enum EPixelBound
    {
        None=0,
        Grid,
        Circle,
    }

    public class PostProcess_Stylize : PostProcessComponentBase<PPCore_Stylize, PPData_Stylize>
    {
        
    }
    
    [Serializable]
    public struct PPData_Stylize
    {
        [MTitle]public EStylize m_Stylize;
        [MFoldout(nameof(m_Stylize),EStylize.Pixel)] [ Range(2,20)] public int m_DownSample;
        [MFoldout(nameof(m_Stylize), EStylize.Pixel)] public EPixelBound m_PixelGrid;
        [MFoldout(nameof(m_Stylize), EStylize.Pixel)] [MFold(nameof(m_PixelGrid), EPixelBound.None)] [Range(0.01f, 0.49f)] public float m_GridWidth;
        [MFoldout(nameof(m_Stylize), EStylize.Pixel)] [MFold(nameof(m_PixelGrid), EPixelBound.None)] public Color m_PixelGridColor;
        [MFoldout(nameof(m_Stylize), EStylize.OilPaint)] [Range(1,20)]public int m_OilPaintKernel;
        [MFoldout(nameof(m_Stylize), EStylize.OilPaint)] [Range(0.1f, 5f)] public float m_OilPaintSize;
        [MFoldout(nameof(m_Stylize), EStylize.ObraDithering)] [Range(0.001f,1f)]public float m_ObraDitherScale;
        [MFoldout(nameof(m_Stylize), EStylize.ObraDithering)] [Range(0.1f,1f)]public float m_ObraDitherStrength;
        [MFoldout(nameof(m_Stylize), EStylize.ObraDithering)] public Color m_ObraDitherColor;
        [MFoldout(nameof(m_Stylize), EStylize.BilateralFilter)] [Range(0.1f, 5f)] public float m_BilaterailSize;
        [MFoldout(nameof(m_Stylize), EStylize.BilateralFilter)] [Range(0.01f, 1f)] public float m_BilateralFactor;
        public static readonly PPData_Stylize m_Default = new PPData_Stylize()
        {
            m_Stylize = EStylize.Pixel,
            m_DownSample = 7,
            m_PixelGrid = EPixelBound.None,
            m_GridWidth = .1f,
            m_PixelGridColor = Color.white.SetAlpha(.5f),
            m_OilPaintKernel = 10,
            m_OilPaintSize = 2f,
            m_ObraDitherColor = Color.yellow * .3f,
            m_ObraDitherScale = .33f,
            m_ObraDitherStrength = .5f,
            m_BilaterailSize = 5f,
            m_BilateralFactor=.5f,
        };
    }

    public class PPCore_Stylize:PostProcessCore<PPData_Stylize>
    {
        #region ShaderProperties
        static readonly int ID_PixelizeDownSample = Shader.PropertyToID("_STYLIZE_PIXEL_DOWNSAMPLE");
        static readonly RenderTargetIdentifier RT_PixelizeDownSample = new RenderTargetIdentifier(ID_PixelizeDownSample);
        static readonly string[] KW_PixelGrid = new string[] { "_PIXEL_GRID" ,"_PIXEL_CIRCLE"};
        static readonly int ID_PixelGridColor = Shader.PropertyToID("_PixelGridColor");
        static readonly int ID_PixelGridWidth = Shader.PropertyToID("_PixelGridWidth");

        static readonly int ID_OilPaintKernel = Shader.PropertyToID("_OilPaintKernel");
        static readonly int ID_OilPaintSize = Shader.PropertyToID("_OilPaintSize");

        static readonly int ID_ObraDitherScale = Shader.PropertyToID("_ObraDitherScale");
        static readonly int ID_ObraDitherStrength = Shader.PropertyToID("_ObraDitherStrength");
        static readonly int ID_ObraDitherColor = Shader.PropertyToID("_ObraDitherColor");

        static readonly int ID_BilateralSize = Shader.PropertyToID("_BilateralSize");
        static readonly int ID_BilateralFactor = Shader.PropertyToID("_BilateralFactor");
        #endregion
        public override void OnValidate(ref PPData_Stylize _params)
        {
            base.OnValidate(ref _params);
            m_Material.EnableKeywords(KW_PixelGrid, (int)_params.m_PixelGrid);
            m_Material.SetColor(ID_PixelGridColor,_params.m_PixelGridColor);
            m_Material.SetVector(ID_PixelGridWidth, new Vector2(_params.m_GridWidth, 1f - _params.m_GridWidth));
            m_Material.SetVector(ID_OilPaintKernel, new Vector2(-_params.m_OilPaintKernel / 2, _params.m_OilPaintKernel / 2 + 1));
            m_Material.SetFloat(ID_OilPaintSize, _params.m_OilPaintSize);
            m_Material.SetFloat(ID_ObraDitherScale, _params.m_ObraDitherScale);
            m_Material.SetFloat(ID_ObraDitherStrength, _params.m_ObraDitherStrength);
            m_Material.SetColor(ID_ObraDitherColor, _params.m_ObraDitherColor);
            m_Material.SetFloat(ID_BilateralSize, _params.m_BilaterailSize);
            m_Material.SetFloat(ID_BilateralFactor, _params.m_BilateralFactor);
        }
        public override void ExecutePostProcessBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _descriptor,ref PPData_Stylize _data)
        {
            switch (_data.m_Stylize)
            {
                default:
                    base.ExecutePostProcessBuffer(_buffer, _src, _dst, _descriptor,ref _data);
                    break;
                case EStylize.Pixel:
                    _buffer.GetTemporaryRT(ID_PixelizeDownSample, _descriptor.width / _data.m_DownSample, _descriptor.height / _data.m_DownSample, 0, FilterMode.Point, _descriptor.colorFormat);
                    _buffer.Blit(_src, RT_PixelizeDownSample);
                    _buffer.Blit(RT_PixelizeDownSample, _dst, m_Material, 0);
                    _buffer.ReleaseTemporaryRT(ID_PixelizeDownSample);
                    break;
                case EStylize.OilPaint:
                    _buffer.Blit(_src, _dst, m_Material, 1);
                    break;
                case EStylize.ObraDithering:
                    _buffer.Blit(_src, _dst, m_Material, 2);
                    break;
                case EStylize.BilateralFilter:
                    _buffer.Blit(_src, _dst, m_Material, 3);
                    break;
            }
        }
    }
}
